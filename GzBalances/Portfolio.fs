namespace GzBalances

module PortfolioTypes =
    open System
    open GzDb.DbUtil

    type Risk = Low | Medium | High

    type PortfolioId = int

    type Shares = decimal

    type Money = decimal

    type Portfolio = {
        PortfolioId : PortfolioId;
        Risk : Risk
    }

    [<Literal>]
    let LowRiskPortfolioId = 1
    [<Literal>]
    let MediumRiskPortfolioId = 3
    [<Literal>]
    let HighRiskPortfolioId = 5
    [<Literal>]
    let LowRiskArrayIndex = 0
    [<Literal>]
    let MediumRiskArrayIndex = 1
    [<Literal>]
    let HighRiskArrayIndex = 2

    /// Input monthly argument for calculating portfolio
    type PortfolioShares = {
        SharesLowRisk : Shares;
        SharesMediumRisk : Shares;
        SharesHighRisk : Shares;
    } with 
        static member (-) (soldShares, owned : PortfolioShares) =
            {
                SharesLowRisk = owned.SharesLowRisk - soldShares.SharesLowRisk; 
                SharesMediumRisk = owned.SharesMediumRisk - soldShares.SharesMediumRisk; 
                SharesHighRisk = owned.SharesHighRisk - soldShares.SharesHighRisk;
            }
        static member (+) (owned, soldShares : PortfolioShares) =
            {
                SharesLowRisk = owned.SharesLowRisk + soldShares.SharesLowRisk; 
                SharesMediumRisk = owned.SharesMediumRisk + soldShares.SharesMediumRisk; 
                SharesHighRisk = owned.SharesHighRisk + soldShares.SharesHighRisk;
            }

    type PortfolioPriced = {
        PortfolioShares : PortfolioShares;
        Worth : Money
    }

    type VintagesSold = {
        PortfolioShares : PortfolioShares; 
        BoughtAt : Money; 
        SoldAt : Money 
    }

    type PortfolioSharePrice = {
        PortfolioId : PortfolioId;
        Price : float;
        TradedOn : DateTime;
    }

    type PortfoliosPrices = {
        PortfolioLowRiskPrice : PortfolioSharePrice;
        PortfolioMediumRiskPrice : PortfolioSharePrice;
        PortfolioHighRiskPrice : PortfolioSharePrice;
    }

    type PortfoliosPricesMap = Map<string, PortfoliosPrices>

    type DbUserMonth = {
        Db : DbContext;
        UserId : int;
        Month : string;
    }

    type UserPortfolioShares = {
        PrevPortfolioShares : PortfolioPriced;
        NewPortfolioShares : PortfolioPriced
    }

    type UserPortfolioInput = {
        DbUserMonth : DbUserMonth;
        VintagesSold : VintagesSold;
        Portfolio : Portfolio;
        CashToInvest : Money;
        PortfoliosPrices : PortfoliosPrices
    }

    type UserFinance = {
        BegBalance : Money;
        Deposits : Money;
        Withdrawals : Money;
        GainLoss : Money;
        EndBalance : Money;
        AmountToBuyStock : Money;
    }

    type FundQuote = { Symbol : string; TradedOn : DateTime; ClosingPrice : float }
    type PortfolioWeight = float32
    type PortfolioFundRecord = {PortfolioId : PortfolioId; PortfolioWeight : PortfolioWeight; Fund : FundQuote}

    let getPortfolioRiskById (portfolioId : PortfolioId) : Risk =
        match portfolioId with
        | LowRiskPortfolioId -> Low
        | MediumRiskPortfolioId -> Medium
        | HighRiskPortfolioId -> High
        | _ -> invalidArg "Portfolio Id" (sprintf "Unknown portfolio id: %d" portfolioId)

    let getPortfolioIdByRisk (portfolioRisk : Risk) : PortfolioId =
        match portfolioRisk with
        | Low -> LowRiskPortfolioId
        | Medium -> MediumRiskPortfolioId
        | High -> HighRiskPortfolioId

module DailyPortfolioShares =
    open System
    open System.Net
    open GzCommon
    open GzDb.DbUtil
    open PortfolioTypes

    // URL of a service that generates price data
    [<Literal>]
    let YahooFinanceUrl = "http://ichart.finance.yahoo.com/table.csv?s="

    /// Returns prices (as YQ type) of a given stock for a 
    /// specified number of days on ascending order of trading time
    let getStockPrices stock count =
        // Download the data and split it into lines
        let wc = new WebClient()
        let data = wc.DownloadString(YahooFinanceUrl + stock)
        let dataLines = 
            data.Split([| '\n' |], StringSplitOptions.RemoveEmptyEntries) 

        // Parse lines of the CSV file and take specified
        // number of days using in the oldest to newest order
        seq { for line in dataLines |> Seq.skip 1 do
                  let infos = line.Split(',')
                  let tradedOn = 
                    match DateTime.TryParseExact(infos.[0], "yyyy-MM-dd", null, Globalization.DateTimeStyles.None) with
                    | (true, dt) -> dt
                    | (false, _) -> invalidArg "Yahoo Finance Api Date" (sprintf "Couldn't parse this yahoo api date %s" infos.[0])
                  let yQuote = { Symbol = stock; TradedOn = tradedOn; ClosingPrice = float infos.[4]}
                  yield yQuote }
        |> Seq.take count |> Seq.rev

    /// insert a new portfolio price for a trading day and price
    let private insDbNewRowPortfolioPrice (db : DbContext)(portfolioPrices : PortfoliosPrices) =
        let newPortfolioPriceRow = new DbPortfolioPrices(
                                        PortfolioLowPrice = portfolioPrices.PortfolioLowRiskPrice.Price,
                                        PortfolioMediumPrice = portfolioPrices.PortfolioMediumRiskPrice.Price,
                                        PortfolioHighPrice = portfolioPrices.PortfolioHighRiskPrice.Price,
                                        YearMonthDay = portfolioPrices.PortfolioLowRiskPrice.TradedOn.ToYyyyMmDd,
                                        UpdatedOnUtc = DateTime.UtcNow
                                    )

        db.PortfolioPrices.InsertOnSubmit(newPortfolioPriceRow)
        
    let private getPortfolioPriceList (db : DbContext)(tradingDay : string) : DbPortfolioPrices =
        query {
            for row in db.PortfolioPrices do
            where (row.YearMonthDay = tradingDay)
            select row
            exactlyOneOrDefault
        }

    let private insDbPortfolioPrices (db : DbContext)(portfoliosPrices : PortfoliosPrices) : unit =

        (db, portfoliosPrices) ||> insDbNewRowPortfolioPrice
        db.DataContext.SubmitChanges()

    /// Insert the portfolio prices for a trading day if not existing already
    let private setDbAskToSavePortfolioPrices(db : DbContext)(portfoliosPrices : PortfoliosPrices) : unit =

        // Any portfolio risk type trading day is the same
        let portfolioPricesRow = (db, portfoliosPrices.PortfolioLowRiskPrice.TradedOn.ToYyyyMmDd) ||> getPortfolioPriceList

        if isNull portfolioPricesRow then

            (db, portfoliosPrices) ||> insDbPortfolioPrices

    /// Save all trading days portfolio prices
    let private setDbPortfoliosPrices(db : DbContext)(portfoliosPrices : PortfoliosPricesMap) : PortfoliosPricesMap=
        portfoliosPrices 
        |> Map.iter (fun key value -> (db, value) ||> setDbAskToSavePortfolioPrices)
        portfoliosPrices

    /// Cast an array of 3 risk portfolios to PortfoliosPrices
    let private portfolioPricesArrToType(portfolioPricesArr : PortfolioSharePrice[]) : PortfoliosPrices =

        // Low : 0, Medium, High
        (portfolioPricesArr.Length = 3, "Incoming Portfolio prices array is not 3 (Low, Medium, High)")
        ||> traceExc

        let portfoliosPrices = { 
            PortfolioLowRiskPrice = {PortfolioId = portfolioPricesArr.[LowRiskArrayIndex].PortfolioId; Price = portfolioPricesArr.[LowRiskArrayIndex].Price; TradedOn = portfolioPricesArr.[LowRiskArrayIndex].TradedOn} 
            PortfolioMediumRiskPrice = {PortfolioId = portfolioPricesArr.[MediumRiskArrayIndex].PortfolioId; Price = portfolioPricesArr.[MediumRiskArrayIndex].Price; TradedOn = portfolioPricesArr.[MediumRiskArrayIndex].TradedOn}; 
            PortfolioHighRiskPrice = {PortfolioId = portfolioPricesArr.[HighRiskArrayIndex].PortfolioId; Price = portfolioPricesArr.[HighRiskArrayIndex].Price; TradedOn = portfolioPricesArr.[HighRiskArrayIndex].TradedOn} 
        }
        portfoliosPrices

    /// Get the portfolio funds weighted
    let private pFundsQry (db : DbContext) =
            query { 
                for f in db.Funds do
                join fp in db.PortFunds on (f.Id = fp.FundId)
                join p in db.Portfolios on (fp.PortfolioId = p.Id)
                where (p.IsActive)
                sortBy fp.PortfolioId
                thenBy f.Id
                select (f, fp)
            }

(*Return from (Dbfunds, DbPortfolioFunds) }   
Key         Value
20170213    PortfolioLowRiskPrice     PortfolioId 1
                                      Price 59.5350015
                                      TradedOn 2/13/2017 0:00
            PortfolioMediumRiskPrice  PortfolioId 3
                                      Price 65.4049995
                                      TradedOn 2/13/2017 0:00
            PortfolioHighRiskPrice    PortfolioId 5
                                      Price 38.8099995
                                      TradedOn 2/13/2017 0:00
20170214    PortfolioLowRiskPrice     PortfolioId 1
                                      Price 59.385001
                                      TradedOn 2/14/2017 0:00
            PortfolioMediumRiskPrice  PortfolioId 3
                                      Price 65.615001
                                      TradedOn 2/14/2017 0:00
            PortfolioHighRiskPrice    PortfolioId 5
                                      Price 38.8670005
                                      TradedOn 2/14/2017 0:00*)
    let private getPortfoliosPrices (takeNdays: int) 
                            (dbFundsWithPortfolioFunds : (DbFunds * DbPortfolioFunds) Linq.IQueryable)
                            : PortfoliosPricesMap =

        let portfolioPrices =
            dbFundsWithPortfolioFunds
            |> Seq.collect(fun (f : DbFunds, fp : DbPortfolioFunds) -> 
                takeNdays 
                |> getStockPrices f.Symbol
                |> Seq.map(fun(quote : FundQuote) -> 
                    let portfolioFundRec = {PortfolioId = fp.PortfolioId; PortfolioWeight = fp.Weight; Fund = quote}
                    portfolioFundRec)
                )
                |> Seq.groupBy(fun (portfolioFundRec : PortfolioFundRecord) -> portfolioFundRec.Fund.TradedOn)
                |> Seq.map(fun (tradedOnKey : DateTime, spfr : PortfolioFundRecord seq) -> 
                    let gpfr = spfr |> Seq.groupBy (fun pfr -> pfr.PortfolioId)
                    let pricedPortfoliosGroup = 
                        gpfr
                        |> Seq.map (fun (p, spsf) ->
                            let price = 
                                spsf 
                                |> Seq.sumBy (fun pfr -> pfr.Fund.ClosingPrice * float pfr.PortfolioWeight / 100.0) 
                            let topPortfolioFundRecord = spsf |> Seq.head
                            { PortfolioId = p; Price = price; TradedOn = topPortfolioFundRecord.Fund.TradedOn }
                        )
                    (tradedOnKey.ToYyyyMmDd, pricedPortfoliosGroup 
                                                |> Seq.toArray
                                                |> portfolioPricesArrToType)
                )
                |> Map
        portfolioPrices 
        // portfolioPrices |> Seq.iter(fun i -> printfn "%A" i)

    /// Ask & Store portfolio shares
    let storeShares (db : DbContext)(takeNdays : int) : PortfoliosPricesMap =
        db
        |> pFundsQry
        |> getPortfoliosPrices takeNdays
        |> setDbPortfoliosPrices db

    /// 1. Read The portfolio funds from Db
    /// 2. Ask yahoo their closing prices
    /// 3. Calculate each portfolio single virtual share closing price
    let getPortfolioPrices(db : DbContext) : PortfolioSharePrice[] =

        let portfolioPricesArr =
            query { 
                for f in db.Funds do
                join fp in db.PortFunds on (f.Id = fp.FundId)
                join p in db.Portfolios on (fp.PortfolioId = p.Id)
                where (p.IsActive)
                sortBy fp.PortfolioId
                thenBy f.Id
                select (f, fp)
            }
            // Ask yahoo api funds closing pricing
            |> Seq.map(fun (f : DbFunds, fp : DbPortfolioFunds) -> 
                let quote = getStockPrices f.Symbol 1 |> Seq.head
                let portfolioFundRec = {PortfolioId = fp.PortfolioId; PortfolioWeight = fp.Weight; Fund = quote}
                portfolioFundRec)
            // Group per portfolio
            |> Seq.groupBy (fun (portfolioFundRec : PortfolioFundRecord) -> portfolioFundRec.PortfolioId)
            // Flatten the seq<fundquote> to a single price per portfolioId group
            |> Seq.map (
                fun (p : PortfolioId, spsf : (seq<PortfolioFundRecord>)) ->
                    let price = 
                        spsf 
                        |> Seq.map (fun (pfr : PortfolioFundRecord) -> pfr.Fund.ClosingPrice * float pfr.PortfolioWeight / 100.0) 
                        |> Seq.reduce (+)
                    let topPortfolioFundRecord = spsf |> Seq.head
                    { PortfolioId = p; Price = price; TradedOn = topPortfolioFundRecord.Fund.TradedOn }
            )
            |> Seq.toArray
        // Need for refactoring if the following fails
        assert (portfolioPricesArr.Length = 3)
        // portfolioPricesArr |> Seq.iter(fun i -> printfn "%A" i)
        portfolioPricesArr

module UserPortfolio =
    open System
    open GzDb
    open GzDb.DbUtil
    open PortfolioTypes
        
    (*** Default Portfolio for new Users ***)
    let defaultPortfolio (db : DbContext) =  
        let gzConf = query {
            for gzConf in db.GzConfigurations do
            select gzConf
            exactlyOne
        } 
        let portfolioRisk = gzConf.FIRST_PORTFOLIO_RISK_VAL |> getPortfolioRiskById
        { PortfolioId = gzConf.FIRST_PORTFOLIO_RISK_VAL; Risk = portfolioRisk}

    let private updDbUserPortfolio 
                (userPortfolioRisk : Risk)
                (userPortfolioRow : DbCustoPortfolios) =

        let portfolioId = userPortfolioRisk |> getPortfolioIdByRisk 
        (**** Hard coded to 100% for now *****)
        userPortfolioRow.Weight <- 100.00F
        userPortfolioRow.PortfolioId <- portfolioId
        userPortfolioRow.UpdatedOnUTC <- DateTime.UtcNow

    let private insDbUserPortfolio (userInputPortfolio : UserPortfolioInput) : unit =
        let newCustomerPortfolioRow = new DbCustoPortfolios(
                                        CustomerId = userInputPortfolio.DbUserMonth.UserId,
                                        YearMonth = userInputPortfolio.DbUserMonth.Month
                                    )
        (userInputPortfolio.Portfolio.Risk, newCustomerPortfolioRow) ||> updDbUserPortfolio
        userInputPortfolio.DbUserMonth.Db.CustPortfolios.InsertOnSubmit(newCustomerPortfolioRow)

    /// get single Customer Portfolio of the desired month param in the yyyyMM format
    let private getUserPortfolio (dbUserMonth : DbUserMonth) : DbCustoPortfolios= 
        let userPortfolio =
            query {
                for userPortfolioRow in dbUserMonth.Db.CustPortfolios do
                where (
                    String.Compare(userPortfolioRow.YearMonth, dbUserMonth.Month, StringComparison.Ordinal) <= 0
                    && userPortfolioRow.CustomerId = dbUserMonth.UserId
                )
                sortByDescending userPortfolioRow.YearMonth
                select userPortfolioRow
                headOrDefault
            }
        userPortfolio

    /// get a Customer Portfolio joined with portfolio of the desired month param in the yyyyMM format
    let getUserPortfolioDetail (dbUserMonth : DbUserMonth) : Portfolio = 
        query {
            for userPortfolioRow in dbUserMonth.Db.CustPortfolios do
            join portfolioRow in dbUserMonth.Db.Portfolios on (userPortfolioRow.PortfolioId = portfolioRow.Id)
            where (
                String.Compare(userPortfolioRow.YearMonth, dbUserMonth.Month, StringComparison.Ordinal) <= 0
                && userPortfolioRow.CustomerId = dbUserMonth.UserId
            )
            sortByDescending userPortfolioRow.YearMonth
            select (portfolioRow)
            headOrDefault
        } 
        |> (fun(portfolioRow : DbPortfolios) ->
                if not <| isNull portfolioRow then
                    {PortfolioId = portfolioRow.Id; Risk = portfolioRow.RiskTolerance |> getPortfolioRiskById}
                else
                    dbUserMonth.Db |> defaultPortfolio
           )

    /// Upsert a customer portfolio
    let upsDbUserPortfolio(userPortfolioInput : UserPortfolioInput) : unit =
        let currentCustomerPortfolio = userPortfolioInput.DbUserMonth |> getUserPortfolio

        if isNull currentCustomerPortfolio then
            userPortfolioInput |> insDbUserPortfolio 
        else
            (userPortfolioInput.Portfolio.Risk, currentCustomerPortfolio) ||> updDbUserPortfolio

        userPortfolioInput.DbUserMonth.Db.DataContext.SubmitChanges()

module CalcUserPortfolioShares =
    open GzDb
    open PortfolioTypes
        
    let private priceLowRiskPortfolio (userPortfolioInput : UserPortfolioInput) : PortfolioPriced =

        let lowRiskPrice = userPortfolioInput.PortfoliosPrices.PortfolioLowRiskPrice.Price
        {
            PortfolioShares = { 
                                SharesLowRisk = userPortfolioInput.CashToInvest / decimal lowRiskPrice;
                                SharesMediumRisk = 0M;
                                SharesHighRisk = 0M
            };
            Worth = userPortfolioInput.CashToInvest
        }

    let private priceMediumRiskPortfolio (userPortfolioInput : UserPortfolioInput) : PortfolioPriced =

        let mediumRiskPrice = userPortfolioInput.PortfoliosPrices.PortfolioMediumRiskPrice.Price
        { 
            PortfolioShares = { 
                                SharesLowRisk = 0M;
                                SharesMediumRisk = userPortfolioInput.CashToInvest / decimal mediumRiskPrice;
                                SharesHighRisk = 0M;
            };
            Worth = userPortfolioInput.CashToInvest
        }

    let private priceHighRiskPortfolio (userPortfolioInput : UserPortfolioInput) : PortfolioPriced =

        let highRiskPrice = userPortfolioInput.PortfoliosPrices.PortfolioHighRiskPrice.Price
        { 
            PortfolioShares = { 
                                SharesLowRisk = 0M;
                                SharesMediumRisk = 0M;
                                SharesHighRisk = userPortfolioInput.CashToInvest / decimal highRiskPrice
            }
            Worth = userPortfolioInput.CashToInvest
        }

    /// Get cash -> shares by looking at the investment cash amount and user portfolio
    let getNewCustomerShares (userPortfolioInput : UserPortfolioInput) : PortfolioPriced =
        match userPortfolioInput.Portfolio.Risk with
        | Low -> userPortfolioInput |> priceLowRiskPortfolio
        | Medium ->  userPortfolioInput |> priceMediumRiskPortfolio
        | High -> userPortfolioInput |> priceHighRiskPortfolio

module VintageShares =
    open System
    open GzCommon
    open GzDb.DbUtil
    open PortfolioTypes
        
    /// update a row to VintageShares
    let private updDbVintageShares
            (userPortfolioShares : PortfolioShares)
            (tradingDay : DateTime)
            (userPortfolioSharesRow : DbVintageShares) : unit =

        userPortfolioSharesRow.PortfolioLowShares <- userPortfolioShares.SharesLowRisk
        userPortfolioSharesRow.PortfolioMediumShares <- userPortfolioShares.SharesMediumRisk
        userPortfolioSharesRow.PortfolioHighShares <- userPortfolioShares.SharesHighRisk
        userPortfolioSharesRow.BuyPortfolioTradeDay <- tradingDay
        userPortfolioSharesRow.UpdatedOnUtc <- DateTime.UtcNow

    /// insert a row to VintageShares
    let private insDbVintageShares 
                (userPortfolioShares : PortfolioShares)
                (tradingDay : DateTime)
                (dbUserMonth : DbUserMonth) : unit =

        let newCustPortfoliosSharesRow = new DbVintageShares
                                            (UserId = dbUserMonth.UserId, YearMonth = dbUserMonth.Month)
        (userPortfolioShares, tradingDay, newCustPortfoliosSharesRow) |||> updDbVintageShares

        dbUserMonth.Db.VintageShares.InsertOnSubmit(newCustPortfoliosSharesRow)

    /// get CustomerPortfolioShares of the desired month param in the yyyyMM format  
    let private getDbVintageSharesRow(dbUserMonth : DbUserMonth) : DbVintageShares = 
        let vintageShares =
            query {
                for row in dbUserMonth.Db.VintageShares do
                where (
                    row.YearMonth = dbUserMonth.Month
                    && row.UserId = dbUserMonth.UserId
                )
                select row
                exactlyOneOrDefault
            }
        vintageShares

    /// upsert user portfolio shares for the desired month
    let upsDbVintageShares
            (dbUserMonth : DbUserMonth)
            (tradingDay : DateTime)
            (userPortfolioShares : PortfolioShares) : unit =

        let dbUserSharesRow = dbUserMonth |> getDbVintageSharesRow

        if isNull dbUserSharesRow then
            (userPortfolioShares, tradingDay, dbUserMonth) |||> insDbVintageShares
        else
            (userPortfolioShares, tradingDay, dbUserSharesRow) |||> updDbVintageShares

        dbUserMonth.Db.DataContext.SubmitChanges()

    /// Price portfolio shares
    let private getPricedPortfolioShares (shares : PortfolioShares)(prices :PortfoliosPrices) : PortfolioPriced=
        { 
            PortfolioShares = shares; 
            Worth = shares.SharesLowRisk * decimal prices.PortfolioLowRiskPrice.Price 
                + shares.SharesMediumRisk * decimal prices.PortfolioMediumRiskPrice.Price 
                + shares.SharesHighRisk * decimal prices.PortfolioHighRiskPrice.Price
        }

    /// Price a db row of portfolio shares
    let private getDbPortfolioShares
                (portfolioSharesRow : DbVintageShares) : PortfolioShares =
        if not <| isNull portfolioSharesRow then
            { 
                SharesLowRisk = portfolioSharesRow.PortfolioLowShares;
                SharesMediumRisk = portfolioSharesRow.PortfolioMediumShares;
                SharesHighRisk = portfolioSharesRow.PortfolioHighShares;
            }
        else // Default of 0 shares 
            {SharesLowRisk = 0M; SharesMediumRisk = 0M; SharesHighRisk = 0M}

    /// Price portfolio shares
    let private getPricedDbPortfolioShares
                (portfoliosPrices :PortfoliosPrices) 
                (portfolioSharesRow : PortfolioShares) : PortfolioPriced =
            (portfolioSharesRow, portfoliosPrices) ||> getPricedPortfolioShares

    /// get the previous month's priced with latest prices and adjusted by vintages sold of present month
    let getPricedPrevMonthShares(userPortfolioInput : UserPortfolioInput) : PortfolioPriced =
        let prevMonth = { userPortfolioInput.DbUserMonth with Month = userPortfolioInput.DbUserMonth.Month.ToPrevYyyyMm }
        prevMonth   |> getDbVintageSharesRow
                    |> getDbPortfolioShares 
                    |> (-) userPortfolioInput.VintagesSold.PortfolioShares
                    |> getPricedDbPortfolioShares userPortfolioInput.PortfoliosPrices

module InvBalance =
    open System
    open GzCommon
    open GzDb.DbUtil
    open PortfolioTypes

    type InvBalancePrevTotals = {
        TotalCashInvestments : decimal;
        TotalCashInvInHold : decimal;
        TotalSoldVintagesSold : decimal
    }

    type InvBalanceInput = {
        InvBalancePrevTotals : InvBalancePrevTotals;
        UserPortfolioShares : UserPortfolioShares;
        UserInputPortfolio : UserPortfolioInput;
        UserFinance : UserFinance
    }

    /// get the invBalance row of the desired month in the format of yyyyMm
    let private getInvBalance (dbUserMonth : DbUserMonth) : DbInvBalances = 
        let invBalance =
            query {
                for row in dbUserMonth.Db.InvBalances do
                where (
                    row.YearMonth = dbUserMonth.Month
                    && row.CustomerId = dbUserMonth.UserId
                )
                select row
                exactlyOneOrDefault
            }
        invBalance

    /// update an invBalance row
    let private updDbInvBalance(input : InvBalanceInput)(invBalanceRow : DbInvBalances) : unit =

        let prevShares = input.UserPortfolioShares.PrevPortfolioShares
        let newShares = input.UserPortfolioShares.NewPortfolioShares
        
        let nowBalance = 
            prevShares.Worth 
            + newShares.Worth

        let cashInv = input.UserInputPortfolio.CashToInvest

        let vintagesSoldThisMonth = input.UserInputPortfolio.VintagesSold

        let ``total Cash invested for All vintages`` = 
            input.InvBalancePrevTotals.TotalCashInvestments 
            + cashInv

        let ``selling Price Of All Sold Vintages`` = 
            input.InvBalancePrevTotals.TotalSoldVintagesSold 
            + vintagesSoldThisMonth.SoldAt

        let ``bought Price of Sold Vintages`` = vintagesSoldThisMonth.BoughtAt

        let ``total Cash Invested for Unsold Vintages`` = 
            input.InvBalancePrevTotals.TotalCashInvInHold 
            + cashInv
            - ``bought Price of Sold Vintages``

        invBalanceRow.Balance <- nowBalance
        // Note: See line below for investment gain including sold vintages
        // invBalanceRow.InvGainLoss <- (nowBalance + ``selling Price Of All Sold Vintages``) - ``total Cash invested for All vintages``
        // Present business choice: Investment gain of vintages in hold. 
        invBalanceRow.InvGainLoss <- nowBalance - ``total Cash Invested for Unsold Vintages``
        invBalanceRow.PortfolioId <- input.UserInputPortfolio.Portfolio.PortfolioId
        invBalanceRow.CashInvestment <- cashInv

        // Vintage Shares
        invBalanceRow.LowRiskShares <- newShares.PortfolioShares.SharesLowRisk
        invBalanceRow.MediumRiskShares <- newShares.PortfolioShares.SharesMediumRisk
        invBalanceRow.HighRiskShares <- newShares.PortfolioShares.SharesHighRisk

        // Totals
        invBalanceRow.TotalCashInvestments <- ``total Cash invested for All vintages``
        invBalanceRow.TotalCashInvInHold <- ``total Cash Invested for Unsold Vintages``
        invBalanceRow.TotalSoldVintagesValue <- ``selling Price Of All Sold Vintages``

        // Gaming Activity
        invBalanceRow.BegGmBalance <- input.UserFinance.BegBalance
        invBalanceRow.Deposits <- input.UserFinance.Deposits
        invBalanceRow.Withdrawals <- input.UserFinance.Withdrawals
        invBalanceRow.GamingGainLoss <- input.UserFinance.GainLoss
        invBalanceRow.EndGmBalance <- input.UserFinance.EndBalance

        invBalanceRow.UpdatedOnUTC <- DateTime.UtcNow

    /// insert a InvBalance row
    let private insDbInvBalance (input : InvBalanceInput) : unit =
        let newInvBalanceRow = 
            new DbInvBalances(
                CustomerId = input.UserInputPortfolio.DbUserMonth.UserId, 
                YearMonth = input.UserInputPortfolio.DbUserMonth.Month
            )
        (input, newInvBalanceRow) ||> updDbInvBalance

        input.UserInputPortfolio.DbUserMonth.Db.InvBalances.InsertOnSubmit(newInvBalanceRow)

    /// upsert a InvBalance row
    let upsDbInvBalance (input : InvBalanceInput) : unit =
        let invBalanceRow = input.UserInputPortfolio.DbUserMonth |> getInvBalance

        if isNull invBalanceRow then
            input |> insDbInvBalance
        else
            (input, invBalanceRow) ||> updDbInvBalance

        input.UserInputPortfolio.DbUserMonth.Db.DataContext.SubmitChanges()

    /// get db invBalance totals of previous month
    let getInvBalancePrevTotals(dbUserMonth : DbUserMonth) : InvBalancePrevTotals =
        let invB = { dbUserMonth with Month = dbUserMonth.Month.ToPrevYyyyMm }
                |> getInvBalance
        if not <| isNull invB then
            { TotalCashInvestments = invB.TotalCashInvestments; TotalCashInvInHold = invB.TotalCashInvInHold; TotalSoldVintagesSold = invB.TotalSoldVintagesValue }
        else
            { TotalCashInvestments = 0M; TotalCashInvInHold = 0M; TotalSoldVintagesSold = 0M }

    /// get any sold vintages for this user during the month we're presently clearing
    let getSoldVintages (dbUserMonth : DbUserMonth) : VintagesSold = 
        let vintageTotals =
            query {
                for row in dbUserMonth.Db.InvBalances do
                where (
                    row.SoldYearMonth = dbUserMonth.Month
                    && row.CustomerId = dbUserMonth.UserId
                )
                // Cast drop nullable from SoldAmount
                select (row.LowRiskShares, row.MediumRiskShares, row.HighRiskShares, row.CashInvestment, decimal row.SoldAmount)
            }
            // Sum fold. The query linq would not allow for Null return of grouped summed tuple so we do summation in F#
            // it's also much shorter
            |> Seq.fold (fun (ls, ms, hs, cs, ss) (l, m, h, c, s) -> (ls+l, ms+m, hs+h, cs+c, ss + s)) (0M, 0M, 0M, 0M, 0M)
        vintageTotals 
                |> (fun (lowShares, mediumShares, highShares, cashInv, soldAmount) ->
                    let vintShares = {
                        SharesLowRisk = lowShares;
                        SharesMediumRisk = mediumShares;
                        SharesHighRisk = highShares
                    }
                    { PortfolioShares = vintShares; BoughtAt = cashInv; SoldAt = soldAmount }
                )

    /// get instance of input values for InvBalance processing
    let getInvBalanceInput (userPortfolioInput : UserPortfolioInput)(userFinance : UserFinance) : InvBalanceInput = 
        let portfolioShares = { 
            PrevPortfolioShares = userPortfolioInput |> VintageShares.getPricedPrevMonthShares;
            NewPortfolioShares = userPortfolioInput |> CalcUserPortfolioShares.getNewCustomerShares
        }
        { 
            InvBalancePrevTotals =  userPortfolioInput.DbUserMonth |> getInvBalancePrevTotals;
            UserPortfolioShares = portfolioShares;
            UserInputPortfolio = userPortfolioInput;
            UserFinance = userFinance
        }

module UserTrx =
    open GzCommon
    open GzDb.DbUtil
    open PortfolioTypes
    open InvBalance
    open System.Collections.Generic
    open System
    open NLog
    let logger = LogManager.GetCurrentClassLogger()

    /// Upsert UserPortfolio, VintageShares, InvBalance
    let private upsDbClearMonth (userPortfolioInput : UserPortfolioInput)(userFinance:UserFinance) : unit =
        let invBalanceInput = (userPortfolioInput, userFinance) ||> getInvBalanceInput

        // Ups CustPortfolios
        userPortfolioInput |> UserPortfolio.upsDbUserPortfolio
        
        // Ups VintageShares
        let buyTradingDay = userPortfolioInput.PortfoliosPrices.PortfolioLowRiskPrice.TradedOn
        (
            invBalanceInput.UserPortfolioShares.PrevPortfolioShares.PortfolioShares 
            + invBalanceInput.UserPortfolioShares.NewPortfolioShares.PortfolioShares
        ) 
        |> VintageShares.upsDbVintageShares userPortfolioInput.DbUserMonth buyTradingDay
        
        // Ups InvBalance
        invBalanceInput 
        |> InvBalance.upsDbInvBalance

    /// Process input balances
    let private processUserBalances (userPortfolioInput : UserPortfolioInput)(userFinance:UserFinance): unit = 
        if userPortfolioInput.CashToInvest > 0M || userFinance.EndBalance > 0M then
            logger.Info(sprintf "Processing investment balances for user id %d on month of %s having financial amounts of %A" 
                userPortfolioInput.DbUserMonth.UserId userPortfolioInput.DbUserMonth.Month userFinance)

        let dbOper() = (userPortfolioInput, userFinance) ||> upsDbClearMonth
        (userPortfolioInput.DbUserMonth.Db, dbOper) ||> tryDBCommit3Times

    /// get the portfolio market quote that's latest within the month processing
    let findNearestPortfolioPrice (portfoliosPrices:PortfoliosPricesMap)(month : string) =
        let nextMonth = month.ToNextMonth1st
        let monthLateQuote = 
            portfoliosPrices
            |> Map.filter(fun key _ -> key < nextMonth)
            |> Seq.maxBy(fun kvp -> kvp.Key)
            |> (fun (kvp : KeyValuePair<string, PortfoliosPrices>) -> kvp.Value)
        // assert quote is within the month
        (monthLateQuote.PortfolioHighRiskPrice.TradedOn.Month = Int32.Parse(month.Substring(4, 2)), "Found a portfolio market quote not within the processing month: " + month)
        ||> traceExc
        monthLateQuote
        
    /// Input type for portfolio processing
    let private getUserPortfolioInput (dbUserMonth : DbUserMonth)(trxRow : DbGzTrx)(portfoliosPricesMap:PortfoliosPricesMap) =
        { 
            DbUserMonth = dbUserMonth;
            VintagesSold = dbUserMonth |> InvBalance.getSoldVintages;
            Portfolio = dbUserMonth |> UserPortfolio.getUserPortfolioDetail;
            CashToInvest = trxRow.Amount;
            PortfoliosPrices = (portfoliosPricesMap, dbUserMonth.Month) ||> findNearestPortfolioPrice
        }

    /// Input type for gaming activities reporting
    let private getUserFinance (trxRow : DbGzTrx): UserFinance =
        {
            BegBalance = trxRow.BegGmBalance.Value;
            EndBalance = trxRow.EndGmBalance.Value;
            Deposits = trxRow.Deposits.Value;
            Withdrawals = trxRow.Withdrawals.Value;
            GainLoss = trxRow.GainLoss.Value;
            AmountToBuyStock = trxRow.Amount
        }

    /// Main entry to process credit losses for the month and put forth balance amounts from gaming activities
    let processGzTrx(db : DbContext)(yyyyMmDd : string)(portfoliosPrices : PortfoliosPricesMap) =

        let yyyyMm = yyyyMmDd.Substring(0, 6)
        query { 
            for trxRow in db.GzTrxs do
                where (
                    trxRow.YearMonthCtd = yyyyMm
                    && trxRow.GzTrxTypes.Code = int GzTransactionType.CreditedPlayingLoss
                )
                sortBy trxRow.CustomerId
                select trxRow
        }
        |> Seq.iter (fun (trxRow : DbGzTrx) ->
                    // set input types
                    let dbUserMonth = {Db = db; UserId = trxRow.CustomerId; Month = yyyyMm}
                    let userPortfolioInput = (dbUserMonth, trxRow, portfoliosPrices) |||> getUserPortfolioInput
                    let userFinance = trxRow |> getUserFinance
                    (userPortfolioInput, userFinance) ||> processUserBalances
                )

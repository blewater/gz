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

    (*** Default Portfolio for new Users ***)
    let defaultPortfolio = { PortfolioId = MediumRiskPortfolioId; Risk = Medium}

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
        EndBalance : Money
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
                                        PortfolioLowPrice = float32 portfolioPrices.PortfolioLowRiskPrice.Price,
                                        PortfolioMediumPrice = float32 portfolioPrices.PortfolioMediumRiskPrice.Price,
                                        PortfolioHighPrice = float32 portfolioPrices.PortfolioHighRiskPrice.Price
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

    /// Insert the portfolio prices for a trading day
    let private setDbPortfolioPrices(db : DbContext)(portfoliosPrices : PortfoliosPrices) : unit =

        // Any portfolio risk type trading day is the same
        let portfolioPricesRow = (db, portfoliosPrices.PortfolioLowRiskPrice.TradedOn.ToYyyyMmDd) ||> getPortfolioPriceList

        if not <| isNull portfolioPricesRow then

            (db, portfoliosPrices) ||> insDbPortfolioPrices

    let private portfolioPricesToType(portfolioPricesArr : PortfolioSharePrice[]) : PortfoliosPrices =
        let portfoliosPrices = { 
            PortfolioLowRiskPrice = {PortfolioId = portfolioPricesArr.[0].PortfolioId; Price = portfolioPricesArr.[0].Price; TradedOn = portfolioPricesArr.[0].TradedOn} 
            PortfolioMediumRiskPrice = {PortfolioId = portfolioPricesArr.[1].PortfolioId; Price = portfolioPricesArr.[1].Price; TradedOn = portfolioPricesArr.[1].TradedOn}; 
            PortfolioHighRiskPrice = {PortfolioId = portfolioPricesArr.[1].PortfolioId; Price = portfolioPricesArr.[2].Price; TradedOn = portfolioPricesArr.[2].TradedOn} 
        }
        portfoliosPrices

    /// 1. Read The portfolio funds from Db
    /// 2. Ask yahoo their closing prices
    /// 3. Calculate each portfolio single virtual share closing price
    let getPortfolioPrices(db : DbContext) : PortfolioSharePrice[] =

        let portfolioPricesArr =
            query { 
                for f in db.Funds do
                join fp in db.PortFunds on (f.Id = fp.FundId)
                sortBy fp.PortfolioId
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
                    defaultPortfolio
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
        
    let private updDbVintageShares
            (userPortfolioShares : PortfolioShares)
            (userPortfolioSharesRow : DbVintageShares) : unit =

        userPortfolioSharesRow.PortfolioLowShares <- userPortfolioShares.SharesLowRisk
        userPortfolioSharesRow.PortfolioMediumShares <- userPortfolioShares.SharesMediumRisk
        userPortfolioSharesRow.PortfolioHighShares <- userPortfolioShares.SharesHighRisk
        userPortfolioSharesRow.UpdatedOnUtc <- DateTime.UtcNow

    let private insDbVintageShares 
                (userPortfolioShares : PortfolioShares)
                (dbUserMonth : DbUserMonth) : unit =

        let newCustPortfoliosSharesRow = new DbVintageShares
                                            (UserId = dbUserMonth.UserId, YearMonth = dbUserMonth.Month)
        (userPortfolioShares, newCustPortfoliosSharesRow) ||> updDbVintageShares

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
    let upsDbVintageShares(dbUserMonth : DbUserMonth)(userPortfolioShares : PortfolioShares) : unit =
        let dbUserSharesRow = dbUserMonth |> getDbVintageSharesRow

        if isNull dbUserSharesRow then
            (userPortfolioShares, dbUserMonth) ||> insDbVintageShares
        else
            (userPortfolioShares, dbUserSharesRow) ||> updDbVintageShares

        dbUserMonth.Db.DataContext.SubmitChanges()

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

    /// Price a db row of portfolio shares priced
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

    let private updDbInvBalance(input : InvBalanceInput)(invBalanceRow : DbInvBalances) : unit =

        let prevShares = input.UserPortfolioShares.PrevPortfolioShares
        let newShares = input.UserPortfolioShares.NewPortfolioShares
        let nowBalance = prevShares.Worth + newShares.Worth
        let cashInv = input.UserInputPortfolio.CashToInvest
        let vintagesSoldThisMonth = input.UserInputPortfolio.VintagesSold
        let ``total Cash invested for All vintages`` = input.InvBalancePrevTotals.TotalCashInvestments + cashInv
        let ``selling Price Of All Sold Vintages`` = input.InvBalancePrevTotals.TotalSoldVintagesSold + vintagesSoldThisMonth.SoldAt
        let ``bought Price of Sold Vintages`` = vintagesSoldThisMonth.BoughtAt
        let ``total Cash Invested for Unsold Vintages`` = input.InvBalancePrevTotals.TotalCashInvInHold - ``bought Price of Sold Vintages``

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

    let private insDbInvBalance (input : InvBalanceInput) : unit =
        let newInvBalanceRow = 
            new DbInvBalances(
                CustomerId = input.UserInputPortfolio.DbUserMonth.UserId, 
                YearMonth = input.UserInputPortfolio.DbUserMonth.Month
            )
        (input, newInvBalanceRow) ||> updDbInvBalance

        input.UserInputPortfolio.DbUserMonth.Db.InvBalances.InsertOnSubmit(newInvBalanceRow)

    let upsDbInvBalance (input : InvBalanceInput) : unit =
        let invBalanceRow = input.UserInputPortfolio.DbUserMonth |> getInvBalance

        if isNull invBalanceRow then
            input |> insDbInvBalance
        else
            (input, invBalanceRow) ||> updDbInvBalance

        input.UserInputPortfolio.DbUserMonth.Db.DataContext.SubmitChanges()

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
            // it's also shorter much shorter
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
    open GzDb
    open GzDb.DbUtil
    open PortfolioTypes
    open InvBalance

    /// Upsert UserPortfolio, VintageShares, InvBalance
    let upsDbClearMonth (userPortfolioInput : UserPortfolioInput)(userFinance:UserFinance) : unit =
        let invBalanceInput = (userPortfolioInput, userFinance) ||> getInvBalanceInput

        userPortfolioInput |> UserPortfolio.upsDbUserPortfolio
        (
            invBalanceInput.UserPortfolioShares.PrevPortfolioShares.PortfolioShares 
            + invBalanceInput.UserPortfolioShares.NewPortfolioShares.PortfolioShares
        ) 
            |> VintageShares.upsDbVintageShares userPortfolioInput.DbUserMonth
        invBalanceInput |> InvBalance.upsDbInvBalance


    let private processUser (userPortfolioInput : UserPortfolioInput)(userFinance:UserFinance): unit = 
        let dbOper() = (userPortfolioInput, userFinance) ||> upsDbClearMonth
        (userPortfolioInput.DbUserMonth.Db, dbOper) ||> tryDBCommit3Times

    let private getUserPortfolioInput (dbUserMonth : DbUserMonth)(trxRow : DbGzTrx)(portfoliosPrices:PortfoliosPrices) =
        { 
            DbUserMonth = dbUserMonth;
            VintagesSold = dbUserMonth |> InvBalance.getSoldVintages;
            Portfolio = dbUserMonth |> UserPortfolio.getUserPortfolioDetail;
            CashToInvest = trxRow.Amount;
            PortfoliosPrices = portfoliosPrices
        }
    let private getUserFinance (trxRow : DbGzTrx): UserFinance =
        {
            BegBalance = trxRow.BegGmBalance.Value;
            EndBalance = trxRow.EndGmBalance.Value;
            Deposits = trxRow.Deposits.Value;
            Withdrawals = trxRow.Withdrawals.Value;
            GainLoss = trxRow.GainLoss.Value
        }

    let processGzTrx(db : DbContext)(yyyyMm : string)(portfoliosPrices :PortfoliosPrices) =

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
                    let dbUserMonth = {Db = db; UserId = trxRow.CustomerId; Month = yyyyMm}
                    let userPortfolioInput = (dbUserMonth, trxRow, portfoliosPrices) |||> getUserPortfolioInput
                    let userFinance = trxRow |> getUserFinance
                    (userPortfolioInput, userFinance) ||> processUser
                )

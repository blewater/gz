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

    type VintagesSold = PortfolioPriced

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

    type TrxInput = {
        Db : DbContext;
        UserId : int;
        Month : string;
    }

    type UserInputPortfolio = {
        TrxInput : TrxInput;
        VintagesSold : VintagesSold;
        Portfolio : Portfolio;
        CashToInvest : decimal;
        PortfoliosPrices : PortfoliosPrices;
        BegBalance : decimal;
        Deposits : decimal;
        Withdrawals : decimal;
        GainLoss : decimal;
        EndBalance : decimal
    }

    type InvBalance = {
        CustomerPortfolio : PortfolioShares;
        PortfolioId : PortfolioId;
        YearMonth : string;
        Balance : Decimal;
        LastUpdated : DateTime;
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
    open GzDb
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

    let private insDbUserPortfolio (userInputPortfolio : UserInputPortfolio) : unit =
        let newCustomerPortfolioRow = new DbCustoPortfolios(
                                        CustomerId = userInputPortfolio.TrxInput.UserId,
                                        YearMonth = userInputPortfolio.TrxInput.Month
                                    )
        (userInputPortfolio.Portfolio.Risk, newCustomerPortfolioRow) ||> updDbUserPortfolio
        userInputPortfolio.TrxInput.Db.CustPortfolios.InsertOnSubmit(newCustomerPortfolioRow)
        

    /// get single Customer Portfolio of the desired month param in the yyyyMM format
    let private getUserPortfolio (trxInput : TrxInput) : DbCustoPortfolios= 
        let userPortfolio =
            query {
                for userPortfolioRow in trxInput.Db.CustPortfolios do
                where (
                    String.Compare(userPortfolioRow.YearMonth, trxInput.Month, StringComparison.Ordinal) <= 0
                    && userPortfolioRow.CustomerId = trxInput.UserId
                )
                sortByDescending userPortfolioRow.YearMonth
                select userPortfolioRow
                headOrDefault
            }
        userPortfolio

    /// get a Customer Portfolio joined with portfolio of the desired month param in the yyyyMM format
    let getUserPortfolioDetail (trxInput : TrxInput) : Portfolio = 
        query {
            for userPortfolioRow in trxInput.Db.CustPortfolios do
            join portfolioRow in trxInput.Db.Portfolios on (userPortfolioRow.PortfolioId = portfolioRow.Id)
            where (
                String.Compare(userPortfolioRow.YearMonth, trxInput.Month, StringComparison.Ordinal) <= 0
                && userPortfolioRow.CustomerId = trxInput.UserId
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
    let upsUserPortfolio(userInputPortfolio : UserInputPortfolio) : unit =
        let currentCustomerPortfolio = userInputPortfolio.TrxInput |> getUserPortfolio

        if isNull currentCustomerPortfolio then
            userInputPortfolio |> insDbUserPortfolio 
        else
            (userInputPortfolio.Portfolio.Risk, currentCustomerPortfolio) ||> updDbUserPortfolio

        userInputPortfolio.TrxInput.Db.DataContext.SubmitChanges()

module CalcUserPortfolioShares =
    open GzDb
    open PortfolioTypes
        
    let private priceLowRiskPortfolio (userInputPortfolio : UserInputPortfolio) : PortfolioPriced =

        let lowRiskPrice = userInputPortfolio.PortfoliosPrices.PortfolioLowRiskPrice.Price
        {
            PortfolioShares = { 
                                SharesLowRisk = userInputPortfolio.CashToInvest / decimal lowRiskPrice;
                                SharesMediumRisk = 0M;
                                SharesHighRisk = 0M
            };
            Worth = userInputPortfolio.CashToInvest
        }

    let private priceMediumRiskPortfolio (userInputPortfolio : UserInputPortfolio) : PortfolioPriced =

        let mediumRiskPrice = userInputPortfolio.PortfoliosPrices.PortfolioMediumRiskPrice.Price
        { 
            PortfolioShares = { 
                                SharesLowRisk = 0M;
                                SharesMediumRisk = userInputPortfolio.CashToInvest / decimal mediumRiskPrice;
                                SharesHighRisk = 0M;
            };
            Worth = userInputPortfolio.CashToInvest
        }

    let private priceHighRiskPortfolio (userInputPortfolio : UserInputPortfolio) : PortfolioPriced =

        let highRiskPrice = userInputPortfolio.PortfoliosPrices.PortfolioHighRiskPrice.Price
        { 
            PortfolioShares = { 
                                SharesLowRisk = 0M;
                                SharesMediumRisk = 0M;
                                SharesHighRisk = userInputPortfolio.CashToInvest / decimal highRiskPrice
            }
            Worth = userInputPortfolio.CashToInvest
        }

    /// Get cash -> shares by looking at the investment cash amount and user portfolio
    let getNewCustomerShares (userInputPortfolio : UserInputPortfolio) : PortfolioPriced =
        match userInputPortfolio.Portfolio.Risk with
        | Low -> userInputPortfolio |> priceLowRiskPortfolio
        | Medium ->  userInputPortfolio |> priceMediumRiskPortfolio
        | High -> userInputPortfolio |> priceHighRiskPortfolio

module VintageShares =
    open System
    open GzDb
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
                (trxInput : TrxInput) : unit =

        let newCustPortfoliosSharesRow = new DbVintageShares
                                            (UserId = trxInput.UserId, YearMonth = trxInput.Month)
        (userPortfolioShares, newCustPortfoliosSharesRow) ||> updDbVintageShares

        trxInput.Db.VintageShares.InsertOnSubmit(newCustPortfoliosSharesRow)

    /// get CustomerPortfolioShares of the desired month param in the yyyyMM format  
    let private getDbVintageSharesRow(trxInput : TrxInput) : DbVintageShares = 
        let vintageShares =
            query {
                for row in trxInput.Db.VintageShares do
                where (
                    row.YearMonth = trxInput.Month
                    && row.UserId = trxInput.UserId
                )
                select row
                exactlyOneOrDefault
            }
        vintageShares

    /// upsert user portfolio shares for the desired month
    let upsDbVintageShares(trxInput : TrxInput)(userPortfolioShares : PortfolioShares) : unit =
        let dbUserSharesRow = trxInput |> getDbVintageSharesRow

        if isNull dbUserSharesRow then
            (userPortfolioShares, trxInput) ||> insDbVintageShares
        else
            (userPortfolioShares, dbUserSharesRow) ||> updDbVintageShares

        trxInput.Db.DataContext.SubmitChanges()

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
    let getPricedPrevMonthShares(userInputPortfolio : UserInputPortfolio) : PortfolioPriced =
        let prevMonth = { userInputPortfolio.TrxInput with Month = userInputPortfolio.TrxInput.Month.ToPrevYyyyMm }
        prevMonth   |> getDbVintageSharesRow
                    |> getDbPortfolioShares 
                    |> (-) userInputPortfolio.VintagesSold.PortfolioShares
                    |> getPricedDbPortfolioShares userInputPortfolio.PortfoliosPrices

module InvBalance =
    open System
    open GzDb
    open GzDb.DbUtil
    open PortfolioTypes

    type InvBalancePrevTotals = {
        TotalCashInvestments : decimal;
        TotalSoldVintagesSold : decimal;
    }

    type InvBalanceInput = {
        InvBalancePrevTotals : InvBalancePrevTotals;
        PrevPortfolioShares : PortfolioPriced;
        NewPortfolioShares : PortfolioPriced;
        UserInputPortfolio : UserInputPortfolio
    }

    /// get the invBalance row of the desired month in the format of yyyyMm
    let private getInvBalance (trxInput : TrxInput) : DbInvBalances = 
        let invBalance =
            query {
                for row in trxInput.Db.InvBalances do
                where (
                    row.YearMonth = trxInput.Month
                    && row.CustomerId = trxInput.UserId
                )
                select row
                exactlyOneOrDefault
            }
        invBalance

    let private updDbInvBalance(input : InvBalanceInput)(invBalanceRow : DbInvBalances) : unit =

        let nowBalance = input.PrevPortfolioShares.Worth + input.NewPortfolioShares.Worth
        let cashInv = input.UserInputPortfolio.CashToInvest
        let totalCashInvestment = input.InvBalancePrevTotals.TotalCashInvestments + cashInv
        let vintagesSoldThisMonth = input.UserInputPortfolio.VintagesSold
        let totalSoldVintagesSold = input.InvBalancePrevTotals.TotalSoldVintagesSold + vintagesSoldThisMonth.Worth

        invBalanceRow.Balance <- nowBalance
        invBalanceRow.InvGainLoss <- nowBalance - totalCashInvestment + totalSoldVintagesSold
        invBalanceRow.PortfolioId <- input.UserInputPortfolio.Portfolio.PortfolioId
        invBalanceRow.CashInvestment <- cashInv
        invBalanceRow.TotalCashInvestments <- totalCashInvestment
        invBalanceRow.TotalSoldVintagesValue <- totalSoldVintagesSold
        invBalanceRow.LowRiskShares <- input.NewPortfolioShares.PortfolioShares.SharesLowRisk
        invBalanceRow.MediumRiskShares <- input.NewPortfolioShares.PortfolioShares.SharesMediumRisk
        invBalanceRow.HighRiskShares <- input.NewPortfolioShares.PortfolioShares.SharesHighRisk

        invBalanceRow.BegGmBalance <- input.UserInputPortfolio.BegBalance
        invBalanceRow.Deposits <- input.UserInputPortfolio.Deposits
        invBalanceRow.Withdrawals <- input.UserInputPortfolio.Withdrawals
        invBalanceRow.GamingGainLoss <- input.UserInputPortfolio.GainLoss
        invBalanceRow.EndGmBalance <- input.UserInputPortfolio.EndBalance

        invBalanceRow.UpdatedOnUTC <- DateTime.UtcNow

    let private insDbInvBalance (input : InvBalanceInput) : unit =
        let newInvBalanceRow = 
            new DbInvBalances(
                CustomerId = input.UserInputPortfolio.TrxInput.UserId, 
                YearMonth = input.UserInputPortfolio.TrxInput.Month
            )
        (input, newInvBalanceRow) ||> updDbInvBalance

        input.UserInputPortfolio.TrxInput.Db.InvBalances.InsertOnSubmit(newInvBalanceRow)

    let upsDbInvBalance (input : InvBalanceInput) : unit =
        let invBalanceRow = input.UserInputPortfolio.TrxInput |> getInvBalance

        if isNull invBalanceRow then
            input |> insDbInvBalance
        else
            (input, invBalanceRow) ||> updDbInvBalance

        input.UserInputPortfolio.TrxInput.Db.DataContext.SubmitChanges()

    let getInvBalancePrevTotals(trxInput : TrxInput) =
        let invB = { trxInput with Month = trxInput.Month.ToPrevYyyyMm }
                |> getInvBalance
        if not <| isNull invB then
            { TotalCashInvestments = invB.TotalCashInvestments; TotalSoldVintagesSold = invB.TotalSoldVintagesValue }
        else
            { TotalCashInvestments = 0M; TotalSoldVintagesSold = 0M }

    let getPrevInvBalanceTotals(trxInput : TrxInput) : InvBalancePrevTotals =
        let prevMonth = { trxInput with Month = trxInput.Month.ToPrevYyyyMm }
        let invBalanceRow = prevMonth |> getInvBalance
        if not <| isNull invBalanceRow then
            { TotalCashInvestments = 0M; TotalSoldVintagesSold = 0M; }
        else
            { TotalCashInvestments = 0M; TotalSoldVintagesSold = 0M; }
        
    /// get any sold vintages for this user during the month we're presently clearing
    let getSoldVintages (trxInput : TrxInput) = 
        let vintageTotals =
            query {
                for row in trxInput.Db.InvBalances do
                where (
                    row.SoldYearMonth = trxInput.Month
                    && row.CustomerId = trxInput.UserId
                )
                // Cast drop nullable from SoldAmount
                select (row.LowRiskShares, row.MediumRiskShares, row.HighRiskShares, decimal row.SoldAmount)
            }
            // Sum fold. The query linq would not allow for Null return of grouped summed tuple so we do summation in F#
            // it's also shorter much shorter
            |> Seq.fold (fun (ls, ms, hs, ss) (l, m, h, s) -> (ls+l, ms+m, hs+h, ss + s)) (0M, 0M, 0M,0M)
        vintageTotals 
                |> (fun (lowShares, mediumShares, highShares, soldAmount) ->
                    let vintShares = {
                        SharesLowRisk = lowShares;
                        SharesMediumRisk = mediumShares;
                        SharesHighRisk = highShares
                    }
                    { PortfolioShares = vintShares; Worth = soldAmount }
                )

module UserTrx =
    open GzDb
    open GzDb.DbUtil
    open PortfolioTypes
    open InvBalance

    let private processUser(userInputPortfolio : UserInputPortfolio)(portfoliosPrices :PortfoliosPrices) : unit = 
        let prevPortfolioShares = userInputPortfolio |> VintageShares.getPricedPrevMonthShares
        let newPortfolioShares = userInputPortfolio |> CalcUserPortfolioShares.getNewCustomerShares

        let invBalanceInput =
            { 
                InvBalancePrevTotals =  userInputPortfolio.TrxInput |> getInvBalancePrevTotals;
                PrevPortfolioShares = prevPortfolioShares;
                NewPortfolioShares = newPortfolioShares;   
                UserInputPortfolio = userInputPortfolio
            }

        // Upserts
        userInputPortfolio |> UserPortfolio.upsUserPortfolio
        (prevPortfolioShares.PortfolioShares + newPortfolioShares.PortfolioShares) 
            |> VintageShares.upsDbVintageShares userInputPortfolio.TrxInput
        invBalanceInput |> InvBalance.upsDbInvBalance

        //userInputPortfolio.TrxInput tryDbTransOperation

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
                        let trxInput = {Db = db; UserId = trxRow.CustomerId; Month = yyyyMm}
                        let userInputPortfoliio = { 
                            TrxInput = trxInput;
                            VintagesSold = trxInput |> InvBalance.getSoldVintages;
                            Portfolio = trxInput |> UserPortfolio.getUserPortfolioDetail;
                            CashToInvest = trxRow.Amount;
                            PortfoliosPrices = portfoliosPrices;
                            BegBalance = trxRow.BegGmBalance.Value;
                            EndBalance = trxRow.EndGmBalance.Value;
                            Deposits = trxRow.Deposits.Value;
                            Withdrawals = trxRow.Withdrawals.Value;
                            GainLoss = trxRow.GainLoss.Value
                        }
                        (userInputPortfoliio, portfoliosPrices) ||> processUser
                    )

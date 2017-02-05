namespace GzBalances

module PortfolioTypes =
    open System
    open GzDb.DbUtil

    type Risk = Low | Medium | High

    type PortfolioId = int

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
    type UserPortfolioShares = {
        SharesLowRisk : Decimal;
        SharesMediumRisk : Decimal;
        SharesHighRisk : Decimal;
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

    type TrxInput = {
        Db : DbContext;
        UserId : int;
        Month : string;
    }

    type UserInputPortfolio = {
        TrxInput : TrxInput;
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
        CustomerPortfolio : UserPortfolioShares;
        PortfolioId : PortfolioId;
        YearMonth : string;
        Balance : Decimal;
        LastUpdated : DateTime;
    }

    type Vintages = InvBalance list

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
    let private insDbNewRowPortfolioPrice (db : DbContext)(portfolioShare : PortfolioSharePrice) =
        let newPortfolioPriceRow = new DbPortfolioPrices(
                                        PortfolioId = portfolioShare.PortfolioId,
                                        YearMonthDay = portfolioShare.TradedOn.ToYyyyMmDd,
                                        Price = float32 portfolioShare.Price
                                    )

        db.PortfolioPrices.InsertOnSubmit(newPortfolioPriceRow)

    /// De-construct the seq of portfolio shares to get the trading day in yyyyMMdd format
    let getTradingDay (portfolioShareSeq : PortfolioSharePrice seq) : string= 
        let {PortfolioId = _; Price = _; TradedOn = TradedOn} = portfolioShareSeq |> (Seq.head)
        TradedOn.ToYyyyMmDd
        
    let getPortfolioPriceList (db : DbContext)(tradingDay : string) : DbPortfolioPrices list=
        query {
            for row in db.PortfolioPrices do
            where (row.YearMonthDay = tradingDay)
            select row
        } 
        |> Seq.toList

    let private insDbPortfolioPrices (db : DbContext)(portfolioShareSeq : PortfolioSharePrice seq) : unit =

            portfolioShareSeq 
            |> Seq.iter (fun (portfolioShare : PortfolioSharePrice) -> 
                (db, portfolioShare) ||> insDbNewRowPortfolioPrice

                db.DataContext.SubmitChanges()
            )

    /// Insert the portfolio prices for a trading day
    let setDbPortfolioPrices(db : DbContext)(portfolioShareSeq : PortfolioSharePrice seq) : unit =

        let portfolioPriceList = (db, portfolioShareSeq |> getTradingDay) ||> getPortfolioPriceList

        if portfolioPriceList.Length = 0 then

            /// Box function pointer with required parameters
            let dbOperation() = (db, portfolioShareSeq) ||> insDbPortfolioPrices
            (db, dbOperation) ||> tryDbTransOperation

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

module CalcUserPortfolioShares  =
    open GzDb
    open PortfolioTypes
        
    let private cash2PortfolioShares (existingShares : decimal)(cashAmount : decimal)(sharePrice : float) : decimal = 
        let newShares = (cashAmount / decimal sharePrice) + existingShares
        newShares

    let private priceLowRiskPortfolio
            (userInputPortfolio : UserInputPortfolio)
            (userPortfolioShares : UserPortfolioShares) =

        let lowRiskPrice = userInputPortfolio.PortfoliosPrices.PortfolioLowRiskPrice.Price
        let sharesLowRisk = (userPortfolioShares.SharesLowRisk, userInputPortfolio.CashToInvest, lowRiskPrice) |||> cash2PortfolioShares 
        { 
            SharesLowRisk = sharesLowRisk; 
            SharesMediumRisk = userPortfolioShares.SharesMediumRisk; 
            SharesHighRisk = userPortfolioShares.SharesHighRisk
        }

    let private priceMediumRiskPortfolio
            (userInputPortfolio : UserInputPortfolio)
            (userPortfolioShares : UserPortfolioShares) =

        let mediumRiskPrice = userInputPortfolio.PortfoliosPrices.PortfolioMediumRiskPrice.Price
        let sharesMediumRisk = (userPortfolioShares.SharesMediumRisk, userInputPortfolio.CashToInvest, mediumRiskPrice) |||> cash2PortfolioShares 
        { 
            SharesLowRisk = userPortfolioShares.SharesLowRisk; 
            SharesMediumRisk = sharesMediumRisk; 
            SharesHighRisk = userPortfolioShares.SharesHighRisk
        }

    let private priceHighRiskPortfolio
            (userInputPortfolio : UserInputPortfolio)
            (userPortfolioShares : UserPortfolioShares) =

        let highRiskPrice = userInputPortfolio.PortfoliosPrices.PortfolioHighRiskPrice.Price
        let sharesHighRisk = (userPortfolioShares.SharesHighRisk, userInputPortfolio.CashToInvest, highRiskPrice) |||> cash2PortfolioShares
        { 
            SharesLowRisk = userPortfolioShares.SharesLowRisk; 
            SharesMediumRisk = userPortfolioShares.SharesMediumRisk; 
            SharesHighRisk = sharesHighRisk
        }

    let getNewCustomerShares
            (userInputPortfolio : UserInputPortfolio)
            (prevMonthUserPortfolioShares : UserPortfolioShares) : UserPortfolioShares =

        let inputPortfolio = (userInputPortfolio, prevMonthUserPortfolioShares)

        let newPortfolioShares =
            match userInputPortfolio.Portfolio.Risk with
            | Low -> inputPortfolio ||> priceLowRiskPortfolio
            | Medium ->  inputPortfolio ||> priceMediumRiskPortfolio
            | High -> inputPortfolio ||> priceHighRiskPortfolio

        newPortfolioShares

module CustomerPortfolio =
    open System
    open GzDb
    open GzDb.DbUtil
    open PortfolioTypes
        
    let private updDbCustomerPortfolio 
                (userPortfolioRisk : Risk)
                (userPortfolioRow : DbCustoPortfolios) =

        let portfolioId = userPortfolioRisk |> getPortfolioIdByRisk 
        (**** Hard coded to 100% for now *****)
        userPortfolioRow.Weight <- 100.00F
        userPortfolioRow.PortfolioId <- portfolioId
        userPortfolioRow.UpdatedOnUTC <- DateTime.UtcNow

    let private insDbCustomerPortfolio (userInputPortfolio : UserInputPortfolio) : unit =
        let newCustomerPortfolioRow = new DbCustoPortfolios(
                                        CustomerId = userInputPortfolio.TrxInput.UserId,
                                        YearMonth = userInputPortfolio.TrxInput.Month
                                    )
        (userInputPortfolio.Portfolio.Risk, newCustomerPortfolioRow) ||> updDbCustomerPortfolio
        userInputPortfolio.TrxInput.Db.CustPortfolios.InsertOnSubmit(newCustomerPortfolioRow)
        

    /// get single Customer Portfolio of the desired month param in the yyyyMM format
    let private getCustomerPortfolio (trxInput : TrxInput) : DbCustoPortfolios= 
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
    let getCustomerPortfolioDetail (trxInput : TrxInput) : Portfolio = 
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
    let upsCustomerPortfolio(userInputPortfolio : UserInputPortfolio) : unit =
        let currentCustomerPortfolio = userInputPortfolio.TrxInput |> getCustomerPortfolio

        if isNull currentCustomerPortfolio then
            userInputPortfolio |> insDbCustomerPortfolio 
        else
            (userInputPortfolio.Portfolio.Risk, currentCustomerPortfolio) ||> updDbCustomerPortfolio

        userInputPortfolio.TrxInput.Db.DataContext.SubmitChanges()

module CustomerPortfolioShares =
    open System
    open GzDb
    open GzDb.DbUtil
    open PortfolioTypes
        
    let private updDbCustomerPortfolioShares
            (userPortfolioShares : UserPortfolioShares)
            (userPortfolioSharesRow : DbCustPortfolioShares) : unit =

        userPortfolioSharesRow.PortfolioLowShares <- userPortfolioShares.SharesLowRisk
        userPortfolioSharesRow.PortfolioMediumShares <- userPortfolioShares.SharesMediumRisk
        userPortfolioSharesRow.PortfolioHighShares <- userPortfolioShares.SharesHighRisk
        userPortfolioSharesRow.UpdatedOnUtc <- DateTime.UtcNow

    let private insDbCustomerPortfolioShares 
                (userPortfolioShares : UserPortfolioShares)
                (trxInput : TrxInput) : unit =

        let newCustPortfoliosSharesRow = new DbCustPortfolioShares
                                            (CustomerId = trxInput.UserId, YearMonth = trxInput.Month)
        (userPortfolioShares, newCustPortfoliosSharesRow) ||> updDbCustomerPortfolioShares

        trxInput.Db.CustPortfoliosShares.InsertOnSubmit(newCustPortfoliosSharesRow)

    /// get CustomerPortfolioShares of the desired month param in the yyyyMM format  
    let private getCurrentCustomerPortfolioShares(trxInput : TrxInput) : DbCustPortfolioShares = 
        let userPortfolioShares =
            query {
                for row in trxInput.Db.CustPortfoliosShares do
                where (
                    row.YearMonth = trxInput.Month
                    && row.CustomerId = trxInput.UserId
                )
                select row
                exactlyOneOrDefault
            }
        userPortfolioShares

    let prevMonthUserPortfolioShares(trxInput : TrxInput) : UserPortfolioShares =
        let prevMonthPortfolioSharesRow = trxInput |> getCurrentCustomerPortfolioShares
        if not <| isNull prevMonthPortfolioSharesRow then
            { 
                SharesLowRisk = prevMonthPortfolioSharesRow.PortfolioLowShares;
                SharesMediumRisk = prevMonthPortfolioSharesRow.PortfolioMediumShares;
                SharesHighRisk = prevMonthPortfolioSharesRow.PortfolioHighShares
            }
        else // Default of 0 shares 
            {SharesLowRisk = 0M; SharesMediumRisk = 0M; SharesHighRisk = 0M}

    let upsDbCustomerPortfolioShares(trxInput : TrxInput)(userPortfolioShares : UserPortfolioShares) : unit =
        let dbUserSharesRow = trxInput |> getCurrentCustomerPortfolioShares

        if isNull dbUserSharesRow then
            (userPortfolioShares, trxInput) ||> insDbCustomerPortfolioShares
        else
            (userPortfolioShares, dbUserSharesRow) ||> updDbCustomerPortfolioShares

        trxInput.Db.DataContext.SubmitChanges()

module InvBalance =
    open System
    open GzDb
    open GzDb.DbUtil
    open PortfolioTypes

    type GzTrxGainLoss = {
        CustomerId : int;
        GainLossAmount : decimal;
        BegBalance : decimal;
        EndBalance : decimal;
        Deposits : decimal;
        Withdrawals : decimal;
        YearMonth : string;
    }

    /// get the invBalance row of the desired month in the format of yyyyMm
    let getInvBalance (trxInput : TrxInput) : DbInvBalances = 
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

    /// Calculate the InvBalance shares for the month
    let getInvBalanceShares
            (userInputPortfolio : UserInputPortfolio)
            (userPortfolioShares : UserPortfolioShares) =


        let prevMonInvBalanceRow = userInputPortfolio.TrxInput |> getInvBalance
        let prevBalance = prevMonInvBalanceRow.Balance
        let userNewShares = (userInputPortfolio, userPortfolioShares) ||> CalcUserPortfolioShares.getNewCustomerShares

        userNewShares


    let private updDbInvBalance
            (userPortfolioShares : UserPortfolioShares)
            (invBalanceRow : DbInvBalances) : unit =

        invBalanceRow.UpdatedOnUTC <- DateTime.UtcNow

    let private insDbInvBalance
                (userInputPortfolio : UserInputPortfolio)(userPortfolioShares : UserPortfolioShares) : unit =

        let newInvBalanceRow = 
            new DbInvBalances(
                CustomerId = userInputPortfolio.TrxInput.UserId, 
                YearMonth = userInputPortfolio.TrxInput.Month
            )
        (userPortfolioShares, newInvBalanceRow) ||> updDbInvBalance

        userInputPortfolio.TrxInput.Db.InvBalances.InsertOnSubmit(newInvBalanceRow)
        
    let upsDbInvBalance(userInputPortfolio : UserInputPortfolio)(userPortfolioShares : UserPortfolioShares) : unit =
        let invBalanceRow = userInputPortfolio.TrxInput |> getInvBalance
//        let currBalance = prevMonInvBalanceRow.Balance

        if isNull invBalanceRow then
            (userInputPortfolio, userPortfolioShares) ||> insDbInvBalance
        else
            (userPortfolioShares, invBalanceRow) ||> updDbInvBalance

        userInputPortfolio.TrxInput.Db.DataContext.SubmitChanges()

module UserTrx =
    open GzDb
    open GzDb.DbUtil
    open PortfolioTypes

    let processUser(userInputPortfolio : UserInputPortfolio) : unit = 
        let prevMonthPortfolioShares = userInputPortfolio.TrxInput
                                        |> CustomerPortfolioShares.prevMonthUserPortfolioShares
        let userPortfolioShares = (userInputPortfolio, prevMonthPortfolioShares) 
                                        ||> CalcUserPortfolioShares.getNewCustomerShares
        (userInputPortfolio, userPortfolioShares) ||> InvBalance.upsDbInvBalance
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
                        let begBalance = trxRow.BegGmBalance.Value
                        let endBalance = trxRow.EndGmBalance.Value
                        let deposits = trxRow.Deposits.Value
                        let withdrawals = trxRow.Withdrawals.Value
                        let gainLoss = begBalance + deposits - withdrawals - endBalance
                        let userInputPortfoliio = { 
                            TrxInput = trxInput;
                            Portfolio = trxInput |> CustomerPortfolio.getCustomerPortfolioDetail;
                            CashToInvest = trxRow.Amount;
                            PortfoliosPrices = portfoliosPrices;
                            BegBalance = begBalance;
                            EndBalance = endBalance;
                            Deposits = deposits;
                            Withdrawals = withdrawals;
                            GainLoss = gainLoss;
                        }
                        userInputPortfoliio |> processUser
                    )



//        .iter (fun trxRow -> 
//            let customerGzTrx = { 
//                CustomerId = trxRow.CustomerId; 
//                GailLossAmount = trxRow.Amount; 
//                BegBalance = trxRow.BegGmBalance;
//                EndBalance = trxRow.EndGmBalance;
//                Deposits = trxRow.Deposits;
//                Withdrawals = trxRow.Withdrawals;
//                YearMonth = trxRow.YearMonthCtd
//            }

    // Read from GzTrx
    // Read from CustPortfolioShare
    // Read from CustPortfolios
    // Calc shares save ->
    //    InvBalances
    //    CustPortfolioShare
    //    CustPortfolios

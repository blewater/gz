namespace GzBalances

module Portfolio =
    open System

    type Risk = Low | Medium | High

    type PortfolioId = int

    type PortfolioRisk = {
        PortfolioId : PortfolioId;
        Risk : Risk
    }

    [<Literal>]
    let LowRiskPortfolioId = 1
    [<Literal>]
    let MediumRiskPortfolioId = 3
    [<Literal>]
    let HighRiskPortfolioId = 5

    type CustomerPortfolioShares = {
        SharesLowRisk : Decimal;
        SharesMediumRisk : Decimal;
        SharesHighRisk : Decimal;
    }

    type InvBalance = {
        CustomerPortfolio : CustomerPortfolioShares;
        PortfolioId : PortfolioId;
        YearMonth : string;
        Balance : Decimal;
        LastUpdated : DateTime;
    }

    type Vintages = InvBalance list

    type FundQuote = { Symbol : string; TradedOn : DateTime; ClosingPrice : float }
    type PortfolioWeight = float32
    type PortfolioFundRecord = {PortfolioId : PortfolioId; PortfolioWeight : PortfolioWeight; Fund : FundQuote}
    type PortfolioSharePrice = {
        PortfolioId : PortfolioId;
        Price : float;
        TradedOn : DateTime;
    }

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
    open Portfolio

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
     

    /// 1. Read The portfolio funds from Db
    /// 2. Ask yahoo their closing prices
    /// 3. Calculate each portfolio single virtual share closing price
    let getPortfolioPrices(db : DbContext) : PortfolioSharePrice list =

        let portfolioPricesList =
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
            |> Seq.toList
        // Need for refactoring if the following fails
        assert (portfolioPricesList.Length = 3)
        // portfolioPricesList |> Seq.iter(fun i -> printfn "%A" i)
        portfolioPricesList

module CalcUserPortfolioShares  =
    open GzDb
    open GzDb.DbUtil
    open Portfolio
        
    let private getPortfolioPricesListIdxbyPortfolioId = 
        function
            | Low -> 0
            | Medium -> 1
            | High -> 2

    let private cash2PortfolioShares (existingShares : decimal)(cashAmount : decimal)(sharePrice : float) : decimal = 
        let newShares = (cashAmount / decimal sharePrice) + existingShares
        newShares

    let private priceLowRiskPortfolio
            (cashAmount : decimal)
            (portfolioPricesList : PortfolioSharePrice list)
            (custPortfolioShares : DbCustPortfolioShares) =

        let lowRiskPrice = portfolioPricesList.[Low |> getPortfolioPricesListIdxbyPortfolioId].Price
        let sharesLowRisk = (custPortfolioShares.PortfolioLowShares, cashAmount, lowRiskPrice) |||> cash2PortfolioShares 
        { 
            SharesLowRisk = sharesLowRisk; 
            SharesMediumRisk = custPortfolioShares.PortfolioMediumShares; 
            SharesHighRisk = custPortfolioShares.PortfolioHighShares
        }

    let private priceMediumRiskPortfolio
            (cashAmount : decimal)
            (portfolioPricesList : PortfolioSharePrice list)
            (custPortfolioShares : DbCustPortfolioShares) =

        let mediumRiskPrice = portfolioPricesList.[Medium |> getPortfolioPricesListIdxbyPortfolioId].Price
        let sharesMediumRisk = (custPortfolioShares.PortfolioLowShares, cashAmount, mediumRiskPrice) |||> cash2PortfolioShares 
        { 
            SharesLowRisk = custPortfolioShares.PortfolioLowShares; 
            SharesMediumRisk = sharesMediumRisk; 
            SharesHighRisk = custPortfolioShares.PortfolioHighShares
        }

    let private priceHighRiskPortfolio
            (cashAmount : decimal)
            (portfolioPricesList : PortfolioSharePrice list)
            (custPortfolioShares : DbCustPortfolioShares) =

        let highRiskPrice = portfolioPricesList.[High |> getPortfolioPricesListIdxbyPortfolioId].Price
        let sharesHighRisk = (custPortfolioShares.PortfolioHighShares, cashAmount, highRiskPrice) |||> cash2PortfolioShares
        { 
            SharesLowRisk = custPortfolioShares.PortfolioLowShares; 
            SharesMediumRisk = custPortfolioShares.PortfolioMediumShares; 
            SharesHighRisk = sharesHighRisk
        }

    let getNewCustomerShares
            (cashAmount : decimal)
            (portfolioPricesList : PortfolioSharePrice list)
            (custPortfolioShares : DbCustPortfolioShares)
            (customerPortfolio : DbCustoPortfolios) : CustomerPortfolioShares =

        let newPortfolioShares =
            match customerPortfolio.PortfolioId |> getPortfolioRiskById with
            | Low -> (cashAmount, portfolioPricesList, custPortfolioShares) |||> priceLowRiskPortfolio
            | Medium ->  (cashAmount, portfolioPricesList, custPortfolioShares) |||> priceMediumRiskPortfolio
            | High -> (cashAmount, portfolioPricesList, custPortfolioShares) |||> priceHighRiskPortfolio

        newPortfolioShares

module CustomerPortfolio =
    open System
    open GzDb
    open GzDb.DbUtil
    open Portfolio
        
    let private updDbCustomerPortfolio 
                (customerPortfolioRisk : Risk)
                (customerPortfolioRow : DbCustoPortfolios) =

        let portfolioId = customerPortfolioRisk |> getPortfolioIdByRisk 
        (**** Hard coded to 100% for now *****)
        customerPortfolioRow.Weight <- 100.00F
        customerPortfolioRow.PortfolioId <- portfolioId
        customerPortfolioRow.UpdatedOnUTC <- DateTime.UtcNow

    let private insDbCustomerPortfolio (customerPortfolioRisk : Risk)(db : DbContext)(yyyyMm : string)(customerId : int) : unit =
        let newCustomerPortfolioRow = new DbCustoPortfolios(
                                        CustomerId = customerId,
                                        YearMonth = yyyyMm
                                    )
        (customerPortfolioRisk, newCustomerPortfolioRow) ||> updDbCustomerPortfolio
        db.CustPortfolios.InsertOnSubmit(newCustomerPortfolioRow)
        

    /// get single Customer Portfolio of the desired month param in the yyyyMM format
    let private getCustomerPortfolio(db : DbContext)(yyyyMm : string)(customerId : int) : DbCustoPortfolios = 
        let custPortfolios =
            query {
                for row in db.CustPortfolios do
                where (
                    row.YearMonth <= yyyyMm
                    && row.CustomerId = customerId
                )
                sortByDescending row.YearMonth
                select row
                exactlyOneOrDefault
            }
        custPortfolios

    let upsCustomerPortfolio(db : DbContext)(yyyyMm : string)(customerId : int)(customerPortfolioRisk : Risk) : unit =
        let currentCustomerPortfolio = (db, yyyyMm, customerId) |||> getCustomerPortfolio

        if isNull currentCustomerPortfolio then
            (db, yyyyMm, customerId) |||> insDbCustomerPortfolio customerPortfolioRisk
        else
            (customerPortfolioRisk, currentCustomerPortfolio) ||> updDbCustomerPortfolio

        db.DataContext.SubmitChanges()

module CustomerPortfolioShares =
    open System
    open GzDb
    open GzDb.DbUtil
    open Portfolio
        
    let private updDbCustomerPortfolioShares
            (customerPortfolioShares : CustomerPortfolioShares)
            (customerPortfolioSharesRow : DbCustPortfolioShares) : unit =

        customerPortfolioSharesRow.PortfolioLowShares <- customerPortfolioShares.SharesLowRisk
        customerPortfolioSharesRow.PortfolioMediumShares <- customerPortfolioShares.SharesMediumRisk
        customerPortfolioSharesRow.PortfolioHighShares <- customerPortfolioShares.SharesHighRisk
        customerPortfolioSharesRow.UpdatedOnUtc <- DateTime.UtcNow

    let private insDbCustomerPortfolioShares 
                (customerPortfolioShares : CustomerPortfolioShares)
                (db : DbContext)
                (yyyyMm : string)(customerId : int) : unit =

        let newCustPortfoliosSharesRow = new DbCustPortfolioShares(CustomerId = customerId, YearMonth = yyyyMm)
        (customerPortfolioShares, newCustPortfoliosSharesRow) ||> updDbCustomerPortfolioShares

        db.CustPortfoliosShares.InsertOnSubmit(newCustPortfoliosSharesRow)
        

    /// get CustomerPortfolioShares of the desired month param in the yyyyMM format  
    let private getCurrentCustomerPortfolioShares(db : DbContext)(yyyyMm : string)(customerId : int) : DbCustPortfolioShares = 
        let customerPortfolioShares =
            query {
                for row in db.CustPortfoliosShares do
                where (
                    row.YearMonth = yyyyMm
                    && row.CustomerId = customerId
                )
                select row
                exactlyOneOrDefault
            }
        customerPortfolioShares

    let upsDbCustomerPortfolioShares(db : DbContext)(yyyyMm : string)(customerId : int)(customerPortfolioShares : CustomerPortfolioShares) : unit =
        let currentCustomerPortfolioShares = (db, yyyyMm, customerId) |||> getCurrentCustomerPortfolioShares

        if isNull currentCustomerPortfolioShares then
            (db, yyyyMm, customerId) |||> insDbCustomerPortfolioShares customerPortfolioShares
        else
            (customerPortfolioShares, currentCustomerPortfolioShares) ||> updDbCustomerPortfolioShares

        db.DataContext.SubmitChanges()

module InvBalance =
    open System
    open GzDb
    open GzDb.DbUtil
    open Portfolio

    type GzTrxGainLoss = {
        CustomerId : int;
        GainLossAmount : decimal;
        BegBalance : decimal;
        EndBalance : decimal;
        Deposits : decimal;
        Withdrawals : decimal;
        YearMonth : string;
    }

    let getGzTrx(db : DbContext)(yyyyMm : string) : DbGzTrx list=

        let customerGzTrxRows =
            query { 
                for trxRow in db.GzTrxs do
                    where (
                        trxRow.YearMonthCtd = yyyyMm
                        && trxRow.GzTrxTypes.Code = int GzTransactionType.CreditedPlayingLoss
                    )
                    sortBy trxRow.CustomerId
                    select trxRow
            }
            |> Seq.toList
        customerGzTrxRows

    /// get single Customer Portfolio of the month param
    let getInvBalance(db : DbContext)(yyyyMm : string)(customerId : int) : DbInvBalances = 
        let invBalance =
            query {
                for row in db.InvBalances do
                where (
                    row.YearMonth = yyyyMm
                    && row.CustomerId = customerId
                )
                select row
                exactlyOneOrDefault
            }
        invBalance

        
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

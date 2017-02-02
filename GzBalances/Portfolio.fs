namespace GzBalances

module Portfolio =

    open System
    open System.Net
    open GzDb
    open GzDb.DbUtil

    type Risk = Low | Medium | High

    type PortfolioRisk = {
        Id : int;
        Risk : Risk
    }

    let lowRiskPortfolioId = 1
    let mediumRiskPortfolioId = 3
    let highRiskPortfolioId = 5

    let getPortfolioRiskById (portfolioId : int) : Risk =
        match portfolioId with
        | lowRiskPortfolioId -> Low
        | mediumRiskPortfolioId -> Medium
        | highRiskPortfolioId -> High
        //| _ -> invalidArg "Portfolio Id" (sprintf "Unknown portfolio id: %d" portfolioId)

    type Portfolio = { 
        ClosingPrice : float32;
        Risk : PortfolioRisk;
    }

    type CustomerPortfolioShares = {
        SharesLowRisk : Decimal;
        SharesMediumRisk : Decimal;
        SharesHighRisk : Decimal;
    }

    type InvBalance = {
        CustomerPortfolio : CustomerPortfolioShares;
        PortfolioRisk : PortfolioRisk;
        YearMonth : string;
        Balance : Decimal;
        LastUpdated : DateTime;
    }

    type Vintages = InvBalance list

    type FundQuote = { Symbol : string; TradedOn : DateTime; ClosingPrice : float }
    type PortfolioId = int
    type PortfolioWeight = float32
    type PortfolioFundRecord = {PortfolioId : PortfolioId; PortfolioWeight : PortfolioWeight; Fund : FundQuote}
    type PortfolioSharePrice = {
        PortfolioId : PortfolioId;
        Price : float;
        TradedOn : DateTime;
    }
    type PortfoliosSharePrice = {
        LowRiskPrice : float;
        MediumRiskPrice : float;
        HighRiskPrice : float;
        TradedOn : DateTime;
    }

    // URL of a service that generates price data
    let url = "http://ichart.finance.yahoo.com/table.csv?s="

    /// Returns prices (as YQ type) of a given stock for a 
    /// specified number of days on ascending order of trading time
    let getStockPrices stock count =
        // Download the data and split it into lines
        let wc = new WebClient()
        let data = wc.DownloadString(url + stock)
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

    /// Insert the portofolio prices for a trading day
    let insDbPortfolioPrices(db : DbContext)(portfolioShareSeq : PortfolioSharePrice seq) : unit =

        let portfolioPriceList  = 
            query {
                for row in db.PortfolioPrices do
                where (row.YearMonthDay = (portfolioShareSeq |> getTradingDay))
                select row
            } 
            |> Seq.toList

        if portfolioPriceList.Length = 0 then

            portfolioShareSeq |> Seq.iter (fun (portfolioShare : PortfolioSharePrice) -> 
                (db, portfolioShare) ||> insDbNewRowPortfolioPrice
                db.DataContext.SubmitChanges()
            )

    /// 1. Read The portfolio funds from Db
    /// 2. Ask yahoo their closing prices
    /// 3. Calculate each portfolio single virtual share closing price
    let getPortfolioPrices(db : DbContext) : PortfolioSharePrice seq =

        let gSeq =
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
//        gSeq |> Seq.iter(fun i -> printfn "%A" i)
        gSeq

    let getCustomerPortfolio(db : DbContext)(yyyyMm : string)(customerId : int) : DbCustoPortfolios = 
        let custPortfolios =
            query {
                for row in db.CustPortfolios do
                where (
                    row.YearMonth < yyyyMm
                    && row.CustomerId = customerId
                )
                sortByDescending row.YearMonth
                select row
                exactlyOneOrDefault
            }
        custPortfolios
        
    let getCustomerPortShares(db : DbContext)(yyyyMm : string)(customerId : int) : DbCustPortfolioShares = 
        let customerPortfolioShares =
            query {
                for row in db.CustPortfoliosShares do
                where (
                    row.YearMonth = yyyyMm.ToPrevYyyyMm
                    && row.CustomerId = customerId
                )
                select row
                exactlyOneOrDefault
            }
        customerPortfolioShares

    let getListIdxbyPortfolioId = 
        function
            | 1 -> 0
            | 3 -> 1
            | 5 -> 2
            | _ -> -1

    let buyShares (existingShares : decimal)(cashAmount : decimal)(sharePrice : float) : decimal = 
        let newShares = (cashAmount / decimal sharePrice) + existingShares
        newShares

    let getNewCustomerShares
            (cashAmount : decimal)
            (portfolioSharePrices : PortfolioSharePrice seq)
            (custPortfolioShares : DbCustPortfolioShares)
            (customerPortfolio : DbCustoPortfolios) =

        
        let portfolioPricesList = portfolioSharePrices |> Seq.toList
        let newPortfolioShares =
            match customerPortfolio.PortfolioId |> getPortfolioRiskById with
            | Low -> 
                let lowRiskPrice = portfolioPricesList.[customerPortfolio.PortfolioId |> getListIdxbyPortfolioId].Price
                let sharesLowRisk = buyShares custPortfolioShares.PortfolioLowShares cashAmount lowRiskPrice
                { SharesLowRisk = sharesLowRisk; SharesMediumRisk = custPortfolioShares.PortfolioMediumShares; SharesHighRisk = custPortfolioShares.PortfolioHighShares}
            | Medium -> 
                let mediumRiskPrice = portfolioPricesList.[customerPortfolio.PortfolioId |> getListIdxbyPortfolioId].Price
                let sharesMediumRisk = buyShares custPortfolioShares.PortfolioMediumShares cashAmount mediumRiskPrice 
                { SharesLowRisk = custPortfolioShares.PortfolioLowShares; SharesMediumRisk = sharesMediumRisk; SharesHighRisk = custPortfolioShares.PortfolioHighShares}
            | High -> 
                let highRiskPrice = portfolioPricesList.[customerPortfolio.PortfolioId |> getListIdxbyPortfolioId].Price
                let sharesHighRisk = buyShares custPortfolioShares.PortfolioHighShares cashAmount highRiskPrice
                { SharesLowRisk = custPortfolioShares.PortfolioLowShares; SharesMediumRisk = custPortfolioShares.PortfolioMediumShares ; SharesHighRisk = sharesHighRisk}

        newPortfolioShares

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

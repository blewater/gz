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

    type UserInputPortfolio = {
        Db : DbContext;
        UserId : int;
        Month : string;
        PrevMonth : string;
        Portfolio : Portfolio;
        CashToInvest : decimal;
        PortfoliosPrices : PortfoliosPrices
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
            (userPortfolioShares : UserPortfolioShares) : UserPortfolioShares =

        let inputPortfolio = (userInputPortfolio, userPortfolioShares)

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
                                        CustomerId = userInputPortfolio.UserId,
                                        YearMonth = userInputPortfolio.Month
                                    )
        (userInputPortfolio.Portfolio.Risk, newCustomerPortfolioRow) ||> updDbCustomerPortfolio
        userInputPortfolio.Db.CustPortfolios.InsertOnSubmit(newCustomerPortfolioRow)
        

    /// get single Customer Portfolio of the desired month param in the yyyyMM format
    let private getCustomerPortfolio (userInputPortfolio : UserInputPortfolio) : DbCustoPortfolios = 
        let userPortfolio =
            query {
                for row in userInputPortfolio.Db.CustPortfolios do
                where (
                    String.Compare(row.YearMonth, userInputPortfolio.Month, StringComparison.Ordinal) <= 0
                    && row.CustomerId = userInputPortfolio.UserId
                )
                sortByDescending row.YearMonth
                select row
                headOrDefault
            }
        userPortfolio

    let upsCustomerPortfolio(userInputPortfolio : UserInputPortfolio) : unit =
        let currentCustomerPortfolio = userInputPortfolio |> getCustomerPortfolio

        if isNull currentCustomerPortfolio then
            userInputPortfolio |> insDbCustomerPortfolio 
        else
            (userInputPortfolio.Portfolio.Risk, currentCustomerPortfolio) ||> updDbCustomerPortfolio

        userInputPortfolio.Db.DataContext.SubmitChanges()

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
                (userInputPortfolio : UserInputPortfolio) : unit =

        let newCustPortfoliosSharesRow = new DbCustPortfolioShares
                                            (CustomerId = userInputPortfolio.UserId, YearMonth = userInputPortfolio.Month)
        (userPortfolioShares, newCustPortfoliosSharesRow) ||> updDbCustomerPortfolioShares

        userInputPortfolio.Db.CustPortfoliosShares.InsertOnSubmit(newCustPortfoliosSharesRow)

    /// get CustomerPortfolioShares of the desired month param in the yyyyMM format  
    let private getCurrentCustomerPortfolioShares(userInputPortfolio : UserInputPortfolio) : DbCustPortfolioShares = 
        let userPortfolioShares =
            query {
                for row in userInputPortfolio.Db.CustPortfoliosShares do
                where (
                    row.YearMonth = userInputPortfolio.Month
                    && row.CustomerId = userInputPortfolio.UserId
                )
                select row
                exactlyOneOrDefault
            }
        userPortfolioShares

    let upsDbCustomerPortfolioShares(userInputPortfolio : UserInputPortfolio)(userPortfolioShares : UserPortfolioShares) : unit =
        let dbUserSharesRow = userInputPortfolio |> getCurrentCustomerPortfolioShares

        if isNull dbUserSharesRow then
            (userPortfolioShares, userInputPortfolio) ||> insDbCustomerPortfolioShares
        else
            (userPortfolioShares, dbUserSharesRow) ||> updDbCustomerPortfolioShares

        userInputPortfolio.Db.DataContext.SubmitChanges()

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

    let getGzTrx(db : DbContext)(yyyyMm : string) : DbGzTrx list=

        let userGzTrxRows =
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
        userGzTrxRows

    /// get the invBalance row of the desired month in the format of yyyyMm
    let getInvBalance (userInputPortfolio : UserInputPortfolio) : DbInvBalances = 
        let invBalance =
            query {
                for row in userInputPortfolio.Db.InvBalances do
                where (
                    row.YearMonth = userInputPortfolio.Month
                    && row.CustomerId = userInputPortfolio.UserId
                )
                select row
                exactlyOneOrDefault
            }
        invBalance

    let getInvBalanceAmounts
            (userInputPortfolio : UserInputPortfolio)
            (userPortfolioShares : UserPortfolioShares) =


        let prevMonInvBalanceRow = userInputPortfolio |> getInvBalance
        let prevBalance = prevMonInvBalanceRow.Balance
        let userNewShares = (userInputPortfolio, userPortfolioShares) ||> CalcUserPortfolioShares.getNewCustomerShares

        userNewShares


    let private updDbInvBalance
            (userPortfolioShares : UserPortfolioShares)
            (invBalanceRow : DbInvBalances) : unit =

        invBalanceRow.UpdatedOnUTC <- DateTime.UtcNow

    let private insDbInvBalance
                (userInputPortfolio : UserInputPortfolio)(userPortfolioShares : UserPortfolioShares) : unit =

        let newInvBalanceRow = new DbInvBalances(CustomerId = userInputPortfolio.UserId, YearMonth = userInputPortfolio.Month)
        (userPortfolioShares, newInvBalanceRow) ||> updDbInvBalance

        userInputPortfolio.Db.InvBalances.InsertOnSubmit(newInvBalanceRow)
        
    let upsDbInvBalance(userInputPortfolio : UserInputPortfolio)(userPortfolioShares : UserPortfolioShares) : unit =
        let invBalanceRow = userInputPortfolio |> getInvBalance
//        let currBalance = prevMonInvBalanceRow.Balance

        if isNull invBalanceRow then
            (userInputPortfolio, userPortfolioShares) ||> insDbInvBalance
        else
            (userPortfolioShares, invBalanceRow) ||> updDbInvBalance

        userInputPortfolio.Db.DataContext.SubmitChanges()

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

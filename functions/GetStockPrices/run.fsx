#r "System.Configuration"
#r "System.Data"
#r "System.Data.Linq"
open System
open System.Configuration
#if INTERACTIVE

#r "C:/Users/Mario/AppData/Roaming/npm/node_modules/azure-functions-core-tools/bin/Microsoft.Azure.Webjobs.Host.dll"
#r "C:/Users/Mario/data/Functions/packages/nuget/fsharp.data.typeproviders/5.0.0.2/lib/net40/FSharp.Data.TypeProviders.dll"
#r @"C:\Users\Mario\data\Functions\packages\nuget\fsharp.data\2.4.6\lib\net45\FSharp.Data.dll"

open Microsoft.Azure.WebJobs.Host

#endif
//--------------------------------
open FSharp.Data
open FSharp.Data.TypeProviders

type Funds = CsvProvider<"10/31/2017,37.27,37.27,37.27,37.27,0", HasHeaders = true, Schema = "timestamp (date), open (float), high (float), low (float), close (float), volume (int)">
let isNull(obj) = Object.ReferenceEquals(obj, null)

type DateTime with
    member this.ToYyyyMm =
        this.Year.ToString("0000") + this.Month.ToString("00")
            
type DateTime with
    member this.ToYyyyMmDd = 
        this.ToYyyyMm + this.Day.ToString("00") 

let pConnStr = ConfigurationManager.ConnectionStrings.["gzProdDb"].ConnectionString
[<Literal>]
let dbmlLocalFile = __SOURCE_DIRECTORY__ + "/dbmldev.dbml"
type DbSchema = DbmlFile<dbmlLocalFile, ContextTypeName="GzRunTimeDb">
type DbContext = DbSchema.GzRunTimeDb


/// <summary>
///
/// Get a database object by creating a new DataContext and opening a database connection
///
/// </summary>
/// <param name="dbConnectionString">The database connection string to use</param>
/// <returns>A newly created database object</returns>
let getOpenDb (log: TraceWriter)(dbConnectionString : string) : DbContext= 
    let rec openConn(timesLeft : int) : DbContext = 
        log.Info(sprintf "Left number of attempts to open the database connection: %d." timesLeft)
        try
            let db = new DbSchema.GzRunTimeDb(dbConnectionString)
            db.Connection.Open()
            db
        with ex ->
            log.Info(sprintf "Exception while opening the database connection: %s." ex.Message)
            openConn (timesLeft - 1)
    openConn 5 // tries

// Update 2017-11-01 switched to alphavantage.co; format:
// timestamp	open	high	low	   close	volume
// 10/31/2017	10.77	10.77	10.77	10.77	0

// URL of a service that generates price data
[<Literal>]
let DownloadFromFinanceUrl = "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&outputsize=compact&apikey=NMDBJ3KTITWWY5LN&datatype=csv&symbol="
//https://www.alphavantage.co/query?function=BATCH_STOCK_QUOTES&apikey=NMDBJ3KTITWWY5LN&datatype=csv&symbols=VBMFX,VSTCX,CFA,MUB,VSGBX,VTI
type Risk = Low | Medium | High

type PortfolioId = int

type Shares = decimal

type Money = decimal

type YyyyMm = string

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

type Symbol = string
type FundQuote = { Symbol : Symbol; TradedOn : DateTime; ClosingPrice : float }
type PortfolioWeight = float32
type PortfolioFundRecord = {PortfolioId : PortfolioId; PortfolioWeight : PortfolioWeight; Fund : FundQuote}
type DbFunds = DbSchema.Funds
type DbPortfolios = DbSchema.Portfolios
type DbPortfolioFunds = DbSchema.PortFunds
type DbPortfolioPrices = DbSchema.PortfolioPrices
type DbVintageShares = DbSchema.VintageShares
let getLatestFundQuote (log: TraceWriter)(fund : string) : FundQuote =

    let urlLoad = DownloadFromFinanceUrl + fund
    let tradedFunds = Funds.Load(urlLoad)

    // Get latest quote; may be earlier than (Today - 1)
    tradedFunds.Rows 
    |> Seq.maxBy (fun (f) -> f.Timestamp)
    |> fun (row) ->
    { 
        Symbol = fund
        TradedOn = row.Timestamp
        ClosingPrice = float row.Close
    }

/// Get the portfolio funds weighted
let qryFunds (db : DbContext) : (DbSchema.Funds * DbSchema.PortFunds) list =
    query { 
        for f in db.Funds do
        join fp in db.PortFunds on (f.Id = fp.FundId)
        join p in db.Portfolios on (fp.PortfolioId = p.Id)
        where (p.IsActive)
        sortBy fp.PortfolioId
        thenBy f.Id
        select (f, fp)
    }
    |> Seq.toList

let getSymbolPrice (symbol : string)(symbolPrices : Map<string, FundQuote>) : float =
    let fundQuote = symbolPrices.[symbol]
    fundQuote.ClosingPrice

let getSymbolTradeDay (symbol : string)(symbolPrices : Map<string, FundQuote>) : DateTime =
    let fundQuote = symbolPrices.[symbol]
    fundQuote.TradedOn

/// Cast an array of 3 risk portfolios to PortfoliosPrices
let portfolioPricesArrToType(portfolioPricesArr : PortfolioSharePrice[]) : PortfoliosPrices =

    let portfoliosPrices = { 
        PortfolioLowRiskPrice = {PortfolioId = portfolioPricesArr.[LowRiskArrayIndex].PortfolioId; Price = portfolioPricesArr.[LowRiskArrayIndex].Price; TradedOn = portfolioPricesArr.[LowRiskArrayIndex].TradedOn} 
        PortfolioMediumRiskPrice = {PortfolioId = portfolioPricesArr.[MediumRiskArrayIndex].PortfolioId; Price = portfolioPricesArr.[MediumRiskArrayIndex].Price; TradedOn = portfolioPricesArr.[MediumRiskArrayIndex].TradedOn}; 
        PortfolioHighRiskPrice = {PortfolioId = portfolioPricesArr.[HighRiskArrayIndex].PortfolioId; Price = portfolioPricesArr.[HighRiskArrayIndex].Price; TradedOn = portfolioPricesArr.[HighRiskArrayIndex].TradedOn} 
    }
    portfoliosPrices

let getFundsClosingPrice (log: TraceWriter)(dbFundsWithPortfolioFunds : (DbFunds * DbPortfolioFunds) list) : FundQuote list =
    dbFundsWithPortfolioFunds
    |> Seq.map
        (fun (f : DbFunds, _ : DbPortfolioFunds) -> 
            getLatestFundQuote log f.Symbol)
    |> Seq.toList

/// Map portfolio funds to Map of (Fund, fundQuotes) all synchronized to the maximum latest closing date
let fund2MappedQuotes (log: TraceWriter)(dbFundsWithPortfolioFunds : (DbFunds * DbPortfolioFunds) list) : Map<string, FundQuote> =

    getFundsClosingPrice log dbFundsWithPortfolioFunds
    // Sync to latest max TradedOn day date
    |> (fun quotes ->
            let maxDate =
                quotes
                |> Seq.maxBy (
                    fun (quote) -> 
                        quote.TradedOn)
                |> fun q -> 
                    q.TradedOn
            quotes
            |> Seq.map 
                (fun (quote) ->
                    { 
                        Symbol = quote.Symbol; 
                        ClosingPrice = quote.ClosingPrice; 
                        TradedOn = maxDate})
        )
    // Return a Map
    |> Seq.map 
        (fun quote -> 
            (quote.Symbol, quote))
    |> Map.ofSeq 

/// 1. Read The portfolio funds from Db (in another function)
/// 2. Ask yahoo for the fund market closing prices
/// 3. Fold fund prices into a portfolio single share market price

let getPortfoliosPrices (log: TraceWriter)(dbFundsWithPortfolioFunds : (DbFunds * DbPortfolioFunds) list) : PortfoliosPricesMap =

    let fundQuotes = fund2MappedQuotes log dbFundsWithPortfolioFunds

    let portfolioPrices =
        //printfn "%A" fundTradedPrices

        dbFundsWithPortfolioFunds
        |> Seq.map(
            fun (f : DbFunds, fp : DbPortfolioFunds) -> 
                let portfolioFundRec = {
                    PortfolioId = fp.PortfolioId; 
                    PortfolioWeight = fp.Weight; 
                    Fund = fundQuotes.[f.Symbol]
                }
//                    printfn "%A" portfolioFundRec
                portfolioFundRec
            )
//               |> ~~ (printfn "%A")
        |> Seq.groupBy(
            fun (portfolioFundRec : PortfolioFundRecord) -> 
                portfolioFundRec.Fund.TradedOn)
//              |> ~~ (printfn "%A")
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
        |> Seq.iter(fun pp -> 
            Diagnostics.Trace.Assert(
                pp.Value.PortfolioLowRiskPrice.Price > 10.0 &&
                pp.Value.PortfolioMediumRiskPrice.Price > 10.0 &&
                pp.Value.PortfolioHighRiskPrice.Price > 10.0,
                "Portfolio Prices are artificially low check the portfolio calculations"
            )
            )
            
    portfolioPrices

/// Update an existing row of PortfolioPrices
let updDbPortfolioPrice
            (dbPortfolioPrices : DbPortfolioPrices)
            (portfolioPrices : PortfoliosPrices) =

    dbPortfolioPrices.PortfolioLowPrice <- portfolioPrices.PortfolioLowRiskPrice.Price
    dbPortfolioPrices.PortfolioMediumPrice <- portfolioPrices.PortfolioMediumRiskPrice.Price
    dbPortfolioPrices.PortfolioHighPrice <- portfolioPrices.PortfolioHighRiskPrice.Price
    dbPortfolioPrices.UpdatedOnUtc <- DateTime.UtcNow

/// insert a new portfolio price for a trading day and price
let insDbNewRowPortfolioPrice (db : DbContext)(portfolioPrices : PortfoliosPrices) =
    let newPortfolioPriceRow = new DbPortfolioPrices(
                                    YearMonthDay = portfolioPrices.PortfolioLowRiskPrice.TradedOn.ToYyyyMmDd
                                )
    (newPortfolioPriceRow, portfolioPrices) 
        ||> updDbPortfolioPrice

    db.PortfolioPrices.InsertOnSubmit(newPortfolioPriceRow)
    
let getPortfolioPriceList (db : DbContext)(tradingDay : string) : DbPortfolioPrices =
    query {
        for row in db.PortfolioPrices do
        where (row.YearMonthDay = tradingDay)
        select row
        exactlyOneOrDefault
    }

/// Insert the portfolio prices for a trading day if not existing already
let setDbAskToSavePortfolioPrices(db : DbContext)(portfoliosPrices : PortfoliosPrices) : unit =

    // Any portfolio risk type trading day is the same
    let portfolioPricesRow = 
        (db, portfoliosPrices.PortfolioLowRiskPrice.TradedOn.ToYyyyMmDd) 
        ||> getPortfolioPriceList

    if isNull portfolioPricesRow then

        (db, portfoliosPrices) ||> insDbNewRowPortfolioPrice
    else
        (portfolioPricesRow, portfoliosPrices) 
            ||> updDbPortfolioPrice

    db.SubmitChanges()

/// Save all trading days portfolio prices
let setDbPortfoliosPrices(db : DbContext)(portfoliosPrices : PortfoliosPricesMap) : PortfoliosPricesMap=
    portfoliosPrices 
    |> Map.iter (fun key value 
                    -> (db, value) 
                        ||> setDbAskToSavePortfolioPrices)
    portfoliosPrices

/// Ask & Store portfolio shares
let storeShares (log: TraceWriter)(db : DbContext): PortfoliosPricesMap =
    db
    |> qryFunds
    |> getPortfoliosPrices log
    |> setDbPortfoliosPrices db

let Run(myTimer: TimerInfo, log: TraceWriter) =
    let pConnStr = ConfigurationManager.ConnectionStrings.["gzProdDb"].ConnectionString
//    try
    log.Info(
        sprintf "Portfolio shares saved: %A" 
            (getOpenDb log pConnStr
            |> storeShares log))
//    with ex ->
//        log.Info(sprintf "Exception: %s" ex.Message)

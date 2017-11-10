#I __SOURCE_DIRECTORY__
#r "System.Core.dll"
#r "System.Data.dll"
#r "System.Data.Linq.dll"
#r "System.dll"
#r "System.Numerics.dll"
#r "System.Xml.Linq.dll"
#r "../GzCommon/bin/Production/GzCommon.dll"
#r @"..\GzBatch\packages\System.ValueTuple.4.4.0\lib\net461\System.ValueTuple.dll"
#r @"..\GzBatch\packages\NLog.4.4.12\lib\net45\NLog.dll"
#r @"..\gzbatch\packages\FSharp.Data.2.4.2\lib\net45\FSharp.Data.dll"
#load "PortfolioTypes.fs"

open System
open System.Net
open GzBatchCommon
open GzDb.DbUtil
open GzBatchCommon.ErrorHandling
open GzBalances.PortfolioTypes
open NLog
open FSharp.Data

type Funds = CsvProvider<"quotes.csv">

let dbConnectionString = "Server=tcp:gzdbprod.database.windows.net,1433;Database=gzDbProd;User ID=gzProdDbUser;Password=#2!97q7#2BN@Mx5TfK2Y;Encrypt=True;MultipleActiveResultSets=False;TrustServerCertificate=True;Connection Timeout=60;";;
let db = getOpenDb dbConnectionString

[<Literal>]
let downloadFromFinanceUrl = "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&outputsize=compact&apikey=NMDBJ3KTITWWY5LN&datatype=csv&symbol="

let getLatestFundQuote (fund : string) : FundQuote =

    let urlLoad = downloadFromFinanceUrl + fund
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
let pFundsQry (db : DbContext) =
        query { 
            for f in db.Funds do
            join fp in db.PortFunds on (f.Id = fp.FundId)
            join p in db.Portfolios on (fp.PortfolioId = p.Id)
            where (p.IsActive)
            sortBy fp.PortfolioId
            thenBy f.Id
            select (f, fp)
        }

/// Cast an array of 3 risk portfolios to PortfoliosPrices
let portfolioPricesArrToType(portfolioPricesArr : PortfolioSharePrice[]) : PortfoliosPrices =

    // Low : 0, Medium, High
    (portfolioPricesArr.Length = 3, "Incoming Portfolio prices array is not 3 (Low, Medium, High)")
    ||> traceExc

    let portfoliosPrices = { 
        PortfolioLowRiskPrice = {PortfolioId = portfolioPricesArr.[LowRiskArrayIndex].PortfolioId; Price = portfolioPricesArr.[LowRiskArrayIndex].Price; TradedOn = portfolioPricesArr.[LowRiskArrayIndex].TradedOn} 
        PortfolioMediumRiskPrice = {PortfolioId = portfolioPricesArr.[MediumRiskArrayIndex].PortfolioId; Price = portfolioPricesArr.[MediumRiskArrayIndex].Price; TradedOn = portfolioPricesArr.[MediumRiskArrayIndex].TradedOn}; 
        PortfolioHighRiskPrice = {PortfolioId = portfolioPricesArr.[HighRiskArrayIndex].PortfolioId; Price = portfolioPricesArr.[HighRiskArrayIndex].Price; TradedOn = portfolioPricesArr.[HighRiskArrayIndex].TradedOn} 
    }
    portfoliosPrices

    /// Test method Subset of fund2MappedQuotes. Does not sync TradedOn dates to max of the group.
let getFundClosingPrices (dbFundsWithPortfolioFunds : (DbFunds * DbPortfolioFunds) Linq.IQueryable) : Map<string, FundQuote> =
    dbFundsWithPortfolioFunds
    |> Seq.map
        (fun (f : DbFunds, _ : DbPortfolioFunds) -> 
            f.Symbol)
    |> Seq.map getLatestFundQuote
    |> Seq.map 
        (fun quote -> 
            (quote.Symbol, quote))
    |> Map.ofSeq 

    /// Map portfolio funds to Map of (Fund, fundQuotes) all synchronized to the maximum latest closing date
let fund2MappedQuotes(dbFundsWithPortfolioFunds : (DbFunds * DbPortfolioFunds) Linq.IQueryable) : Map<string, FundQuote> =
    dbFundsWithPortfolioFunds
    |> Seq.map
        (fun (f : DbFunds, _ : DbPortfolioFunds) -> 
            f.Symbol)
    |> Seq.map getLatestFundQuote
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

(* Return from (Dbfunds, DbPortfolioFunds) }   
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
let getPortfoliosPrices (dbFundsWithPortfolioFunds : (DbFunds * DbPortfolioFunds) Linq.IQueryable) : PortfoliosPricesMap =

    let fundQuotes = fund2MappedQuotes dbFundsWithPortfolioFunds

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
    let portfolioPricesRow = (db, portfoliosPrices.PortfolioLowRiskPrice.TradedOn.ToYyyyMmDd) ||> getPortfolioPriceList

    if isNull portfolioPricesRow then

        (db, portfoliosPrices) ||> insDbNewRowPortfolioPrice
    else
        (portfolioPricesRow, portfoliosPrices) 
            ||> updDbPortfolioPrice

    db.DataContext.SubmitChanges()

/// Save all trading days portfolio prices
let setDbPortfoliosPrices(db : DbContext)(portfoliosPrices : PortfoliosPricesMap) : PortfoliosPricesMap=
    portfoliosPrices 
    |> Map.iter (fun key value 
                    -> (db, value) 
                        ||> setDbAskToSavePortfolioPrices)
    portfoliosPrices

db
|> pFundsQry
|> getPortfoliosPrices
|> setDbPortfoliosPrices db

// Just checking if symbol dates 
db |> pFundsQry |> getFundClosingPrices
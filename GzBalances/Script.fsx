#I __SOURCE_DIRECTORY__
#r "System.Core.dll"
#r "System.Data.dll"
#r "System.Data.Linq.dll"
#r "System.dll"
#r "System.Numerics.dll"
#r "../GzCommon/bin/Production/GzCommon.dll"
#r @"..\GzBatch\packages\NLog.4.4.11\lib\net45\NLog.dll"
#r @"..\GzBatch\packages\FSharp.Data.2.3.3\lib\net40\FSharp.Data.dll"
#load "PortfolioTypes.fs"

open System
open System.Net
open GzBatchCommon
open GzDb.DbUtil
open GzBatchCommon.ErrorHandling
open GzBalances.PortfolioTypes
open NLog
open FSharp.Data

type Funds = CsvProvider<"quotes.csv", Separators=",", HasHeaders=false, Schema = "TradedOn (Date),Symbol (string),ClosedPrice (float)">

let dbConnectionString = "Server=tcp:gzdbprod.database.windows.net,1433;Database=gzDbProd;User ID=gzProdDbUser;Password=#2!97q7#2BN@Mx5TfK2Y;Encrypt=True;MultipleActiveResultSets=False;TrustServerCertificate=True;Connection Timeout=60;";;
let logger = LogManager.GetCurrentClassLogger()
let db = getOpenDb dbConnectionString

[<Literal>]
let YahooFinanceUrl = "http://download.finance.yahoo.com/d/quotes.csv?s="

let getLatestStockPrice (funds : string) : Map<string, FundQuote> =

    let urlLoad = YahooFinanceUrl + funds + "&f=d1sp"
    let tradedFunds = Funds.Load(urlLoad)

    // Save to map for easy reference
    [ 
        for row in tradedFunds.Rows do
            let tradedOn = row.TradedOn
            let symbol = row.Symbol
            let closedPrice = row.ClosedPrice
            let yQuote = 
                { 
                    Symbol = symbol;
                    TradedOn = tradedOn
                    ClosingPrice = closedPrice
                }
            yield (symbol, yQuote)
    ]
    |> Map.ofList


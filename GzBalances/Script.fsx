#I __SOURCE_DIRECTORY__
#r "System.Core.dll"
#r "System.Data.dll"
#r "System.Data.Linq.dll"
#r "System.dll"
#r "System.Numerics.dll"
#r "../GzCommon/bin/Production/GzCommon.dll"
#r @"..\GzBatch\packages\NLog.4.4.9\lib\net45\NLog.dll"
#r @"..\GzBatch\packages\FSharp.Data.2.3.3\lib\net40\FSharp.Data.dll"
#load "PortfolioTypes.fs"

open System
open System.Net
open GzCommon
open GzDb.DbUtil
open GzCommon.ErrorHandling
open GzBalances.PortfolioTypes
open NLog
open FSharp.Data

type Stocks = CsvProvider<"quotes.csv">

let dbConnectionString = "Server=tcp:gzdbprod.database.windows.net,1433;Database=gzDbProd;User ID=gzProdDbUser;Password=#2!97q7#2BN@Mx5TfK2Y;Encrypt=True;MultipleActiveResultSets=False;TrustServerCertificate=True;Connection Timeout=60;";;
let logger = LogManager.GetCurrentClassLogger()
let db = getOpenDb dbConnectionString

[<Literal>]
let YahooFinanceUrl = "http://download.finance.yahoo.com/d/quotes.csv?s="
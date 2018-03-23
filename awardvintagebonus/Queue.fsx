//#I __SOURCE_DIRECTORY__
#r "./packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#r "./packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll"
#r "./packages/NLog/lib/net45/NLog.dll"
#r "./packages/FSharp.Configuration/lib/net45/FSharp.Configuration.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/FSharp.Azure.StorageTypeProvider.dll"
#r "System.Data.Linq.dll"

open FSharp.Configuration
open FSharp.Azure.StorageTypeProvider
open FSharp.Azure.StorageTypeProvider.Queue
open FSharp.Data.TypeProviders

//type Azure = AzureTypeProvider<"UseDevelopmentStorage=true">

//-- Db Types / entities

type DbSchema = DbmlFile<"gzdbdev.dbml", ContextTypeName="GzRunTimeDb">

type DbContext = DbSchema.GzRunTimeDb
//type DbPlayerRevRpt = DbSchema.PlayerRevRpt
//type DbGzTrx = DbSchema.GzTrxs
//type DbFunds = DbSchema.Funds
//type DbPortfolios = DbSchema.Portfolios
//type DbPortfolioFunds = DbSchema.PortFunds
//type DbPortfolioPrices = DbSchema.PortfolioPrices
//type DbVintageShares = DbSchema.VintageShares
type DbInvBalances = DbSchema.InvBalances
//type DbCustoPortfolios = DbSchema.CustPortfolios

type Settings = AppSettings<"App.config">

let path = System.IO.Path.Combine [|__SOURCE_DIRECTORY__ ; "bin"; "debug"; "awardbonus.exe" |]
Settings.SelectExecutableFile path
printfn "%s" Settings.ConfigFileName

// Connect via configuration file with named connection string.
type FunctionsQueue = AzureTypeProvider<connectionStringName = "storageConnString", configFileName=Settings.ConfigFileName>


//let initQueue (queueConnString : string)(queueName : string) : CloudQueue=
//    // Parse the connection string and return a reference to the storage account.
//    let storageAccount = CloudStorageAccount.Parse(queueConnString)
//    let queueClient = storageAccount.CreateCloudQueueClient()
//    let queue = queueClient.GetQueueReference(queueName)
//    queue

[<Literal>]
let InvBalKey = "rowId"
System.Environment.SetEnvironmentVariable(InvBalKey, "588")

let envVars = 
    System.Environment.GetEnvironmentVariables()
    |> Seq.cast<System.Collections.DictionaryEntry>
    |> Seq.map (fun d ->
        d.Key :?> string, d.Value :?> string
       )
    |> dict

envVars
    |> Seq.sortBy (fun (KeyValue(k,v)) -> k)
    |> Seq.iter (fun (KeyValue(k,v)) -> printfn "%s: %s" k v)
    
let (res, value) = envVars.TryGetValue(InvBalKey)

/// <summary>
///
/// Get a database object by creating a new DataContext and opening a database connection
///
/// </summary>
/// <param name="dbConnectionString">The database connection string to use</param>
/// <returns>A newly created database object</returns>
let getOpenDb (dbConnectionString : string) : DbContext= 

    //logger.Debug("Attempting to open the database connection...")
    // Open db
    let db = new DbSchema.GzRunTimeDb(dbConnectionString)
    //db.DataContext.Log <- System.Console.Out
    db.Connection.Open()
    db
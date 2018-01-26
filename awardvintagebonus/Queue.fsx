#I __SOURCE_DIRECTORY__
#r "./packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#r "./packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll"
#r "./packages/NLog/lib/net45/NLog.dll"
#r "./packages/FSharp.Configuration\lib/net45/FSharp.Configuration.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/FSharp.Azure.StorageTypeProvider.dll"

open FSharp.Configuration
open FSharp.Configuration.AppSettingsTypeProvider
//open Microsoft.WindowsAzure.Storage // Namespace for CloudStorageAccount
//open Microsoft.WindowsAzure.Storage.Queue // Namespace for Queue storage types
open FSharp.Azure.StorageTypeProvider
open FSharp.Data.TypeProviders
open NLog

let logger = LogManager.GetCurrentClassLogger()

// Use for compile time memory schema representation
[<Literal>]
let CompileTimeDbString = "Server=tcp:gzdbdev.database.windows.net,1433;Database=gzDbDev;User ID=gzDevReader;Password=Life is good wout writing8!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

//-- Types

type DbSchema = SqlDataConnection<ConnectionString=CompileTimeDbString >
type DbContext = DbSchema.ServiceTypes.SimpleDataContextTypes.GzDbDev
type DbPlayerRevRpt = DbSchema.ServiceTypes.PlayerRevRpt
type DbGzTrx = DbSchema.ServiceTypes.GzTrxs
type DbFunds = DbSchema.ServiceTypes.Funds
type DbPortfolios = DbSchema.ServiceTypes.Portfolios
type DbPortfolioFunds = DbSchema.ServiceTypes.PortFunds
type DbPortfolioPrices = DbSchema.ServiceTypes.PortfolioPrices
type DbVintageShares = DbSchema.ServiceTypes.VintageShares
type DbInvBalances = DbSchema.ServiceTypes.InvBalances

let path = System.IO.Path.Combine [|__SOURCE_DIRECTORY__ ; "bin"; "debug"; "awardbonus.exe" |]
Settings.SelectExecutableFile path
printfn "%s" Settings.ConfigFileName

let queueConnString = Settings.QueueConnString.ToString()
let queueName = Settings.QueueName

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

initQueue queueConnString queueName

/// <summary>
///
/// Get a database object by creating a new DataContext and opening a database connection
///
/// </summary>
/// <param name="dbConnectionString">The database connection string to use</param>
/// <returns>A newly created database object</returns>
let getOpenDb (dbConnectionString : string) : DbContext= 

    logger.Debug("Attempting to open the database connection...")
    // Open db
    let db = DbSchema.GetDataContext(dbConnectionString)
    //db.DataContext.Log <- System.Console.Out
    db.Connection.Open()
    db


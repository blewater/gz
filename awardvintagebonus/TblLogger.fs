#if INTERACTIVE
#r "./packages/FSharp.Configuration/lib/net45/FSharp.Configuration.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/Microsoft.WindowsAzure.Storage.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/FSharp.Azure.StorageTypeProvider.dll"
#else
module TblLogger
#endif
open System
open FSharp.Configuration
open FSharp.Azure.StorageTypeProvider
open Microsoft.WindowsAzure.Storage
open FSharp.Azure.StorageTypeProvider.Table
open FSharp.Azure.StorageTypeProvider
open BonusReq
open Newtonsoft.Json

// Connect via configuration file with named connection string.
type AzStorage = AzureTypeProvider<connectionStringName = "storageConnString", configFileName="App.config">

let log = AzStorage.Tables.BonusLog

let handleResponse =
    function
    | SuccessfulResponse(entityId, errorCode) -> printfn "Entity %A succeeded: %d." entityId errorCode
    | EntityError(entityId, httpCode, errorCode) -> printfn "Entity %A failed: %d - %s." entityId httpCode errorCode
    | BatchOperationFailedError(entityId) -> printfn "Entity %A was ignored as part of a failed batch operation." entityId
    | BatchError(entityId, httpCode, errorCode) -> printfn "Entity %A failed with an unknown batch error: %d - %s." entityId httpCode errorCode

let insert (yearMonthSold : string)(logEntry: BonusReqType) =
    let json = JsonConvert.SerializeObject(logEntry)
    log.InsertAsync(Table.Partition yearMonthSold, Table.Row (logEntry.GmUserId.ToString()), json, Table.TableInsertMode.Insert)
    |> Async.RunSynchronously
    |> handleResponse
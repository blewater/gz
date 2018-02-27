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
type AzStorage = AzureTypeProvider<connectionStringName = "storageConnString", configFileName="App.config", tableSchema="TblLoggerSchema.json">

let log = AzStorage.Tables.BonusLog

type BonusLogType = {
    YearMonthSold : string;
    GmUserId : int;
    UserEmail : string;
    UserFirstName : string;
    Currency : string;
    Amount : decimal;
    Fees : decimal;
    InvBalIds : string;
    ProcessedCnt : int;
    CreatedOn: DateTime;
    LastProcessedTime: DateTime;
}

let bonusReq2Log(bonusReg : BonusReqType) : BonusLogType =
    {
        YearMonthSold = bonusReg.YearMonthSold;
        GmUserId = bonusReg.GmUserId;
        UserEmail = bonusReg.UserEmail;
        UserFirstName = bonusReg.UserFirstName;
        Currency = bonusReg.Currency;
        Amount = bonusReg.Amount;
        Fees = bonusReg.Fees;
        InvBalIds = bonusReg.InvBalIds 
                    |> Array.map(fun i -> i.ToString()) 
                    |> String.concat ",";
        ProcessedCnt = bonusReg.ProcessedCnt;
        CreatedOn = bonusReg.CreatedOn;
        LastProcessedTime = bonusReg.LastProcessedTime;
    }

let handleResponse =
    function
    | SuccessfulResponse(entityId, errorCode) -> printfn "Entity %A succeeded: %d." entityId errorCode
    | EntityError(entityId, httpCode, errorCode) -> printfn "Entity %A failed: %d - %s." entityId httpCode errorCode
    | BatchOperationFailedError(entityId) -> printfn "Entity %A was ignored as part of a failed batch operation." entityId
    | BatchError(entityId, httpCode, errorCode) -> printfn "Entity %A failed with an unknown batch error: %d - %s." entityId httpCode errorCode

let insert (yearMonthSold : string)(logEntry: BonusReqType) =
    let bonusLog = bonusReq2Log logEntry
    log.InsertAsync(
        Table.Partition logEntry.YearMonthSold, 
        Table.Row (logEntry.GmUserId.ToString()), 
        bonusLog, 
        Table.TableInsertMode.Insert)
    |> Async.RunSynchronously
    |> handleResponse
#if INTERACTIVE
#I __SOURCE_DIRECTORY__
#r "./packages/FSharp.Configuration/lib/net45/FSharp.Configuration.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/Microsoft.WindowsAzure.Storage.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/FSharp.Azure.StorageTypeProvider.dll"
#r "./packages/FSharp.Json/lib/net45/FSharp.Json.dll"
#r "./packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
#load "BonusReq.fs"
open System
open FSharp.Configuration
open FSharp.Azure.StorageTypeProvider
open Microsoft.WindowsAzure.Storage
open FSharp.Azure.StorageTypeProvider.Queue
open BonusReq
open Newtonsoft.Json
open FSharp.Json
type Settings = AppSettings<"App.config">

let path = System.IO.Path.Combine [|__SOURCE_DIRECTORY__ ; "bin"; "debug"; "awardbonus.exe" |]
Settings.SelectExecutableFile path
printfn "%s" Settings.ConfigFileName

// Connect via configuration file with named connection string.
type FunctionsQueue = AzureTypeProvider<connectionStringName = "storageConnString", configFileName=Settings.ConfigFileName>
#else
module BonusQueue
open System
open FSharp.Configuration
open FSharp.Azure.StorageTypeProvider
open Microsoft.WindowsAzure.Storage
open FSharp.Azure.StorageTypeProvider.Queue
open BonusReq
open Newtonsoft.Json
open FSharp.Json
// Connect via configuration file with named connection string.
let appConfigPath = System.IO.Path.Combine(__SOURCE_DIRECTORY__, "App.config")
type FunctionsQueue = AzureTypeProvider<connectionStringName = "storageConnString", configFileName="App.config">
#endif

let bonusQueue = FunctionsQueue.Queues.bonusreq

let qCnt() =
    let cnt = bonusQueue.GetCurrentLength()
    printfn "Queue length is %d." (cnt)
    cnt

let printMessage msg =
    printfn "Message %A with body '%s' has been dequeued %d times." msg.Id msg.AsString.Value msg.DequeueCount

let getNextQMsg() : ProvidedQueueMessage option =
    async {
        let! qMsg = bonusQueue.Dequeue()
        return qMsg
    } |> Async.RunSynchronously

let bonusQ2Obj(qMsg : ProvidedQueueMessage) : BonusReqType =
    printMessage qMsg
    let bonusReq = Json.deserialize<BonusReqType> qMsg.AsString.Value
    bonusReq

let deleteBonusReq(msq : ProvidedQueueMessage) =
    async { do! bonusQueue.DeleteMessage msq.Id } |> Async.RunSynchronously

let enQueueJson(br : BonusReqType) =
    async {
        let json = JsonConvert.SerializeObject(br)
        do! bonusQueue.Enqueue(json)
    } |> Async.RunSynchronously

let enQUpdated(qMsgId)(br : BonusReqType) =
    async {
        let json = JsonConvert.SerializeObject(br)
        do! bonusQueue.UpdateMessage(qMsgId, json)
    } |> Async.RunSynchronously

let testBonusReq() : BonusReqType = {
    AdminEmailRecipients = [|"mario@greenzorro.com"|];
    Amount = 0.10m;
    Currency = "EUR";
    Fees = 0.025m;
    GmUserId = 4300962; // ladderman
    InvBalIds = [|588; 2783|];
    UserEmail = "salem8@gmail.com";
    UserFirstName = "Mario";
    YearMonthSold = "201802";
    ProcessedCnt = 0;
    CreatedOn = DateTime.UtcNow;
    LastProcessedTime = DateTime.UtcNow;
}

let createTestBonusReq() =
    let br = testBonusReq()
    enQueueJson br

// Connect to local storage emulator localstorageConnString
(*
type FunctionsQueueLocal = AzureTypeProvider<"UseDevelopmentStorage=true">
let localBonusQueue = FunctionsQueueLocal.Queues.``withdrawn-vintages-bonus``
printfn "Local Queue length is %d." (localBonusQueue.GetCurrentLength())
*)

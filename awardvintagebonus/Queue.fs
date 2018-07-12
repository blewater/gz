﻿#if INTERACTIVE
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
open NLog
open Microsoft.WindowsAzure.Storage.Queue

let logger = LogManager.GetCurrentClassLogger()
// Connect via configuration file with named connection string.
let appConfigPath = System.IO.Path.Combine(__SOURCE_DIRECTORY__, "App.config")
type FunctionsQueue = AzureTypeProvider<connectionStringName = "storageConnString", configFileName="App.config">
// Parse the connection string and return a reference to the storage account.
type Settings = AppSettings<"App.config">
#endif

let bonusQueue = FunctionsQueue.Queues.bonusreq

// Init the bon
[<Literal>]
let bonusMsgQName = "bonusmsg"
let storageAccount = CloudStorageAccount.Parse(Settings.ConnectionStrings.StorageConnString)
let queueClient = storageAccount.CreateCloudQueueClient()
let bonusMsgQ = queueClient.GetQueueReference(bonusMsgQName)

let qCnt() =
    let cnt = bonusQueue.GetCurrentLength()
    logger.Info(sprintf "Queue length is %d." cnt)
    cnt

let printMessage msg =
    logger.Info(sprintf "Message %A with body '%s' has been dequeued %d times." msg.Id msg.AsString.Value msg.DequeueCount)

let getNextQMsg() : ProvidedQueueMessage option =
    async {
        let! qMsg = bonusQueue.Dequeue()
        return qMsg
    } |> Async.RunSynchronously

let bonusQ2Obj(qMsg : ProvidedQueueMessage) : BonusReqType =
    printMessage qMsg
    let bonusReq = Json.deserialize<BonusReqType> qMsg.AsString.Value
    bonusReq

/// Resilient Bonus Queue Msg Deletion which tries 10 times with a 1 sec pause
let deleteBonusReq(msq : ProvidedQueueMessage)(bonusReq : BonusReqType) : BonusReqType =
    let rec retryDel(cnt) =
        let res = 
            async { do! bonusQueue.DeleteMessage msq.Id }
            |> Async.Catch
            |> Async.RunSynchronously
        match res with
        | Choice1Of2 _ -> logger.Info("Message deleted.")
        | Choice2Of2 exn ->
            TblLogger.Upsert (Some exn) bonusReq |> ignore
            logger.Error( sprintf "Message not deleted: %s." exn.Message)
            match cnt with
            | 1 -> raise exn
            | _ -> // Sleep 1 sec
                Threading.Thread.Sleep(1000)
                retryDel (cnt - 1)
    retryDel 60
    bonusReq

let enQueue2BonusReq(bonusReq : BonusReqType) =
    let json = JsonConvert.SerializeObject(bonusReq)
    async {
        do! bonusQueue.Enqueue(json)
    } |> Async.RunSynchronously

/// Enqueue message using window q library instead of the mono type provider bug
let enQueue2BonusMsg(bonusReq : BonusReqType) =
    let json = JsonConvert.SerializeObject(bonusReq)
    let bonusQMsg = new CloudQueueMessage(json)
    async {
        bonusMsgQ.AddMessageAsync(bonusQMsg) |> ignore
    } |> Async.RunSynchronously

let enQUpdated(qMsgId)(br : BonusReqType) =
    let json = JsonConvert.SerializeObject(br)
    async {
        do! bonusQueue.UpdateMessage(qMsgId, json)
    } |> Async.RunSynchronously

let createTestBonusReq() =
    let br = testBonusReq()
    enQueue2BonusReq br

// Connect to local storage emulator localstorageConnString
(*
type FunctionsQueueLocal = AzureTypeProvider<"UseDevelopmentStorage=true">
let localBonusQueue = FunctionsQueueLocal.Queues.``withdrawn-vintages-bonus``
printfn "Local Queue length is %d." (localBonusQueue.GetCurrentLength())
*)

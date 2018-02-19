#if INTERACTIVE
#r "./packages/FSharp.Configuration/lib/net45/FSharp.Configuration.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/Microsoft.WindowsAzure.Storage.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/FSharp.Azure.StorageTypeProvider.dll"
#r "./packages/FSharp.Json/lib/net45/FSharp.Json.dll"
#r "./packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
#else
module BonusQueue
#endif
open System
open FSharp.Configuration
open FSharp.Azure.StorageTypeProvider
open Microsoft.WindowsAzure.Storage
open FSharp.Azure.StorageTypeProvider.Queue
open BonusReq
open Newtonsoft.Json
open FSharp.Json

// Connect via configuration file with named connection string.
type FunctionsQueue = AzureTypeProvider<connectionStringName = "storageConnString", configFileName="App.config">
let qCnt() =
    let bonusQueue = FunctionsQueue.Queues.bonusreq
    let cnt = bonusQueue.GetCurrentLength()
    printfn "Queue length is %d." (cnt)
    cnt

let printMessage msg =
    printfn "Message %A with body '%s' has been dequeued %d times." msg.Id msg.AsString.Value msg.DequeueCount

let getNextQMsg(qs : FunctionsQueue.Domain.Queues) : ProvidedQueueMessage option =
    async {
        let! qMsg = qs.bonusreq.Dequeue()
        return qMsg
    } |> Async.RunSynchronously

let getNextBonusReq(qMsg : ProvidedQueueMessage option) : BonusReqType option =
    match qMsg with
    | Some msq ->
        printMessage msq
        let bonusReq = Json.deserialize<BonusReqType> msq.AsString.Value
        Some bonusReq
    | _ -> None

let private enQueueJson(qs : FunctionsQueue.Domain.Queues)(json : string) =
    async {
        let json = JsonConvert.SerializeObject(json)
        do! qs.bonusreq.Enqueue(json)
    } |> Async.RunSynchronously

/// Partially applied: bound to
let enqJson = enQueueJson FunctionsQueue.Queues

// Connect to local storage emulator localstorageConnString
(*
type FunctionsQueueLocal = AzureTypeProvider<"UseDevelopmentStorage=true">
//let qc = FunctionsQueueLocal.Queues.CloudQueueClient
//let lq = qc.GetQueueReference(Settings.QueueName)
//let res = lq.CreateIfNotExists()
let localBonusQueue = FunctionsQueueLocal.Queues.``withdrawn-vintages-bonus``
printfn "Local Queue length is %d." (localBonusQueue.GetCurrentLength())
*)

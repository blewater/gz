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
let bonusQueue = FunctionsQueue.Queues.bonusreq
printfn "Queue length is %d." (bonusQueue.GetCurrentLength())

let printMessage msg =
    printfn "Message %A with body '%s' has been dequeued %d times." msg.Id msg.AsString.Value msg.DequeueCount

let getNextBonusReq(qs : FunctionsQueue.Domain.Queues) =
    async {
        let! message = qs.bonusreq.Dequeue()
        let bonusReq =
            match message with
            | Some req ->
                printMessage req
                let bonusReq = Json.deserialize<BonusReqType> req.AsString.Value
                Some bonusReq
            | _ -> None
        return bonusReq
    } |> Async.RunSynchronously

let enQueueBonusReq(qs : FunctionsQueue.Domain.Queues)(bonusReqJson : string) =
    async {
        let json = JsonConvert.SerializeObject(bonusReqJson)
        do! qs.bonusreq.Enqueue(json)
    } |> Async.RunSynchronously


// Connect to local storage emulator localstorageConnString
(*
type FunctionsQueueLocal = AzureTypeProvider<"UseDevelopmentStorage=true">
//let qc = FunctionsQueueLocal.Queues.CloudQueueClient
//let lq = qc.GetQueueReference(Settings.QueueName)
//let res = lq.CreateIfNotExists()
let localBonusQueue = FunctionsQueueLocal.Queues.``withdrawn-vintages-bonus``
printfn "Local Queue length is %d." (localBonusQueue.GetCurrentLength())
*)

#if INTERACTIVE
#r "./packages/FSharp.Configuration/lib/net45/FSharp.Configuration.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/Microsoft.WindowsAzure.Storage.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/FSharp.Azure.StorageTypeProvider.dll"
#else
module Queue
#endif
open System
open FSharp.Configuration
open FSharp.Azure.StorageTypeProvider
open Microsoft.WindowsAzure.Storage
open FSharp.Azure.StorageTypeProvider.Queue

// Connect via configuration file with named connection string.
type FunctionsQueue = AzureTypeProvider<connectionStringName = "storageConnString", configFileName="App.config">
let bonusQueue = FunctionsQueue.Queues.``withdrawn-vintages-bonus``
printfn "Queue length is %d." (bonusQueue.GetCurrentLength())

// Connect to local storage emulator localstorageConnString
(*
type FunctionsQueueLocal = AzureTypeProvider<"UseDevelopmentStorage=true">
//let qc = FunctionsQueueLocal.Queues.CloudQueueClient
//let lq = qc.GetQueueReference(Settings.QueueName)
//let res = lq.CreateIfNotExists()
let localBonusQueue = FunctionsQueueLocal.Queues.``withdrawn-vintages-bonus``
printfn "Local Queue length is %d." (localBonusQueue.GetCurrentLength())
*)

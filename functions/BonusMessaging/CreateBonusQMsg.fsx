#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/Microsoft.WindowsAzure.Storage.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/FSharp.Azure.StorageTypeProvider.dll"
#r "./packages/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"
#r "./packages/FSharp.Configuration/lib/net45/FSharp.Configuration.dll"

open System
open FSharp.Configuration
open FSharp.Azure.StorageTypeProvider
open FSharp.Azure.StorageTypeProvider.Queue
open Newtonsoft.Json

type Settings = AppSettings<"App.config">
Settings.SelectExecutableFile __SOURCE_DIRECTORY__
printfn "%s" Settings.ConfigFileName

type FunctionsQueue = AzureTypeProvider<connectionStringName = "storageConnString", configFileName=Settings.ConfigFileName>

type BonusReqType = {
    AdminEmailRecipients : string[];
    Currency : string;
    Amount : decimal;
    Fees : decimal;
    GmUserId : int;
    InvBalIds : int[];
    UserEmail : string;
    UserFirstName : string;
    YearMonthSold : string;
    ProcessedCnt : int;
    CreatedOn: DateTime;
    LastProcessedTime: DateTime;
}

let testBonusReq() : BonusReqType = {
    AdminEmailRecipients = [|"mario@greenzorro.com"|];
    Amount = 0.10m;
    Currency = "EUR";
    Fees = 0.025m;
    GmUserId = 4300962; // ladderman
    InvBalIds = [|588|];
    UserEmail = "salem8@gmail.com";
    UserFirstName = "Mario";
    YearMonthSold = "201803";
    ProcessedCnt = 1;
    CreatedOn = DateTime.UtcNow;
    LastProcessedTime = DateTime.UtcNow;
}

let bonusQueueMsq = FunctionsQueue.Queues.bonusmsg

let printMessage msg =
    printfn "Message %A with body '%s' has been dequeued %d times." msg.Id msg.AsString.Value msg.DequeueCount

let enQueueJson(br : BonusReqType) =
    async {
        let json = JsonConvert.SerializeObject(br)
        do! bonusQueueMsq.Enqueue(json)
    } |> Async.RunSynchronously

let createTestBonusMsgReq() =
    let br = testBonusReq()
    enQueueJson br

do createTestBonusMsgReq()
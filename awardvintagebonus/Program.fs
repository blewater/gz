#if INTERACTIVE
#r "./packages/canopy/lib/canopy.dll"
#r "./packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#r "./packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll"
#r "./packages/NLog/lib/net45/NLog.dll"
#r "./packages/Selenium.WebDriver/lib/net45/WebDriver.dll"
#r "./packages/FSharp.Configuration/lib/net45/FSharp.Configuration.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/Microsoft.WindowsAzure.Storage.dll"
#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/FSharp.Azure.StorageTypeProvider.dll"
#load "BonusReq.fs"
#load "Queue.fs"
//#else
//module main
#endif
open Microsoft.FSharp.Collections
open FSharp.Configuration
open NLog
open BonusQueue
open BonusReq
open System
open FSharp.Azure.StorageTypeProvider.Queue
let logger = LogManager.GetCurrentClassLogger()

type Settings = AppSettings<"App.config">

let hostEmail = Settings.HostGmailUser;
let hostPwd = Settings.HostGmailPwd;
let helpEmail = Settings.HelpGmailUser;
let helpPwd = Settings.HelpGmailPwd;
//let dbCtx = Db.getOpenDb Settings.ConnectionStrings.GzProdDb
let yearMonthSold = DateTime.UtcNow.Year.ToString("0000") + DateTime.UtcNow.Month.ToString("00")

//let dbAwardGiven(bonusReq : BonusReqType) = 
//    let sql =
//        sprintf "UPDATE InvBalances Set AwardedSoldAmount = 1, UpdatedOnUTC = GETUTCDATE() WHERE Id IN (%s)"
//            (bonusReq.InvBalIds 
//                |> Array.map(fun i -> i.ToString()) 
//                |> String.concat ",")

//    try 
//        let rowCnt = 
//            dbCtx.ExecuteCommand(sql)
//        printfn "Updated %d rows." rowCnt

//    with ex ->
//        logger.Error(ex, (sprintf "Database update error %s" sql) )
//    bonusReq

let updQBonusReq(bonusQReq : ProvidedQueueMessage) =

    let bonusReq = 
        bonusQReq 
        |> bonusQ2Obj 
    try
        bonusReq
        |> incProcessCnt
        |> enQUpdated bonusQReq.Id
    with ex ->
        logger.Error(ex, sprintf "Error in updQBonusReq() for id: %d" bonusReq.GmUserId )
        TblLogger.Upsert (Some ex) bonusReq |> ignore

[<EntryPoint>]
let main argv = 

    try
        let queuedItemCnt = qCnt()
        if queuedItemCnt > 0 then
            try
                ChromeAwarder.startBrowserSession false
                let rec procQueue(qLeftItems) : unit =
                    match getNextQMsg() with
                    | Some bonusQReq ->
                        try
                            bonusQReq 
                            |> bonusQ2Obj
                            |> TblLogger.Upsert None
                            |> ChromeAwarder.awardUser
                            |> enQueue2BonusMsg
                            //|> emailSender.SendBonusReqUserReceipt helpEmail helpPwd
                            //|> emailSender.SendBonusReqAdminReceipt hostEmail hostPwd
                            deleteBonusReq bonusQReq
                        with ex ->
                            TblLogger.Upsert (Some ex) (bonusQ2Obj bonusQReq) |> ignore
                            // Update process cnt
                            updQBonusReq bonusQReq
                            logger.Error(ex, sprintf "Failed processing q msg %A" bonusQReq.Id)
                    | _ -> ()
                    if qLeftItems > 1 then
                        procQueue (qLeftItems - 1)
                procQueue queuedItemCnt
            finally
                ChromeAwarder.endBrowserSession()
            
    with ex -> 
        logger.Fatal(ex, "Aborting awardbonus!")

    0 // return an integer exit code
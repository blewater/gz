//#if INTERACTIVE
//#r "./packages/canopy/lib/canopy.dll"
//#r "./packages/FSharp.Data/lib/net45/FSharp.Data.dll"
//#r "./packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll"
//#r "./packages/NLog/lib/net45/NLog.dll"
//#r "./packages/Selenium.WebDriver/lib/net45/WebDriver.dll"
//#r "./packages/FSharp.Configuration/lib/net45/FSharp.Configuration.dll"
//#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/Microsoft.WindowsAzure.Storage.dll"
//#r "./packages/FSharp.Azure.StorageTypeProvider/lib/net452/FSharp.Azure.StorageTypeProvider.dll"
//#else
//module main
//#endif
open Microsoft.FSharp.Collections
open FSharp.Configuration
open NLog
open BonusQueue
open BonusReq
open System
open SendEmail
open FSharp.Azure.StorageTypeProvider.Queue

let logger = LogManager.GetCurrentClassLogger()

type Settings = AppSettings<"App.config">

let hostEmail = Settings.HostGmailUser;
let hostPwd = Settings.HostGmailPwd;
let helpEmail = Settings.HelpGmailUser;
let helpPwd = Settings.HelpGmailPwd;
let emailSender = EmailReceipts()
let dbCtx = Db.getOpenDb Settings.ConnectionStrings.GzProdDb
let yearMonthSold = DateTime.UtcNow.Year.ToString("0000") + DateTime.UtcNow.Month.ToString("00")

let dbAwardGiven(bonusReq : BonusReqType) = 
    try 
        let invIds =
            bonusReq.InvBalIds |> Array.toSeq
            //query {
            //    for id in bonusReq.InvBalIds do
            //    select id
            //}

        query {
            for soldVintage in dbCtx.InvBalances do
            where (invIds |> Seq.contains (soldVintage.Id))
            select soldVintage
        } 
        |> Seq.iter (fun sv -> 
            
            sv.AwardedSoldAmount <- true
            sv.UpdatedOnUTC <- DateTime.UtcNow
        )
        dbCtx.SubmitChanges()
    with ex ->
        logger.Error(ex, "Database update")
    bonusReq

let updQBonusReq(bonusQReq : ProvidedQueueMessage) =

    let bonusReq = 
        bonusQReq 
        |> bonusQ2Obj 
    try
        bonusReq
        |> incProcessCnt
        |> enQUpdated bonusQReq.Id
    with ex ->
        TblLogger.insert yearMonthSold bonusReq

[<EntryPoint>]
let main argv = 

    try
        while qCnt() > 0 do
            match getNextQMsg() with
            | Some bonusQReq ->
                try
                    bonusQReq 
                    |> bonusQ2Obj 
                    |> BonusAwarder.start
                    |> dbAwardGiven
                    |> emailSender.SendBonusReqUserReceipt helpEmail helpPwd
                    |> emailSender.SendBonusReqAdminReceipt hostEmail hostPwd
                    deleteBonusReq bonusQReq
                with ex ->
                    TblLogger.insert yearMonthSold (bonusQ2Obj bonusQReq)
                    // Update process cnt
                    updQBonusReq bonusQReq
                    logger.Fatal(ex, sprintf "Failed processing q msg %A" bonusQReq.Id)
            | _ -> ()
    with ex -> 
        logger.Fatal(ex, "Aborting awardbonus!")

        
    0 // return an integer exit code
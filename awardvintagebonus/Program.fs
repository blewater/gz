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

let logger = LogManager.GetCurrentClassLogger()

type Settings = AppSettings<"App.config">

let hostEmail = Settings.HostGmailUser;
let hostPwd = Settings.HostGmailPwd;
let helpEmail = Settings.HelpGmailUser;
let helpPwd = Settings.HelpGmailPwd;

let emailSender = EmailReceipts()

let dbCtx = Db.getOpenDb Settings.ConnectionStrings.GzProdDb
(*
 var newBonusReq = new BonusReq()
            {
                AdminEmailRecipients = adminsArr,
                Amount = user.NetProceeds,
                Fees = user.Fees,
                GmUserId = user.GmUserId,
                InvBalIds = soldVintageDtos.Select(v => v.InvBalanceId).ToArray(),
                UserEmail = user.Email,
                UserFirstName = user.FirstName
            };
            *)

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

[<EntryPoint>]
let main argv = 

    try
        while qCnt() > 0 do
            match getNextQMsg() with
            | Some bonusQReq ->
                bonusQReq 
                |> getNextBonusReq 
                |> BonusAwarder.start
                |> dbAwardGiven
                |> emailSender.SendBonusReqUserReceipt helpEmail helpPwd

                deleteBonusReq bonusQReq
            | _ -> ()
    with ex -> 
        logger.Fatal(ex, "Aborting awardbonus!")
        
    0 // return an integer exit code
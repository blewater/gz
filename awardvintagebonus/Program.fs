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
open System
open FSharp.Configuration
open NLog
open FSharp.Data.TypeProviders

let logger = LogManager.GetCurrentClassLogger()

type Settings = AppSettings<"App.config">

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

[<EntryPoint>]
let main argv = 

    let dbConnection = Db.getOpenDb Settings.ConnectionStrings.GzProdDb
    try
        query {
            for soldVintages in dbConnection.InvBalances do
            where (soldVintages.AwardedSoldAmount = false)
            select soldVintages.SoldAmount
        } 
        |> Seq.iter (fun sv -> 
        
            BonusAwarder.start()
        )

    with ex -> 
        logger.Fatal(ex, "Aborting awardbonus!")
        
    0 // return an integer exit code
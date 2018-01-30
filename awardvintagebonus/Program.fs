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

[<EntryPoint>]
let main argv = 

    BonusAwarder.start()    
        
    0 // return an integer exit code
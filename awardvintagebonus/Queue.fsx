#I __SOURCE_DIRECTORY__
#r "./packages/FSharp.Data/lib/net45/FSharp.Data.dll"
#r "./packages/FSharp.Data.TypeProviders/lib/net40/FSharp.Data.TypeProviders.dll"
#r "./packages/NLog/lib/net45/NLog.dll"
#r "./packages/WindowsAzure.Storage/lib/net45/Microsoft.WindowsAzure.Storage.dll"
#r "./packages/FSharp.Configuration\lib/net45/FSharp.Configuration.dll"
open FSharp.Configuration

type Settings = AppSettings<"App.config">

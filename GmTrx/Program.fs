open System
open FSharp.Configuration
open CpcDataServices

type Settings = AppSettings< "app.config" >
#if DEBUG

let connString = Settings.ConnectionStrings.GzDevDb

printfn "Development db: %s" connString
#else
let connString = Settings.ConnectionStrings.GzProdDb
printfn "PRODUCTION db: %s" connString
#endif


let inRptFolder = String.Concat(Settings.BaseFolder, Settings.ExcelInFolder)
let currencyRatesUrl = Settings.CurrencyRatesUrl.ToString()

[<EntryPoint>]
let main argv = 

    let rates = CurrencyRates.getCurrencyRates currencyRatesUrl
    let json = CurrencyRates.setDbRates rates

    Etl.Phase1Processing connString inRptFolder
    printfn "Press Enter to finish..."
    Console.ReadLine() |> ignore
    0
(*****
* Notes
* http://stackoverflow.com/questions/22608584/how-to-project-transform-an-array-of-fileinfo-to-a-list-of-strings-with-fsharp/22608949#22608949
* http://theburningmonk.com/2011/09/fsharp-pipe-forward-and-pipe-backward/
* http://stackoverflow.com/questions/14657954/using-nlog-with-f-interactive-in-visual-studio-need-documentation
* http://www.c-sharpcorner.com/UploadFile/mgold/writing-equivalent-linq-expressions-in-fsharp/
* https://msdn.microsoft.com/visualfsharpdocs/conceptual/walkthrough-accessing-a-sql-database-by-using-type-providers-%5bfsharp%5d
* http://stackoverflow.com/questions/13768757/how-is-one-supposed-to-use-the-f-sqldataconnection-typeprovider-with-an-app-con
* http://stackoverflow.com/questions/13107676/f-type-provider-for-sql-in-a-class
* http://stackoverflow.com/questions/32312503/f-typeproviders-how-to-change-database?noredirect=1&lq=1
*)


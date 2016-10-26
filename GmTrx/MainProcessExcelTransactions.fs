open NLog
open System
open FSharp.Configuration
open CpcDataServices
open gzCpcLib.Task

type Settings = AppSettings< "app.config" >
let logger = LogManager.GetCurrentClassLogger()

#if DEBUG && !PRODUCTION

let isProd = false
let dbConnectionString = Settings.ConnectionStrings.GzDevDb

logger.Warn(sprintf "Development db: %s" dbConnectionString)

#endif
#if PRODUCTION

let isProd = true
let dbConnectionString = Settings.ConnectionStrings.GzProdDb
logger.Warn(sprintf "PRODUCTION db: %s" dbConnectionString)
#endif

let inRptFolder = String.Concat(Settings.BaseFolder, Settings.ExcelInFolder)
let outRptFolder = String.Concat(Settings.BaseFolder, Settings.ExcelOutFolder)
let currencyRatesUrl = Settings.CurrencyRatesUrl.ToString()

[<EntryPoint>]
let main argv = 

    try
        logger.Info("Start processing @ UTC : " + DateTime.UtcNow.ToString("s"))
        logger.Info("----------------------------")

        // Create a database context
        use db = DbUtil.getOpenDb dbConnectionString

        // Update Funds from Yahoo Api
        (new FundsUpdTask(isProd)).DoTask()

        // Update Currency Rates from open exchange api
        let rates = CurrencyRates.updCurrencyRates currencyRatesUrl db

        // Extract & Load Daily Everymatrix Report
        Etl.ProcessExcelFolder isProd db inRptFolder outRptFolder rates

        logger.Info("----------------------------")
        logger.Info("Finished processing @ UTC : " + DateTime.UtcNow.ToString("s"))

    with ex ->
        logger.Fatal(ex, "Runtime Exception at main")

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


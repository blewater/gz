﻿open NLog
open System
open FSharp.Configuration
open GzBalances
open GzDb
open DbImport

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
        use db = DbUtil.getOpenDb dbConnectionString

        let stock = Portfolio.getPortfolioPrices db

        logger.Info("Start processing @ UTC : " + DateTime.UtcNow.ToString("s"))
        logger.Info("----------------------------")

        logger.Info("Validating Gm excel rpt files")
        
        let rptFilesOkToProcess = { GmRptFiles.isProd = isProd; GmRptFiles.folderName = inRptFolder }
                                    |> GmRptFiles.getExcelFilenames
                                    |> GmRptFiles.getExcelDtStr
                                    |> GmRptFiles.getExcelDates 
                                    |> GmRptFiles.areExcelFilenamesValid
        if not rptFilesOkToProcess then
            exit 1

        // Create a database context
        use db = DbUtil.getOpenDb dbConnectionString

        // Update Funds from Yahoo Api
        //(new FundsUpdTask(isProd)).DoTask()

        // Extract & Load Daily Everymatrix Report
        Etl.ProcessExcelFolder isProd db inRptFolder outRptFolder

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
* http://stackoverflow.com/questions/14657954/using-nlog-with-f-interactive-in-visual-studio-need-documentation
* https://msdn.microsoft.com/en-us/microsoft-r/hh225374 walkthrough of sql data provider
* http://stackoverflow.com/questions/13768757/how-is-one-supposed-to-use-the-f-sqldataconnection-typeprovider-with-an-app-con
* http://stackoverflow.com/questions/13107676/f-type-provider-for-sql-in-a-class
* http://stackoverflow.com/questions/32312503/f-typeproviders-how-to-change-database?noredirect=1&lq=1
*)


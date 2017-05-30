open NLog
open System
open FSharp.Configuration
open GzBalances
open GzDb
open DbImport
open ArgumentsProcessor

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

let processGm2Gz (db : DbContext)(marketPortfolioShares : PortfolioTypes.PortfoliosPricesMap) =
    try
        logger.Info("Start processing @ UTC : " + DateTime.UtcNow.ToString("s"))
        logger.Info("----------------------------")

        logger.Info("Validating Gm excel rpt files")
        
        let rptFilesOkToProcess = { GmRptFiles.isProd = isProd; GmRptFiles.folderName = inRptFolder }
                                    |> GmRptFiles.getExcelFilenames
                                    |> GmRptFiles.balanceRptDateMatchTitles
                                    |> GmRptFiles.getExcelDtStr
                                    |> GmRptFiles.getExcelDates 
                                    |> GmRptFiles.areExcelFilenamesValid
        if not rptFilesOkToProcess.Valid then
            exit 1

        // Extract & Load Daily Everymatrix Report
        Etl.ProcessExcelFolder isProd db inRptFolder outRptFolder

        (db, rptFilesOkToProcess.DayToProcess, marketPortfolioShares) |||> UserTrx.processGzTrx

        logger.Info("----------------------------")
        logger.Info("Finished processing @ UTC : " + DateTime.UtcNow.ToString("s"))

    with ex ->
        logger.Fatal(ex, "Runtime Exception at main")

let processArgs (db : DbContext)(argResult : HandleShares) =
    match argResult with
        | StoreOnlyShares days -> 
            DailyPortfolioShares.storeShares db
            |> ignore
        | GetShares days -> 
            DailyPortfolioShares.storeShares db
            |> processGm2Gz db

[<EntryPoint>]
let main argv = 

    // Create a database context
    let db = getOpenDb dbConnectionString
    argv 
    |> parseCmdArgs
    |> processArgs db

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


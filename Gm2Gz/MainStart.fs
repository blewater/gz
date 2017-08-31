open NLog
open System
open System.IO
open FSharp.Configuration
open GzBalances
open GzDb
open DbImport
open GzBatchCommon
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

let drive = Path.GetPathRoot  __SOURCE_DIRECTORY__
let inRptFolder = Path.Combine([| drive ; Settings.BaseFolder; Settings.ExcelInFolder |])
let outRptFolder = Path.Combine([| drive ; Settings.BaseFolder; Settings.ExcelOutFolder |])
let currencyRatesUrl = Settings.CurrencyRatesUrl.ToString()

/// Canopy related and excel downloading 
let downloadArgs : ConfigArgs.EverymatriReportsArgsType =
    let everymatrixPortalArgs  : ConfigArgs.EverymatrixPortalArgsType = {
        EmailReportsUser = Settings.ReportsEmailUser;
        EmailReportsPwd = Settings.ReportsEmailPwd;
        EverymatrixPortalUri = Settings.EverymatrixProdPortalUri;
        EverymatrixUsername = Settings.EverymatrixProdUsername;
        EverymatrixPassword = Settings.EverymatrixProdPassword;
        EverymatrixToken = Settings.EverymatrixProdToken;
    }

    let reportsFolders : ConfigArgs.ReportsFoldersType = {
        BaseFolder = Settings.BaseFolder;
        ExcelInFolder = Settings.ExcelInFolder;
        ExcelOutFolder = Settings.ExcelOutFolder;
        reportsDownloadFolder = Settings.ReportsDownloadFolder;
    }

    let reportsFilesArgs : ConfigArgs.ReportsFilesArgsType = {
            DownloadedCustomFilter = Settings.DownloadedCustomFilter;
            DownloadedBalanceFilter = Settings.DownloadedBalanceFilter;
            DownloadedWithdrawalsFilter = Settings.DownloadedWithdrawalsFilter;
            downloadedDepositsFilter = Settings.DownloadedDepositsFilter;
            CustomRptFilenamePrefix = Settings.CustomRptFilenamePrefix;
            EndBalanceRptFilenamePrefix = Settings.EndBalanceRptFilenamePrefix;
            WithdrawalsPendingRptFilenamePrefix = Settings.WithdrawalsPendingRptFilenamePrefix;
            WithdrawalsRollbackRptFilenamePrefix = Settings.WithdrawalsRollbackRptFilenamePrefix;
            depositsRptFilenamePrefix = Settings.DepositsRptFilenamePrefix;
            Wait_For_File_Download_MS = Settings.WaitForFileDownloadMs;
        }

    let everymatrixReportsArgs : ConfigArgs.EverymatriReportsArgsType = {
        EverymatrixPortalArgs = everymatrixPortalArgs;
        ReportsFoldersArgs = reportsFolders;
        ReportsFilesArgs = reportsFilesArgs;
    }
    everymatrixReportsArgs

let gmReports2InvBalanceUpdate 
        (db : DbContext)
        (emailToProcAlone : string option)
        (marketPortfolioShares : PortfolioTypes.PortfoliosPricesMap) =

    logger.Info("Start processing @ UTC : " + DateTime.UtcNow.ToString("s"))
    logger.Info("----------------------------")

    logger.Info("Validating Gm excel rpt files")
        
    let rptFilesOkToProcess = { GmRptFiles.isProd = isProd; GmRptFiles.folderName = inRptFolder }
                                |> GmRptFiles.getExcelFilenames
                                |> GmRptFiles.balanceRptDateMatchTitles
                                |> GmRptFiles.depositsRptContentMatch
                                |> GmRptFiles.getExcelDtStr
                                |> GmRptFiles.getExcelDates 
                                |> GmRptFiles.areExcelFilenamesValid
    if not rptFilesOkToProcess.Valid then
        exit 1

    // Extract & Load Daily Everymatrix Report
    Etl.ProcessExcelFolder isProd db inRptFolder outRptFolder emailToProcAlone

    (db, rptFilesOkToProcess.DayToProcess, marketPortfolioShares) 
    |||> UserTrx.processGzTrx



    logger.Info("----------------------------")
    logger.Info("Finished processing @ UTC : " + DateTime.UtcNow.ToString("s"))

/// Choose next biz processing steps based on args
let portfolioSharesPrelude2MainProcessing (db : DbContext) =
    DailyPortfolioShares.storeShares db

[<EntryPoint>]
let main argv = 

    try 
        // Create a database context
        let db = getOpenDb dbConnectionString

        let cmdArgs = 
            argv |> parseCmdArgs

        // Download reports
        let dwLoader = ExcelDownloader(downloadArgs, cmdArgs.BalanceFilesUsage)
        dwLoader.SaveReportsToInputFolder()

        // Main database processing 
        gmReports2InvBalanceUpdate 
            db 
            cmdArgs.UserEmailProcAlone
            <| portfolioSharesPrelude2MainProcessing db

        // Vintage withdrawal bonus
        WithdrawnVintageBonusGen.updDbRewSoldVintages downloadArgs.EverymatrixPortalArgs downloadArgs.ReportsFoldersArgs db DateTime.UtcNow

        printfn "Press Enter to finish..."
        Console.ReadLine() |> ignore
        0
    with ex ->
        logger.Fatal(ex, "1 or more runtime exceptions at GzBatch")
        1


(*****
* Notes
* http://stackoverflow.com/questions/22608584/how-to-project-transform-an-array-of-fileinfo-to-a-list-of-strings-with-fsharp/22608949#22608949
* http://stackoverflow.com/questions/14657954/using-nlog-with-f-interactive-in-visual-studio-need-documentation
* https://msdn.microsoft.com/en-us/microsoft-r/hh225374 walkthrough of sql data provider
* http://stackoverflow.com/questions/13768757/how-is-one-supposed-to-use-the-f-sqldataconnection-typeprovider-with-an-app-con
* http://stackoverflow.com/questions/13107676/f-type-provider-for-sql-in-a-class
* http://stackoverflow.com/questions/32312503/f-typeproviders-how-to-change-database?noredirect=1&lq=1
*)


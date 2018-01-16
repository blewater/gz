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
            DownloadedDepositsFilter = Settings.DownloadedDepositsFilter;
            DownloadedBonusFilter = Settings.DownloadedBonusFilter;
            CustomRptFilenamePrefix = Settings.CustomRptFilenamePrefix;
            EndBalanceRptFilenamePrefix = Settings.EndBalanceRptFilenamePrefix;
            WithdrawalsPendingRptFilenamePrefix = Settings.WithdrawalsPendingRptFilenamePrefix;
            WithdrawalsRollbackRptFilenamePrefix = Settings.WithdrawalsRollbackRptFilenamePrefix;
            DepositsRptFilenamePrefix = Settings.DepositsRptFilenamePrefix;
            BonusRptFilenamePrefix = Settings.BonusRptFilenamePrefix;
            Wait_For_File_Download_MS = Settings.WaitForFileDownloadMs;
        }

    let everymatrixReportsArgs : ConfigArgs.EverymatriReportsArgsType = {
        EverymatrixPortalArgs = everymatrixPortalArgs;
        ReportsFoldersArgs = reportsFolders;
        ReportsFilesArgs = reportsFilesArgs;
    }
    everymatrixReportsArgs

let gmReports2Db 
        (db : DbContext)
        (emailToProcAlone : string option) =

    logger.Info("Start processing @ UTC : " + DateTime.UtcNow.ToString("s"))
    logger.Info("----------------------------")

    logger.Info("Validating Gm excel rpt files")
        
    let rptFilesOkToProcess = { GmRptFiles.isProd = isProd; GmRptFiles.folderName = inRptFolder }
                                |> GmRptFiles.getExcelFilenames
                                |> GmRptFiles.balanceRptDateMatchTitles
                                |> GmRptFiles.depositsRptContentMatch
                                |> GmRptFiles.bonusRptContentMatch
                                |> GmRptFiles.getExcelDtStr
                                |> GmRptFiles.getExcelDates 
                                |> GmRptFiles.areExcelFilenamesValid
    if not rptFilesOkToProcess.Valid then
        exit 1

    // Extract & Load Daily Everymatrix Report
    Etl.ProcessExcelFolder isProd db inRptFolder outRptFolder emailToProcAlone
    rptFilesOkToProcess.DayToProcess

/// Choose next biz processing steps based on args
let portfolioSharesPrelude2MainProcessing (db : DbContext) =
    DailyPortfolioShares.storeShares db

let getTimer() =
    let timer = System.Diagnostics.Stopwatch()
    timer.Start()
    timer

[<EntryPoint>]
let main argv = 

    let appTimer = getTimer()

    try 
        // Create a database context
        let db = getOpenDb dbConnectionString

        // Rtm args
        let cmdArgs = 
            argv |> parseCmdArgs

        // Download reports
        let dwLoader = ExcelDownloader(downloadArgs, cmdArgs.BalanceFilesUsage)
        dwLoader.SaveReportsToInputFolder()

        // Excel reports -> db
        let customRpdDate = 
            gmReports2Db 
                db 
                cmdArgs.UserEmailProcAlone

        // Clear Monthly Investment User Balances
        UserTrx.processGzTrx
            db
            (customRpdDate.Substring(0, 6))
            (portfolioSharesPrelude2MainProcessing db)
            cmdArgs.UserEmailProcAlone

        // Vintage withdrawal bonus
        WithdrawnVintageBonusGen.updDbRewSoldVintages downloadArgs.EverymatrixPortalArgs downloadArgs.ReportsFoldersArgs db DateTime.UtcNow

        appTimer.Stop()
        logger.Info("----------------------------")
        logger.Info(sprintf "The investment awarding process took %f seconds or %f minutes." <| (float) appTimer.ElapsedMilliseconds/1000.0 <| (float) appTimer.ElapsedMilliseconds/60000.0)
        logger.Info("Finished processing @ UTC : " + DateTime.UtcNow.ToString("s"))

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


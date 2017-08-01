namespace DbImport

open System
open System.IO
open GzBatchCommon
open ConfigArgs
open canopy
open ExcelSchemas
open NLog

type ExcelDownloader(reportsArgs : EverymatriReportsArgsType, balanceFilesArg : BalanceFilesUsageType) =

    static let logger = LogManager.GetCurrentClassLogger()

    let customReportFilenameWithInputPath (dayToProcess : DateTime): string =
        let customRptName = reportsArgs.ReportsFilesArgs.CustomRptFilenamePrefix + dayToProcess.ToYyyyMmDd + ".xlsx"
        let drive = Path.GetPathRoot  __SOURCE_DIRECTORY__
        let destCustomRptName = Path.Combine([| drive; reportsArgs.ReportsFoldersArgs.BaseFolder; reportsArgs.ReportsFoldersArgs.ExcelInFolder; customRptName |])
        destCustomRptName

    let downloadReports(dayToProcess : DateTime)(destCustomRptName : string) : unit =
        logger.Info (sprintf "Download parameters: %A" reportsArgs)
        logger.Info (sprintf "Downloading Custom Report: %s" destCustomRptName)
        let gmailClient = EmailAccess(dayToProcess, reportsArgs.EverymatrixPortalArgs.EmailReportsUser, reportsArgs.EverymatrixPortalArgs.EmailReportsPwd)
        let messageCount = gmailClient.DownloadCustomReport(destCustomRptName)
        logger.Info (sprintf "Found %d message(s), found custom report? %b." messageCount.TotalReportMessages messageCount.FoundDaysReport)

        logger.Info "*** Downloading Rest of reports ***"
        let webDownloader = CanopyDownloader(dayToProcess, reportsArgs)
        webDownloader.DownloadReports(balanceFilesArg)

    member this.SaveReportsToInputFolder() =

        let dayToProcess = DateTime.UtcNow.Date.AddDays(-1.0)
        downloadReports dayToProcess (customReportFilenameWithInputPath dayToProcess)


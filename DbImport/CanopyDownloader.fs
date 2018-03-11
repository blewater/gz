namespace DbImport

open canopy
open System.IO
open System
open GzBatchCommon
open ExcelSchemas
open ConfigArgs
open Microsoft.FSharp.Collections
open NLog

type CanopyDownloader(dayToProcess : DateTime, reportsArgs : EverymatriReportsArgsType) =

    static let logger = LogManager.GetCurrentClassLogger()

    let Wait_For_File_Download_Ms = reportsArgs.ReportsFilesArgs.Wait_For_File_Download_MS
    [<Literal>]
    let ScheduledRptEmailRecipient = "hostmaster@greenzorro.com"

    // Browser efforts to download everymatrix reports
    [<Literal>]
    let NUM_DOWNLOAD_ATTEMPTS = 20

    [<Literal>]
    let WaitBefDownloadRetryinMillis = 1000 // 1 sec
    [<Literal>]
    let WaitForSearchResults = 2000 // 2 sec
    let drive = Path.GetPathRoot  __SOURCE_DIRECTORY__
    let downloadFolderName = Path.Combine (drive, reportsArgs.ReportsFoldersArgs.reportsDownloadFolder)
    let inRptFolderName = Path.Combine ([|drive; reportsArgs.ReportsFoldersArgs.BaseFolder; reportsArgs.ReportsFoldersArgs.ExcelInFolder |])

    //let downloadedCustomFilter = reportsArgs.ReportsFilesArgs.DownloadedCustomFilter

    let rptFilename (rptFilenamePrefix : string)(filenameDatePostfix : DateTime) : string =
        rptFilenamePrefix
        + filenameDatePostfix.ToYyyyMmDd
        + ".xlsx"

    let lastDownloadedRpt (rptFilter : string)(downloadFolderName : string) : FileInfo = 

        (downloadFolderName
            |> DirectoryInfo
        ).GetFiles(rptFilter) 
            |> Array.toList
            |> List.sortByDescending(fun (f) -> f.CreationTime)
            |> List.head

    let overwriteMoveFile (sourceFilename : string)(destFilename : string) : unit = 
        if destFilename |> File.Exists then File.Delete(destFilename)
        File.Move(sourceFilename, destFilename)

    /// Alias of overwriteMoveFile
    let overwriteRenameFile = overwriteMoveFile

    let switchToWindow window =
        browser.SwitchTo().Window(window) |> ignore

    let getOtherWindow currentWindow =
        browser.WindowHandles |> Seq.find (fun w -> w <> currentWindow)

    let switchToOtherWindow currentWindow =
        switchToWindow (getOtherWindow currentWindow) |> ignore

    let closeOtherWindow currentWindow =
        switchToOtherWindow currentWindow
        browser.Close()

    let closeSwitchWindow newWindow =
        browser.Close()
        switchToWindow newWindow

    let uiAutomateLoginEverymatrixReports 
            (everyMatrixUserName : string)
            (everymatrixPassword : string)
            (everymatrixLoginToken : string) : unit =

        url "https://admin3.gammatrix.com/Admin/Login.aspx"
        "#rtbTop_text" << everyMatrixUserName
        "#rtbMid_text" << everymatrixPassword
        "#rtbBottom_text" << everymatrixLoginToken
        click "#btnLogin" 
        click "#ctl00_rmMainMenu > ul > li:nth-child(4) > a"
        let everymatrixAppWindow = browser.CurrentWindowHandle
        let rptWindow = getOtherWindow everymatrixAppWindow
        closeSwitchWindow rptWindow

    /// Remove the last month's scheduled email report
    let removeScheduledEmailCustomReport() : unit =
        // Misc
        click "#nav > li:nth-child(4) > a"
        // Email Report
        click "#nav > li:nth-child(4) > ul > li:first-child > a"
        // List email report
        click "#nav > li:nth-child(4) > ul > li:first-child > ul > li:nth-child(1) > a"
        // Remove last report entry
        // click "#gvList > tbody > tr:nth-last-child(1) > td:nth-last-child(1) > a"
        element ScheduledRptEmailRecipient
        |> parent 
        |> elementWithin "td:nth-last-child(1) > a" 
        |> click
        acceptAlert()
    
    /// Enter from the Main Menu to custom reports
    let enterCustomReports() =
        // Activity
        click "#nav > li:nth-child(1) > a"
        // Customized Report
        click "#nav > li:nth-child(1) > ul > li:nth-child(8) > a"
        // Launch custom report page
        click "#nav > li:nth-child(1) > ul > li:nth-child(8) > ul > li:nth-child(1) > a"

    /// Create a monthly custom rpt
    let createNewCustomMonthRpt () : unit = 
        let today = DateTime.UtcNow

        enterCustomReports()

        click "#hyNewReport"
        "#txtReportName" << today.ToYyyyMm
        click "#btnSave"
        "#ddlContionPropertyName" << "ProductActivityDateRange"
        "#txtStartDate" << 
            "01/" 
            + today.Month.ToString("00") 
            + "/" 
            + today.Year.ToString("00")
        "#txtEndDate" << 
            DateTime.DaysInMonth(today.Year, today.Month)
                .ToString("00") 
                + "/" 
                + today.Month.ToString("00") 
                + "/" 
                + today.Year.ToString("00")
        click "#btnAddCondtions"
        // brand
        uncheck "#cbxDisplayColumns_0"
        // role
        uncheck "#cbxDisplayColumns_7"
        // join date
        check "#cbxDisplayColumns_15"
        // registered by
        uncheck "#cbxDisplayColumns_16"
        // Country
        check "#cbxDisplayColumns_21"
        // Postcode
        uncheck "#cbxDisplayColumns_22"
        // City
        check "#cbxDisplayColumns_23"
        // Preferred language
        uncheck "#cbxDisplayColumns_25"
        // Connected IP address
        uncheck "#cbxDisplayColumns_26"
        // Subscribe to marketing materials
        uncheck "#cbxDisplayColumns_27"
        // Allow SMS Offer
        uncheck "#cbxDisplayColumns_28"
        // Accepts bonueses
        uncheck "#cbxDisplayColumns_29"
        // Affiliate marker
        uncheck "#cbxDisplayColumns_30"
        // Initial deposit date
        check "#cbxDisplayColumns_31"
        // Initial deposit amount
        uncheck "#cbxDisplayColumns_32"
        // Last deposit date
        check "#cbxDisplayColumns_38"
        // check Withdrawas made
        check "#cbxDisplayColumns_39"
        // user payment methods
        uncheck "#cbxDisplayColumns_41"
        // mobile
        check "#cbxDisplayColumns_42"
        // phone
        check "#cbxDisplayColumns_43"
        // last played date
        check "#cbxDisplayColumns_44"
        // Real Balance
        check "#cbxDisplayColumns_54"
        // Suspended balance
        uncheck "#cbxDisplayColumns_55"
        // check Currency
        check "#cbxDisplayColumns_56"

        click "#btnSaveSelectedColumns"
        click "#Button1"
        // Mail/Ftp report
        click "#btnEmailReport"
        let baseWindow = browser.CurrentWindowHandle
        switchToOtherWindow baseWindow
        // Monthly
        "#txtReceivers" << ScheduledRptEmailRecipient
        // scheduled time
        "#txtSentTime" << "0530"
        click "#btnSave"
        browser.Close()
        switchToWindow baseWindow

    /// Check on present if the custom report exists
    let monthlyCustomReportExists() : bool =

        let thisMonth = DateTime.UtcNow.ToYyyyMm
        enterCustomReports()
        let foundMonthIndex = (element "#ddlReport").Text.LastIndexOf(thisMonth)
        match foundMonthIndex with
        | -1 -> false
        | _ -> true

    /// Download the monthly custom report
    let uiAutomateDownloadedCustomRpt (dayToProcess : DateTime) =

        let monthToFind = dayToProcess.ToYyyyMm
        "#ddlReport" << "GreenZorro.com " + monthToFind
        click "#BtnToExcel"
        Threading.Thread.Sleep(Wait_For_File_Download_Ms)

    /// Enter transactions report and set it to withdrawals mode
    let uiAutomatedEnterTransactionsReport() = 
        // Activity
        click "#nav > li:nth-child(1) > a"
        // Transaction
        click "#nav > li:nth-child(1) > ul > li:nth-child(3) > a"

        /// Set the transaction report to the full month withdrawal dates
    let setFormDates (formDateToSet : DateTime) : unit =
        "#txtStartDate" << "01/" + formDateToSet.Month.ToString("00") + "/" + formDateToSet.Year.ToString()
        "#txtEndDate" << formDateToSet.Day.ToString("00") + "/" + formDateToSet.Month.ToString("00") + "/" + formDateToSet.Year.ToString()

    /// Bonus, Transaction Reports: Press show and if there's data download it
    let display2SavedRpt (searchBtnId: string)(downBtnId: string)(searchResultsSel: string)(selectorDataFound : string) : bool =

        try 
            // Raw selenium click
            (element searchBtnId).Click()
            
        with ex ->
            waitForElement searchResultsSel
            match someElement "#TransDetail1_gvTransactionDetails" with
            | Some(_) -> logger.Debug "Found results within the transactions report"
            | None -> raise ex

        let dataFound : bool =
            match fastTextFromCSS selectorDataFound with
            | [] -> true
            | h::t -> h.ToLower() <> "no data"

        if 
            dataFound then
            try 
                // Raw selenium click
                (element downBtnId).Click()
            with ex ->
                logger.Warn(ex, sprintf "Exception after clicking the download button: %s." downBtnId)

            // Workaround for completing the download task without looping
            enabled downBtnId
            Threading.Thread.Sleep(Wait_For_File_Download_Ms)
                
        dataFound

    /// Deposits, cash bonus
    let uiAutomateDownloadedDepositsBonusCashRpt (dayToProcess : DateTime) : bool =
    
        uiAutomatedEnterTransactionsReport()

        setFormDates dayToProcess

        // Deposit
        check "#chkTransType_0"

        // Withdrawals
        uncheck "#chkTransType_1"

        // Vendor2User
        check "#chkTransType_4"

        display2SavedRpt "#btnShowReport" "#btnSaveAsExcel" "#TransDetail1_gvTransactionDetails" "#TransDetail1_gvTransactionDetails > tbody > tr:nth-child(1) > td:nth-child(1)"

    // Into the player bonuses including inv bonus
    let uiEnterBonusReport() = 
        // Activity
        click "#nav > li:nth-child(1) > a"
        // Casino engine
        click "#nav > li:nth-child(1) > ul > li:nth-child(9) > a"
        // Casino bonus-completed bonus
        click "#nav > li:nth-child(1) > ul > li:nth-child(9) > ul > li:nth-child(3) > a"

    // Download Bonuses
    let uiDownloadBonusRpt (dayToProcess : DateTime) : bool =
    
        uiEnterBonusReport()

        setFormDates dayToProcess

        display2SavedRpt "#ButShow" "#ButSave" "#GVC" "#TransDetail1_gvTransactionDetails > tbody > tr:nth-child(1) > td:nth-child(1)"

    /// Pending + Completed Withdrawals: Initiated + Pending + Success
    let uiAutomateDownloadedPendingWithdrawalsRpt (withdrawalDateToSet : DateTime) : bool =
    
        uiAutomatedEnterTransactionsReport()

        setFormDates withdrawalDateToSet

        // Withdrawals
        check "#chkTransType_1"

        // Initiated
        click "#rblDataPeriodType_0"

        // Pending
        check "#cbxTransStatus_3"
        // Success
        check "#cbxTransStatus_2"
        // Rollback
        uncheck "#cbxTransStatus_6"

        // Vendor2User
        uncheck "#chkTransType_4"

        display2SavedRpt "#btnShowReport" "#btnSaveAsExcel" "#TransDetail1_gvTransactionDetails" "#TransDetail1_gvTransactionDetails > tbody > tr:nth-child(1) > td:nth-child(1)"

        /// Withdrawals: Completed + Rollback
    let uiAutomateDownloadedRollbackWithdrawalsRpt (withdrawalDateToSet : DateTime) : bool =
    
        uiAutomatedEnterTransactionsReport()

        setFormDates withdrawalDateToSet

        // Withdrawals
        check "#chkTransType_1"

        // Completed
        click "#rblDataPeriodType_1"

        // Pending
        uncheck "#cbxTransStatus_3"
        // Success
        uncheck "#cbxTransStatus_2"
        // Rollback
        check "#cbxTransStatus_6"

        // Vendor2User
        uncheck "#chkTransType_4"

        display2SavedRpt "#btnShowReport" "#btnSaveAsExcel" "#TransDetail1_gvTransactionDetails" "#TransDetail1_gvTransactionDetails > tbody > tr:nth-child(1) > td:nth-child(1)"

    /// dayToProcess is interpreted @ 23:59 in the report
    let uiAutomatedEndBalanceRpt (dayToProcess : DateTime)(downloadedBalanceFilter : string) : bool =

        let downloadingFailure = true

        // LGA reports
        click "#nav > li:nth-child(5) > a"
        // Player balances
        click "#nav > li:nth-child(5) > ul > li > a"

        // Bizarre report issue: post with date before doesn't work

        // date
        let dateToStrVal = dayToProcess.ToDdMmYYYYSlashed
        "#txtStartDate" << dateToStrVal

        click "#btnShowReport"

        // Open 2nd balance window
        click "#gvSum > tbody > tr:nth-last-child(1) > td:nth-child(2) > a"

        let baseWindow = browser.CurrentWindowHandle
        let sndWindow = browser.WindowHandles |> Seq.find(fun w -> w <> baseWindow)
        switchToWindow sndWindow

        // Open 3rd balance window
        click "#gvSum > tbody > tr:nth-last-child(1) > td:nth-child(7) > a"
        let thirdWindow = browser.WindowHandles |> Seq.find(fun w -> w <> sndWindow && w <> baseWindow)
        closeSwitchWindow thirdWindow
        // Download Report

        let downloadedBalanceReport =
            try 
                click "#ShowProductBalance1_btnSaveas"
                // Sleep 2 seconds to allow for the file to download
                Threading.Thread.Sleep(Wait_For_File_Download_Ms)

                let downloadedBalanceRpt = lastDownloadedRpt downloadedBalanceFilter downloadFolderName
                // Check for incomplete download if last file entry is "bybalance.xlsx.crdownload"
                not <| downloadedBalanceRpt.Name.EndsWith("crdownload")
            with
            | :? OpenQA.Selenium.WebDriverTimeoutException 
                ->  printfn "Absorbed WebDriverTimeoutException during the Balance download"; 
                    false
            | _ -> false

        // Return to base window
        closeSwitchWindow baseWindow
        downloadedBalanceReport

    let moveDownloadedRptToInRptFolder 
            (everymatrixDwnFileMask : string) // "bybalance.xlsx" --or "values.xlsx" --or "transx.xlsx
            (rptFilenamePrefix : string) // "Custom Prod" --or "Balance Prod" --or withdrawalsPending Prod --or withdrawalsRollback Prod 
            (rptFilenameDatePostfix : DateTime) // 20170430
                : unit =

        // "bybalance.xlsx" --or "values.xlsx" --or "transx.xlsx
        let downloadedRpt = lastDownloadedRpt everymatrixDwnFileMask downloadFolderName

        (* 
         * "Custom Prod 20170430.xlsx"
         *  -- Or -- 
         * "Balance Prod 20170430.xlsx" 
         *  -- Or --
         *  "withdrawalsPending Prod 20170430.xlsx" or "withdrawalsRollback Prod 20170430.xlsx"
         *)
        let correctRptFilename = rptFilename rptFilenamePrefix rptFilenameDatePostfix

        let finalDestinationRptName = Path.Combine(inRptFolderName, correctRptFilename)

        let downloadedCorrectRptFilename = Path.Combine(downloadFolderName, correctRptFilename)
        overwriteRenameFile (downloadedRpt.FullName) downloadedCorrectRptFilename
        overwriteMoveFile downloadedCorrectRptFilename finalDestinationRptName

    let initCanopyChrome() : unit =
        let projDir = __SOURCE_DIRECTORY__
        canopy.configuration.chromeDir <- projDir
        // 10 minute timeouts
        canopy.configuration.elementTimeout <- 600.0
        canopy.configuration.compareTimeout <- 600.0
        canopy.configuration.pageTimeout <- 600.0
        start chrome
        match screen.monitorCount with
        | m when m >= 3 -> pinToMonitor 3
        | m when m = 2 -> pinToMonitor 2
        | _ -> ()

    /// UI Web Automate with Everymatrix reporting site and download reports    
    let uiAutomationDownloading 
                (customEmailRptNotFound : bool)
                (dayToProcess : DateTime)
                (reportsArgs : EverymatriReportsArgsType)
                (balanceFilesArg : BalanceFilesUsageType) =

        let portalArgs = reportsArgs.EverymatrixPortalArgs

        // start chrome and login to everymatrix backo.
        let initNewBrowserSession() : unit =
            do initCanopyChrome()

            // Login
            uiAutomateLoginEverymatrixReports 
                portalArgs.EverymatrixUsername 
                portalArgs.EverymatrixPassword 
                portalArgs.EverymatrixToken

        do initNewBrowserSession()

        // helper for exceptions out of logged out sessions
        let rec retry times fn : unit = 
            if times > 0 then
                try
                    if times < NUM_DOWNLOAD_ATTEMPTS then
                        logger.Info (sprintf "** retry remaining efforts: %d" times)
                    fn()

                with ex ->
                    logger.Warn(ex, "Downloading exception")
                    System.Threading.Thread.Sleep(WaitBefDownloadRetryinMillis);
                    quit()
                    do initNewBrowserSession()
                    retry (times - 1) fn
            else
                failwithf "Quiting after exhausting %d number of report downloading tries." NUM_DOWNLOAD_ATTEMPTS

        let rptFilesArgs = reportsArgs.ReportsFilesArgs
    

        // Create Custom Scheduled Report
        if not <| monthlyCustomReportExists() then
            removeScheduledEmailCustomReport()
            createNewCustomMonthRpt()
        
        // Custom download with retry
        let rec retryCustomRpt times : unit = 
            if times > 0 then
                try
                    if times < NUM_DOWNLOAD_ATTEMPTS then
                        logger.Info (sprintf "** downloading the Custom Report: left remaining efforts: %d" times)

                    uiAutomateDownloadedCustomRpt dayToProcess
                    moveDownloadedRptToInRptFolder 
                        rptFilesArgs.DownloadedCustomFilter 
                        rptFilesArgs.CustomRptFilenamePrefix
                        dayToProcess

                with ex ->
                    logger.Warn(ex, "Custom report downloading exception")
                    System.Threading.Thread.Sleep(WaitBefDownloadRetryinMillis);
                    quit()
                    do initNewBrowserSession()
                    retryCustomRpt (times - 1)

            elif customEmailRptNotFound then
                failwithf "Quiting after failed to download the emailed custom report and exhausting %d number of custom report downloading tries." NUM_DOWNLOAD_ATTEMPTS

        retryCustomRpt NUM_DOWNLOAD_ATTEMPTS

        // Deposits
        let downloadDeposits() =
            if uiAutomateDownloadedDepositsBonusCashRpt dayToProcess then
                moveDownloadedRptToInRptFolder
                    rptFilesArgs.DownloadedDepositsFilter
                    rptFilesArgs.DepositsRptFilenamePrefix 
                    dayToProcess
        retry NUM_DOWNLOAD_ATTEMPTS downloadDeposits

        // Inv Bonus Deposits
        let downloadBonus() =
            if uiDownloadBonusRpt dayToProcess then
                moveDownloadedRptToInRptFolder 
                    rptFilesArgs.DownloadedBonusFilter
                    rptFilesArgs.BonusRptFilenamePrefix
                    dayToProcess
        retry NUM_DOWNLOAD_ATTEMPTS downloadBonus

        // Withdrawals: Pending
        let downloadWithdrawalsPending() = 
            if uiAutomateDownloadedPendingWithdrawalsRpt dayToProcess then
                moveDownloadedRptToInRptFolder 
                    rptFilesArgs.DownloadedWithdrawalsFilter
                    rptFilesArgs.WithdrawalsPendingRptFilenamePrefix
                    dayToProcess
        retry NUM_DOWNLOAD_ATTEMPTS downloadWithdrawalsPending

        // Withdrawals: Rollback
        let downloadWithdrawalsRollback() = 
            if uiAutomateDownloadedRollbackWithdrawalsRpt dayToProcess then
                moveDownloadedRptToInRptFolder 
                    rptFilesArgs.DownloadedWithdrawalsFilter
                    rptFilesArgs.WithdrawalsRollbackRptFilenamePrefix
                    dayToProcess
        retry NUM_DOWNLOAD_ATTEMPTS downloadWithdrawalsRollback

        // Balance: report date counts as day + 23:59
        let downloadEndofMonthBalanceReport() =
            if balanceFilesArg = BalanceFilesUsageType.UseBothBalanceFiles then
                if uiAutomatedEndBalanceRpt dayToProcess rptFilesArgs.DownloadedBalanceFilter then
                    moveDownloadedRptToInRptFolder 
                        rptFilesArgs.DownloadedBalanceFilter
                        rptFilesArgs.EndBalanceRptFilenamePrefix
                        (dayToProcess.AddDays(1.0))
        retry NUM_DOWNLOAD_ATTEMPTS downloadEndofMonthBalanceReport

        quit ()

    member this.DownloadReports
            (balanceFilesArg : BalanceFilesUsageType)
            (customEmailRptNotFound : bool) : unit =

        uiAutomationDownloading customEmailRptNotFound dayToProcess reportsArgs balanceFilesArg
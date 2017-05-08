#I __SOURCE_DIRECTORY__
#r "System.Core.dll"
#r "System.Data.dll"
#r "System.Data.Linq.dll"
#r "System.dll"
#r "System.Numerics.dll"
#r "System.Transactions.dll"
#r "System.Xml.Linq.dll"
#r "System.Drawing.dll"
#r "../packages/Selenium.WebDriver/lib/net40/WebDriver.dll"
#r "../../GzBatch/packages/Selenium.Support.3.4.0/lib/net40/WebDriver.Support.dll"
#r "../packages/canopy/lib/canopy.dll"
#r "../../GzBatch/packages/System.ValueTuple.4.3.0/lib/netstandard1.0/System.ValueTuple.dll"
#r "../../GzCommon/bin/Production/GzCommon.dll"

open canopy
open System.IO
open System
open Microsoft.FSharp.Collections
open GzCommon

let everymatrixUsername = "admin"
let everymatrixPassword = "MoneyLine8!"
let everymatrixSecureToken = "3DFEC757D808494"

let downloadFolderName = @"d:\download\"

let inRptFolderName = @"d:\sc\gz\inRpt\"

let downloadedCustomFilter = "values*.xlsx"
let downloadedBalanceFilter = "byBalance*.xlsx"
let downloadedWithdrawalsFilter = "trans*.xlsx"

let customRptFilenamePrefix = "Custom Prod "
let endBalanceRptFilenamePrefix = "Balance Prod "
let withdrawalsPendingRptFilenamePrefix = "withdrawalsPending Prod "
let withdrawalsRollbackRptFilenamePrefix = "withdrawalsRollback Prod "

let dayToProcess = DateTime.UtcNow.AddDays(-1.0)
let yyyyMmDdString (date : DateTime) = 
      date.Year.ToString() 
    + date.Month.ToString("00") 
    + date.Day.ToString("00")

let rptFilename (rptFilenamePrefix : string)(filenameDatePostfix : DateTime) : string =
    rptFilenamePrefix
    + (filenameDatePostfix |> yyyyMmDdString)
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

/// Create a monthly custom rpt
let createNewCustomMonthRpt (dayToProcess : DateTime) : unit = 
    click "#hyNewReport"
    "#txtReportName" << dayToProcess.ToYyyyMm
    click "#btnSave"
    "#ddlContionPropertyName" << "ProductActivityDateRange"
    "#txtStartDate" << 
        "01/" 
        + dayToProcess.Month.ToString("00") 
        + "/" 
        + dayToProcess.Year.ToString("00")
    "#txtEndDate" << 
        DateTime.DaysInMonth(dayToProcess.Year, dayToProcess.Month)
            .ToString("00") 
            + "/" 
            + dayToProcess.Month.ToString("00") 
            + "/" 
            + dayToProcess.Year.ToString("00")
    click "#btnAddCondtions"
    // brand
    uncheck "#cbxDisplayColumns_0"
    // role
    uncheck "#cbxDisplayColumns_7"
    // join date
    uncheck "#cbxDisplayColumns_15"
    // registered by
    uncheck "#cbxDisplayColumns_16"
    // Country
    uncheck "#cbxDisplayColumns_21"
    // Postcode
    uncheck "#cbxDisplayColumns_22"
    // City
    uncheck "#cbxDisplayColumns_23"
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
    uncheck "#cbxDisplayColumns_31"
    // Initial deposit amount
    uncheck "#cbxDisplayColumns_32"
    // check Withdrawas made
    check "#cbxDisplayColumns_39"
    // user payment methods
    uncheck "#cbxDisplayColumns_41"
    // Suspended balance
    uncheck "#cbxDisplayColumns_55"
    // check Currency
    check "#cbxDisplayColumns_56"

    click "#btnSaveSelectedColumns"
    click "#Button1"

/// Download the monthly custom report
let uiAutomateDownloadedCustomRpt (dayToProcess : DateTime) =
    // Activity
    click "#nav > li:nth-child(1) > a"
    // Customized Report
    click "#nav > li:nth-child(1) > ul > li:nth-child(8) > a"
    // Launch custom report page
    click "#nav > li:nth-child(1) > ul > li:nth-child(8) > ul > li:nth-child(1) > a"
    let monthToFind = dayToProcess.ToYyyyMm
    let foundMonthIndex = (element "#ddlReport").Text.LastIndexOf(monthToFind)
    if foundMonthIndex = -1 then
        createNewCustomMonthRpt dayToProcess
    "#ddlReport" << "GreenZorro.com " + monthToFind
    click "#BtnToExcel"

    /// Enter transactions report and set it to withdrawals mode
let uiAutomatedEnterWithdrawalReport() = 
    // Activity
    click "#nav > li:nth-child(1) > a"
    // Transaction
    click "#nav > li:nth-child(1) > ul > li:nth-child(3) > a"
    // Withdrawals
    check "#chkTransType_1"

    /// Withdrawals Reports: Press show and if there's data download it
let display2SavedRpt (elemSelector : string) : bool =

    click "#btnShowReport"

    let dataFound : bool =  
        match fastTextFromCSS elemSelector with
        | [] -> true
        | h::t -> not (h = "no data")

    if 
        dataFound then
        
        click "#btnSaveAsExcel"
    dataFound

    /// Set the transaction report to the full month withdrawal dates
let setWithdrawalDates (withdrawalDateToSet : DateTime) : unit =
    "#txtStartDate" << "01/" + withdrawalDateToSet.Month.ToString("00") + "/" + withdrawalDateToSet.Year.ToString()
    "#txtEndDate" << withdrawalDateToSet.Day.ToString("00") + "/" + withdrawalDateToSet.Month.ToString("00") + "/" + withdrawalDateToSet.Year.ToString()

    /// Withdrawals: Initiated + Pending + Rollback
let uiAutomateDownloadedPendingWithdrawalsRpt (withdrawalDateToSet : DateTime) : bool =
    
    uiAutomatedEnterWithdrawalReport()

    setWithdrawalDates withdrawalDateToSet

    // Initiated
    click "#rblDataPeriodType_0"

    // Pending
    check "#cbxTransStatus_3"
    // Success
    uncheck "#cbxTransStatus_2"
    // Rollback
    check "#cbxTransStatus_6"

    display2SavedRpt "#TransDetail1_gvTransactionDetails > tbody > tr:nth-child(1) > td:nth-child(1)"

    /// Withdrawals: Completed + Rollback
let uiAutomateDownloadedRollbackWithdrawalsRpt (withdrawalDateToSet : DateTime) : bool =
    
    uiAutomatedEnterWithdrawalReport()

    setWithdrawalDates withdrawalDateToSet

    // Completed
    click "#rblDataPeriodType_1"

    // Pending
    uncheck "#cbxTransStatus_3"
    // Success
    uncheck "#cbxTransStatus_2"
    // Rollback
    check "#cbxTransStatus_6"

    display2SavedRpt "#TransDetail1_gvTransactionDetails > tbody > tr:nth-child(1) > td:nth-child(1)"

let uiAutomatedEndBalanceRpt (dayToProcess : DateTime) : bool =
    // LGA reports
    click "#nav > li:nth-child(5) > a"
    // Player balances
    click "#nav > li:nth-child(5) > ul > li > a"
    // date
    "#txtStartDate" << dayToProcess.Day.ToString("00") + "/" + dayToProcess.Month.ToString("00") + "/" + dayToProcess.Year.ToString()

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
    click "#ShowProductBalance1_btnSaveas"
    
    // Return to base window
    closeSwitchWindow baseWindow
    true

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

    let finalDestinationRptName = inRptFolderName + correctRptFilename

    let downloadedCorrectRptFilename = downloadFolderName + correctRptFilename
    overwriteRenameFile (downloadedRpt.FullName) downloadedCorrectRptFilename
    overwriteMoveFile downloadedCorrectRptFilename finalDestinationRptName

type DownloadedReports =
        {
            WithdrawalPendingDownloaded : bool;
            WithdrawalRollbackDownloaded : bool;
            EndBalaceDownloaded : bool;
        }

/// UI Web Automate with Everymatrix reporting site and download reports    
let uiAutomationDownloading (dayToProcess : DateTime) : DownloadedReports =
    let projDir = Path.Combine(__SOURCE_DIRECTORY__, "..")
    canopy.configuration.chromeDir <- projDir
    start chrome
    pinToMonitor 3

    uiAutomateLoginEverymatrixReports everymatrixUsername everymatrixPassword everymatrixSecureToken
    uiAutomateDownloadedCustomRpt dayToProcess
    let isPendingWithdrawalRptDown = uiAutomateDownloadedPendingWithdrawalsRpt dayToProcess
    let isRollbackWithdrawalRptDown = uiAutomateDownloadedRollbackWithdrawalsRpt dayToProcess
    // Complete end of the month processing with End of Balance Report
    let isEndBalanceRptDown = 
        match DateTime.UtcNow.Day with
        | 1 -> uiAutomatedEndBalanceRpt dayToProcess
        | _ -> false
    quit ()
    { WithdrawalPendingDownloaded = isPendingWithdrawalRptDown;  WithdrawalRollbackDownloaded = isRollbackWithdrawalRptDown; EndBalaceDownloaded = isEndBalanceRptDown }

let downloadedReports = uiAutomationDownloading dayToProcess

// Custom
moveDownloadedRptToInRptFolder downloadedCustomFilter customRptFilenamePrefix dayToProcess

if downloadedReports.WithdrawalPendingDownloaded then
    moveDownloadedRptToInRptFolder downloadedWithdrawalsFilter withdrawalsPendingRptFilenamePrefix dayToProcess

if downloadedReports.WithdrawalRollbackDownloaded then
    moveDownloadedRptToInRptFolder downloadedWithdrawalsFilter withdrawalsRollbackRptFilenamePrefix dayToProcess

// End balance
if downloadedReports.EndBalaceDownloaded then
    moveDownloadedRptToInRptFolder downloadedBalanceFilter endBalanceRptFilenamePrefix DateTime.UtcNow
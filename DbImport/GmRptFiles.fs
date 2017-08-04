﻿namespace DbImport

module GmRptFiles =
    open System
    open System.IO
    open System.Text.RegularExpressions
    open GzBatchCommon
    open ExcelSchemas

    type FolderNameType = string

    // Filename types
    type CustomFilenameType = string
    type DepositsFilenameType = string option
    type WithdrawalsPendingFilenameType = string option
    type WithdrawalsRollingFilenameType = string option
    type BegBalanceFilenameType = string option
    type EndBalanceFilenameType = string option

    // Filename string dates
    type CustomStrDateType = string
    type DepositsStrDateType = string option
    type WithdrawalsPendingStrDateType = string option
    type WithdrawalsRollbackStrDateType = string option
    type BegBalanceStrDateType = string option
    type EndBalanceStrDateType = string option

    // Filename dates
    type CustomDateType = DateTime
    type DepositsDateType = DateTime option
    type WithdrawalsPendingDateType = DateTime option
    type WithdrawalsRollbackDateType = DateTime option
    type BegBalanceDateType = DateTime option
    type EndBalanceDateType = DateTime option

    type InRptFolder = { isProd : bool; folderName : FolderNameType }
    type RptFilenames = { customFilename : CustomFilenameType; DepositsFilename : DepositsFilenameType; withdrawalsPendingFilename : WithdrawalsPendingFilenameType; withdrawalsRollbackFilename : WithdrawalsRollingFilenameType; begBalanceFilename : BegBalanceFilenameType; endBalanceFilename : EndBalanceFilenameType}
    type RptStrDates = { customDtStr : CustomStrDateType; DepositsDtStr : DepositsStrDateType; withdrawalsPendingDtStr : WithdrawalsPendingStrDateType; withdrawalsRollbackDtStr : WithdrawalsRollbackStrDateType; begBalanceDtStr : BegBalanceStrDateType; endBalanceDtStr : EndBalanceStrDateType}
    type RptDates = { customDate : CustomDateType; DepositsDate : DepositsDateType; withdrawalsPendingDate : WithdrawalsPendingDateType; withdrawalsRollbackDate : WithdrawalsRollbackDateType; begBalanceDate : BegBalanceDateType; endBalanceDate : EndBalanceDateType}

    type ExcelDatesValid = { Valid : bool; DayToProcess : string }

    type DirectoryListIndex =
        | FirstAsc
        | LastAsc

    [<Literal>]
    let CustomFileListPrefix = "Custom"
    [<Literal>]
    let Vendor2UserFileListPrefix = "Deposits"
    [<Literal>]
    let WithdrawalPendingFileListPrefix = "withdrawalsPending"
    [<Literal>]
    let WithdrawalRollbackFileListPrefix = "withdrawalsRollback"
    [<Literal>]
    let BalanceFileListPrefix = "Balance"

    let private folderTryF (isProd : bool) f domainException =
        let logInfo = "isProd", isProd
        tryF f domainException logInfo

    /// Read the date part of the excel filename: assuming there's a date part in the filename
    let private datePartofFilename (filename : string) = 
        let len = filename.Length
        filename.Substring(len - 13, 8)

    let validateDateOnExcelFilename (filename : string) : string =
        if Regex.Match(filename, "\d{8}\.xlsx$", RegexOptions.Compiled &&& RegexOptions.CultureInvariant).Success then 
            let dateStrPart = datePartofFilename filename
            match DateTime.TryParseExact(dateStrPart, "yyyymmdd", null, Globalization.DateTimeStyles.None) with
            | true, _ -> dateStrPart
            | false, _ -> failWithLogInvalidArg "[NoparsableDateInFilename]" (sprintf "No date in filename of %s, with date part of: %s." filename dateStrPart)
        else
            failWithLogInvalidArg "[MissingDateInFilename]" (sprintf "No date in filename of %s." filename)

    let private getExcelSortedAscFileList (inRptFolder : InRptFolder) (filenameMask : string) : string list=
        // Deconstruct
        let { isProd = isProd; folderName = folderName} = inRptFolder

        let fileMask = if isProd then filenameMask + " Prod*.xlsx" else filenameMask + " Stage*.xlsx"

        Directory.GetFiles(folderName, fileMask) 
            |> Array.toList 
            |> List.sort
    
    let private getFolderFilenameByMaskIndex (inRptFolder : InRptFolder) (filenameMask : string) (dirListIndex : DirectoryListIndex): string =
        let sortedExcelList = 
            (inRptFolder, filenameMask) 
                ||> getExcelSortedAscFileList

        if sortedExcelList.Length > 0 then
            match dirListIndex with
            | FirstAsc -> sortedExcelList.Item 0
            | LastAsc -> sortedExcelList.Item (sortedExcelList.Length - 1)
        elif sortedExcelList.Length = 0 then
            failWithLogInvalidArg "[EmptyReportFolder]" (sprintf "No excel file within in report folder %s for filename mask %s." inRptFolder.folderName filenameMask)
        else
            failWithLogInvalidArg "[Mismatched index of requested file]" (sprintf "No excel file for requested index: %A within in report folder: %s for filename mask: %s." dirListIndex inRptFolder.folderName filenameMask)
    
    let private getOptionalExcelFilenameByIndex (inRptFolder : InRptFolder) (filenameMask : string) (dirListIndex : DirectoryListIndex): string option=
        
        let sortedExcelList = (inRptFolder, filenameMask) ||> getExcelSortedAscFileList

        if sortedExcelList.Length > 0 then
            match dirListIndex with
            | FirstAsc -> Some <| sortedExcelList.Item 0
            | LastAsc -> Some <| sortedExcelList.Item (sortedExcelList.Length - 1)
        else
            None
    
    /// Get the custom excel rpt filename
    let private getLatestCustomExcelRptFilename (inRptFolder : InRptFolder) : string =
        let readDir () = getFolderFilenameByMaskIndex inRptFolder CustomFileListPrefix LastAsc
        folderTryF inRptFolder.isProd readDir MissingCustomReport
    
    /// Get first balance filename
    let private getMinBalanceRptExcelDirList (inRptFolder : InRptFolder) : BegBalanceFilenameType = 
        let readDir () = 
            (inRptFolder, BalanceFileListPrefix, FirstAsc)  (** TopAsc for Beginning Balance **)
            |||> getOptionalExcelFilenameByIndex

        folderTryF inRptFolder.isProd readDir Missing1stBalanceReport

    /// Get the 2nd balance filename
    let private getNxtBalanceRptExcelDirList (inRptFolder : InRptFolder) : EndBalanceFilenameType = 
        let readDir () = 
            (inRptFolder, BalanceFileListPrefix, LastAsc)  (** LastAsc for EndBalance **)
            |||> getOptionalExcelFilenameByIndex

        folderTryF inRptFolder.isProd readDir Missing2ndBalanceReport

    /// Return the candidate filename only its title date matches the custom filename title date
    let private getFilteredExcelFilename (candidateExcelFilename : string)(customFilename : CustomFilenameType) : string option= 
        let customYyyyMm = 
            (customFilename 
                |> validateDateOnExcelFilename)
                .Substring(0, 6)
        let compareToYyyyMm = 
            (candidateExcelFilename 
                |> validateDateOnExcelFilename)
                .Substring(0, 6)
        if customYyyyMm = compareToYyyyMm then
            Some candidateExcelFilename
        else 
            None

    /// Read the directory() trying to find a filename match for the in param rptFilenameStartingMask
    let private getOptionalExcelRptFilenameInSyncWithCustomDate
                    (rptFilenameStartingMask : string)
                    (inRptFolder : InRptFolder)
                    (customFilename : CustomFilenameType) : string option =

        let readDir () = 
            (inRptFolder, rptFilenameStartingMask, LastAsc) 
                |||> getOptionalExcelFilenameByIndex
                |>  function
                    | Some filename -> getFilteredExcelFilename filename customFilename
                    | _ -> None
        
        folderTryF inRptFolder.isProd readDir MissingWithdrawalReport
    
 //-------- public Excel Filename functions

    let getCustomFilename (inRptFolder : InRptFolder) : string = 
        getLatestCustomExcelRptFilename inRptFolder

    let getDepositsFilename (inRptFolder : InRptFolder)(customFilename : CustomFilenameType) : DepositsFilenameType = 
        (Vendor2UserFileListPrefix , inRptFolder, customFilename) 
            |||> getOptionalExcelRptFilenameInSyncWithCustomDate 

    let getWithdrawalPendingFilename (inRptFolder : InRptFolder)(customFilename : CustomFilenameType) : WithdrawalsPendingFilenameType = 
        (WithdrawalPendingFileListPrefix , inRptFolder, customFilename) 
            |||> getOptionalExcelRptFilenameInSyncWithCustomDate 

    let getWithdrawalsRollbackFilename (inRptFolder : InRptFolder)(customFilename : CustomFilenameType) : WithdrawalsRollingFilenameType = 
        (WithdrawalRollbackFileListPrefix, inRptFolder, customFilename) 
            |||> getOptionalExcelRptFilenameInSyncWithCustomDate 

    let getBegBalanceFilename (inRptFolder : InRptFolder) : BegBalanceFilenameType = 
        getMinBalanceRptExcelDirList inRptFolder

    let getEndBalanceFilename (inRptFolder : InRptFolder) : EndBalanceFilenameType = 
        getNxtBalanceRptExcelDirList inRptFolder
       
    let private getRptFilenames(inRptFolder : InRptFolder) : RptFilenames = 
        let customFilename = 
            inRptFolder 
            |> getCustomFilename 
        { 
            customFilename = customFilename
            DepositsFilename = getDepositsFilename inRptFolder customFilename
            withdrawalsPendingFilename = getWithdrawalPendingFilename inRptFolder customFilename
            withdrawalsRollbackFilename = getWithdrawalsRollbackFilename inRptFolder customFilename
            begBalanceFilename = getBegBalanceFilename inRptFolder
            endBalanceFilename = getEndBalanceFilename inRptFolder
        }

    let getExcelFilenames (inRptFolder : InRptFolder) : RptFilenames =
        let excelFiles = getRptFilenames inRptFolder
        let datesLogMsg = sprintf "Excel filenames: \n%A" excelFiles
        logger.Info datesLogMsg
        excelFiles

 //-------- Excel Date String functions

    let getCustomDtStr (customFilename : string) : string = 
        customFilename 
            |> validateDateOnExcelFilename

    let getOptionalFilenameDateValidation (excelFilename : string option) =
        match excelFilename with
        | Some filename -> Some (filename |> validateDateOnExcelFilename)
        | None -> None

    let getVendor2UserDtStr (vendor2UserFilename : string option) : string option = 
        vendor2UserFilename 
            |> getOptionalFilenameDateValidation

    let getWithdrawalDtStr (withdrawalFilename : string option) : string option = 
        withdrawalFilename 
            |> getOptionalFilenameDateValidation

    let getBegBalanceDtStr (begBalanceFilename : BegBalanceFilenameType) : BegBalanceStrDateType = 
        match begBalanceFilename with
        | Some filename -> Some (validateDateOnExcelFilename filename)
        | None -> None

    let getEndBalanceDtStr (endBalanceFilename : EndBalanceFilenameType) : EndBalanceFilenameType = 
        getBegBalanceDtStr endBalanceFilename 

    let private excelFilenames2DatesStr(excelFiles : RptFilenames) : RptStrDates = 
        { 
            customDtStr = getCustomDtStr excelFiles.customFilename;
            DepositsDtStr = getVendor2UserDtStr excelFiles.DepositsFilename; 
            withdrawalsPendingDtStr = getWithdrawalDtStr excelFiles.withdrawalsPendingFilename; 
            withdrawalsRollbackDtStr = getWithdrawalDtStr excelFiles.withdrawalsRollbackFilename; 
            begBalanceDtStr = getBegBalanceDtStr excelFiles.begBalanceFilename;
            endBalanceDtStr = getEndBalanceDtStr excelFiles.endBalanceFilename;
        }

    /// Get the string dates of all the parsed report filenames string dates in format yyyyMMdd
    let getExcelDtStr (excelFiles : RptFilenames) : RptStrDates =
        let excelFileStrDates = excelFilenames2DatesStr excelFiles
        let datesLogMsg = sprintf "Parsed string dates in excel filenames: \n%A" excelFileStrDates
        logger.Info datesLogMsg
        excelFileStrDates

 //-------- Excel DateTime functions

    let getCustomDate (customDtStr : CustomStrDateType) : CustomDateType = 
        customDtStr.ToDateWithDay

    let getOptionalDtFromFilename (filenameDtStr : string option) =
        match filenameDtStr with
        | Some dateString -> Some dateString.ToDateWithDay
        | None -> None

    let getVendor2UserlDate (vendor2UserDtStr : DepositsStrDateType) : DepositsDateType = 
        vendor2UserDtStr 
            |> getOptionalDtFromFilename

    let getWithdrawalPendingDate (withdrawalDtStr : WithdrawalsPendingStrDateType) : WithdrawalsPendingDateType = 
        withdrawalDtStr 
            |> getOptionalDtFromFilename

    let getWithdrawalsRollingDate (withdrawalDtStr : WithdrawalsRollbackStrDateType) : WithdrawalsRollbackDateType = 
        withdrawalDtStr 
            |> getOptionalDtFromFilename

    let getBegBalanceDate (begBalanceDtStr : BegBalanceStrDateType) : BegBalanceDateType = 
        match begBalanceDtStr with
        | Some dateString -> Some dateString.ToDateWithDay
        | None -> None

    let getEndBalanceDate (endBalanceDtStr : EndBalanceStrDateType) : EndBalanceDateType = 
        getBegBalanceDate endBalanceDtStr

    let private excelDatesStr2Dt(excelDatesStr : RptStrDates) : RptDates = 
        { 
            customDate = getCustomDate excelDatesStr.customDtStr;
            DepositsDate = getVendor2UserlDate excelDatesStr.DepositsDtStr;
            withdrawalsPendingDate = getWithdrawalPendingDate excelDatesStr.withdrawalsPendingDtStr;
            withdrawalsRollbackDate = getWithdrawalsRollingDate excelDatesStr.withdrawalsRollbackDtStr;
            begBalanceDate = getBegBalanceDate excelDatesStr.begBalanceDtStr; 
            endBalanceDate = getEndBalanceDate excelDatesStr.endBalanceDtStr
        }
    /// Get the DateTime dates of all the parsed report filenames string dates in format yyyyMMdd
    let getExcelDates (excelDatesStr : RptStrDates) : RptDates =
        let excelFileDates = excelDatesStr2Dt excelDatesStr
        let datesLogMsg = sprintf "Parsed dates in excel filenames: \n%A" excelFileDates
        logger.Info datesLogMsg
        excelFileDates

//--------------------- Content validation

    /// Check if deposits excel report is really that type of excel report
    let private depositsRptContentMatchCheck(excelFilename : DepositsFilenameType) : bool =

        match excelFilename with
        | Some depositsFilename ->
            let depositsExcel = new DepositsExcelSchema(depositsFilename)

            // Keep the first data row
            let depositsAmounts = depositsExcel.Data |> Seq.truncate 2
            let len = depositsAmounts |> Seq.length 
            if len <= 1 then
                true // header + total line only-> empty balance file
            else
                depositsAmounts
                |> Seq.tryFindIndex(fun (excelRow) ->
                    let depositsExcelTransType = excelRow.``Trans type``
                    depositsExcelTransType = "Vendor2User"
                )   
                |> function
                    | None -> false // this is not a Deposits excel file
                    | _ -> true // Ok it's Deposits file
        | None ->
            true // return ok

    /// Enforce beginning and ending balance Report files title dates match their content
    let depositsRptContentMatch(excelFiles : RptFilenames) : RptFilenames =
        if not (excelFiles.DepositsFilename
                    |> depositsRptContentMatchCheck) then 
            failWithLogInvalidArg "[DepositsFileNotMatchingContent]" (sprintf "The deposits excel report filename: %s does not match its contents." excelFiles.DepositsFilename.Value)
        excelFiles

    /// Check if balance report dates content mismatches title. Precondition is excelFilename is Some  
    let private matchBalanceRptDatesWithTitle(excelFilename : string) : bool =
        let balanceRptDate =  
            Some excelFilename 
                |> getBegBalanceDtStr 
                |> getBegBalanceDate

        let balanceFileExcelSchema = new BalanceExcelSchema(excelFilename)

        // Keep the first data row
        let balances = balanceFileExcelSchema.Data |> Seq.truncate 2
        let len = balances |> Seq.length 
        if len <= 1 then
            true // header + total line only-> empty balance file
        else
            balances
            |> Seq.tryFindIndex(fun (excelRow) ->
                let excelRowDtStr = DateTime.ParseExact(excelRow.Date, "dd/MM/yyyy", null, Globalization.DateTimeStyles.None)
                excelRowDtStr.AddDays(1.0) = balanceRptDate.Value
            )   
            |> function
                | None -> false // Row date mismatch with title
                | _ -> true // Content dates match with title

    /// Enforce beginning and ending balance Report files title dates match their content
    let balanceRptDateMatchTitles(excelFiles : RptFilenames) : RptFilenames =
        if excelFiles.begBalanceFilename.IsSome then
            if not (excelFiles.begBalanceFilename.Value 
                        |> matchBalanceRptDatesWithTitle) then 
                failWithLogInvalidArg "[Begin_BalanceRptDatesNotMathingTitle]" (sprintf "The beginning balance excel report filename: %s does not match its contents." excelFiles.begBalanceFilename.Value)

        if excelFiles.endBalanceFilename.IsSome then
            if not (excelFiles.endBalanceFilename.Value
                        |> matchBalanceRptDatesWithTitle) then 
                failWithLogInvalidArg "[Ending_BalanceRptDatesNotMathingTitle]" (sprintf "The ending balance excel report filename: %s does not match its contents." excelFiles.endBalanceFilename.Value)
        excelFiles

//--------------------- Validation Rules

    /// Enforce same day
    let private sameDayCustomWithdrawalPendingValidation (customDate : DateTime) (withdrawalsPendingDate : WithdrawalsPendingDateType) : Unit =
        match withdrawalsPendingDate with
        | Some withdrawalDate -> 
            if customDate <> withdrawalDate then
                failWithLogInvalidArg "[MismatchedFilenameDates]" (sprintf "Custom date %s mismatch with Withdrawal Pending Date: %s." <| customDate.ToString("yyyy-MMM-dd") <| withdrawalDate.ToString("yyyy-MMM-dd"))
        | None -> 
            logger.Warn "No Pending withdrawals excel file to validate."
            
    let private sameDayCustomWithdrawalRollbackValidation (customDate : DateTime) (withdrawalRollingDate : WithdrawalsRollbackDateType) : Unit =
        match withdrawalRollingDate with
        | Some withdrawalDate -> 
            if customDate <> withdrawalDate then
                failWithLogInvalidArg "[MismatchedFilenameDates]" (sprintf "Custom date %s mismatch with Withdrawal Rollback Date: %s." <| customDate.ToString("yyyy-MMM-dd") <| withdrawalDate.ToString("yyyy-MMM-dd"))
        | None -> 
            logger.Warn "No Rollback withdrawals excel file to validate"
            
    let private sameDayCustomDepositsValidation (customDate : DateTime) (depositsUserDate : DepositsDateType) : Unit =
        match depositsUserDate with
        | Some depositsDate -> 
            if customDate <> depositsDate then
                failWithLogInvalidArg "[MismatchedFilenameDates]" (sprintf "Custom date mismatch with %s Deposits Date: %s." <| customDate.ToString("yyyy-MMM-dd") <| depositsDate.ToString("yyyy-MMM-dd"))
        | None -> 
            logger.Warn "No Deposits excel file to validate."

    /// Enforce begBalance on 1st month day
    let private begBalance1stDay (customDate : DateTime) (begBalanceDate : DateTime) : Unit =
        if customDate.Month <> begBalanceDate.Month && begBalanceDate.Day <> 1 then
            failWithLogInvalidArg "[BegBalanceDateMismatch]" (sprintf "Custom date %s mismatch or begBalance Report on first of month : %s." <| customDate.ToString("yyyy-MMM-dd") <| begBalanceDate.ToString("yyyy-MMM-dd"))

    /// Enforce endBalance on 1st of next month (if the month changed) or enforce endBalanceDate is downloaded today
    let private endBalanceDateValidation (begBalanceDate : DateTime) (endBalanceDate : DateTime) : Unit =

        if begBalanceDate.Month <> endBalanceDate.Month then

            let nextMonth = begBalanceDate.AddMonths 1

            if nextMonth <> endBalanceDate then
                failWithLogInvalidArg "[EndBalanceDateMismatch]" 
                    (
                        sprintf "End balance date: %s is not 1 month greater than begin balance date: %s !" 
                            <| endBalanceDate.ToString("yyyy-MMM-dd") 
                            <| begBalanceDate.ToString("yyyy-MMM-dd")
                    )

            if endBalanceDate.Day <> 1 then
                failWithLogInvalidArg "[EndBalanceDateMismatch]" 
                    (
                        sprintf "End balance date: %s day is not on day one but %d and months differ with begBalance: %s !" 
                            <| endBalanceDate.ToString("yyyy-MMM-dd")
                            <| endBalanceDate.Day
                            <| begBalanceDate.ToString("yyyy-MMM-dd")
                    )

    /// Check for the existence of required report files for a month by checking matching dates etc
    let areExcelFilenamesValid (rDates : RptDates) : ExcelDatesValid =

        sameDayCustomWithdrawalPendingValidation rDates.customDate rDates.withdrawalsPendingDate
        sameDayCustomWithdrawalRollbackValidation rDates.customDate rDates.withdrawalsRollbackDate
        sameDayCustomDepositsValidation rDates.customDate rDates.DepositsDate

        if rDates.begBalanceDate.IsSome then
            begBalance1stDay rDates.customDate rDates.begBalanceDate.Value

            if rDates.endBalanceDate.IsSome then
                endBalanceDateValidation rDates.begBalanceDate.Value rDates.endBalanceDate.Value
        // if no exception occurs to this point:
        { Valid = true; DayToProcess = rDates.customDate.ToYyyyMmDd } 
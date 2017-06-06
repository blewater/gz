﻿namespace DbImport

module GmRptFiles =
    open System
    open System.IO
    open System.Text.RegularExpressions
    open GzCommon
    open ExcelSchemas
    open FSharp.Data.Runtime.WorldBank
    
    type InRptFolder = { isProd : bool; folderName : string }
    type RptFilenames = { customFilename : string; withdrawalsPendingFilename : string option; withdrawalsRollbackFilename : string option; begBalanceFilename : string; endBalanceFilename : string}
    type RptStrDates = { customDtStr : string; withdrawalsPendingDtStr : string option; withdrawalsRollbackDtStr : string option; begBalanceDtStr : string; endBalanceDtStr : string}
    type RptDates = { customDate : DateTime; withdrawalsPendingDate : DateTime option; withdrawalsRollbackDate : DateTime option; begBalanceDate : DateTime; endBalanceDate : DateTime}
    type ExcelDatesValid = { Valid : bool; DayToProcess : string }

    type DirectoryListIndex =
        | FirstAsc
        | LastAsc

    [<Literal>]
    let CustomFileListPrefix = "Custom "
    [<Literal>]
    let WithdrawalPendingFileListPrefix = "withdrawalsPending "
    [<Literal>]
    let WithdrawalRollbackFileListPrefix = "withdrawalsRollback "
    [<Literal>]
    let BalanceFileListPrefix = "Balance "

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

        let fileMask = if isProd then filenameMask + "Prod*.xlsx" else filenameMask + "Stage*.xlsx"

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
            failWithLogInvalidArg "[EmptyReportFolder]" (sprintf "No excel file within in report folder %s for filename maks %s." inRptFolder.folderName filenameMask)
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
    let private getMinBalanceRptExcelDirList (inRptFolder : InRptFolder) : string = 
        let readDir () = 
            (inRptFolder, BalanceFileListPrefix, FirstAsc)  (** TopAsc for Beginning Balance **)
            |||> getFolderFilenameByMaskIndex

        folderTryF inRptFolder.isProd readDir Missing1stBalanceReport

    /// Get the 2nd balance filename
    let private getNxtBalanceRptExcelDirList (inRptFolder : InRptFolder) : string = 
        let readDir () = 
            (inRptFolder, BalanceFileListPrefix, LastAsc)  (** LastAsc for EndBalance **)
            |||> getFolderFilenameByMaskIndex

        folderTryF inRptFolder.isProd readDir Missing2ndBalanceReport

    /// Return the candidate filename only its title date matches the custom filename title date
    let private getFilteredExcelFilename (candidateExcelFilename : string)(customFilename : string) : string option= 
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
                    (customFilename : string) : string option =

        let readDir () = 
            (inRptFolder, rptFilenameStartingMask, FirstAsc) 
                |||> getOptionalExcelFilenameByIndex
                |>  function
                    | Some filename -> getFilteredExcelFilename filename customFilename
                    | _ -> None
        
        folderTryF inRptFolder.isProd readDir MissingWithdrawalReport
    
 //-------- public Excel Filename functions

    let getCustomFilename (inRptFolder : InRptFolder) : string = 
        getLatestCustomExcelRptFilename inRptFolder

    let getWithdrawalPendingFilename (inRptFolder : InRptFolder)(customFilename : string) : string option= 
        (WithdrawalPendingFileListPrefix , inRptFolder, customFilename) 
            |||> getOptionalExcelRptFilenameInSyncWithCustomDate 

    let getWithdrawalsRollbackFilename (inRptFolder : InRptFolder)(customFilename : string) : string option= 
        (WithdrawalRollbackFileListPrefix, inRptFolder, customFilename) 
            |||> getOptionalExcelRptFilenameInSyncWithCustomDate 

    let getBegBalanceFilename (inRptFolder : InRptFolder) : string = 
        getMinBalanceRptExcelDirList inRptFolder

    let getEndBalanceFilename (inRptFolder : InRptFolder) : string= 
        getNxtBalanceRptExcelDirList inRptFolder
       
    let private getRptFilenames(inRptFolder : InRptFolder) : RptFilenames = 
        let customFilename = 
            inRptFolder 
            |> getCustomFilename 
        { 
            customFilename = customFilename
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

    let getWithdrawalDtStr (withdrawalFilename : string option) : string option = 
        withdrawalFilename 
            |> getOptionalFilenameDateValidation

    let getBegBalanceDtStr (begBalanceFilename : string) : string = 
        begBalanceFilename |> validateDateOnExcelFilename

    let getEndBalanceDtStr (endBalanceFilename : string) : string = 
        endBalanceFilename 
            |> validateDateOnExcelFilename

    let private excelFilenames2DatesStr(excelFiles : RptFilenames) : RptStrDates = 
        { 
            customDtStr = getCustomDtStr excelFiles.customFilename;
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

    let getCustomDate (customDtStr : string) : DateTime = 
        customDtStr.ToDateWithDay

    let getOptionalDtFromFilename (filenameDtStr : string option) =
        match filenameDtStr with
        | Some dateString -> Some dateString.ToDateWithDay
        | None -> None

    let getWithdrawalDate (withdrawalDtStr : string option) : DateTime option = 
        withdrawalDtStr 
            |> getOptionalDtFromFilename

    let getBegBalanceDate (begBalanceDtStr : string) : DateTime = 
        begBalanceDtStr.ToDateWithDay

    let getEndBalanceDate (endBalanceDtStr : string) : DateTime = 
        endBalanceDtStr.ToDateWithDay

    let private excelDatesStr2Dt(excelDatesStr : RptStrDates) : RptDates = 
        { 
            customDate = getBegBalanceDate excelDatesStr.customDtStr; 
            withdrawalsPendingDate = getWithdrawalDate excelDatesStr.withdrawalsPendingDtStr;
            withdrawalsRollbackDate = getWithdrawalDate excelDatesStr.withdrawalsRollbackDtStr;
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

    /// Check if balance report dates content mismatches title 
    let matchBalanceRptDatesWithTitle(excelFilename : string) : bool =
        let balanceRptDate =  
            excelFilename 
                |> getBegBalanceDtStr 
                |> getBegBalanceDate

        let balanceFileExcelSchema = new BalanceExcelSchema(excelFilename)

        let balances = balanceFileExcelSchema.Data |> Seq.skip 1 |> Seq.truncate 2
        let len = balances |> Seq.length 
        if len <= 1 then
            true // header + total line only-> empty balance file
        else
            balances
            |> Seq.tryFindIndex(fun (excelRow) ->
                let excelRowDtStr = DateTime.ParseExact(excelRow.Date, "dd/MM/yyyy", null, Globalization.DateTimeStyles.None)
                excelRowDtStr.AddDays(1.0) = balanceRptDate
            )   
            |> function
                | None -> false // Row date mismatch with title
                | _ -> true // Content dates match with title

    /// Enforce beginning and ending balance Report files title dates match their content
    let balanceRptDateMatchTitles(excelFiles : RptFilenames) : RptFilenames =
        if not (excelFiles.begBalanceFilename 
                    |> matchBalanceRptDatesWithTitle) then 
            failWithLogInvalidArg "[Begin_BalanceRptDatesNotMathingTitle]" (sprintf "The beginning balance excel report filename: %s does not match its contents." excelFiles.begBalanceFilename)

        if not (excelFiles.endBalanceFilename
                    |> matchBalanceRptDatesWithTitle) then 
            failWithLogInvalidArg "[Ending_BalanceRptDatesNotMathingTitle]" (sprintf "The ending balance excel report filename: %s does not match its contents." excelFiles.endBalanceFilename)
        excelFiles

//--------------------- Validation Rules

    

    /// Enforce same day
    let private sameDayValidation (withdrawalType : WithdrawalType)(customDate : DateTime) (withdrawalDate : DateTime option) : Unit =
        let withdrawalTypeStr = WithdrawalTypeString withdrawalType
        match withdrawalDate with
        | Some withdrawalDate -> 
            if customDate <> withdrawalDate then
                failWithLogInvalidArg "[MismatchedFilenameDates]" (sprintf "Custom date %s mismatch with %s Withdrawal Date: %s." <| withdrawalTypeStr <| customDate.ToString("yyyy-MMM-dd") <| withdrawalDate.ToString("yyyy-MMM-dd"))
        | None -> 
            logger.Warn (sprintf "No %s withdrawals excel file" <| withdrawalTypeStr )
            
//        if customDate <> withdrawalPendingDate then
//            failWithLogInvalidArg "[MismatchedFilenameDates]" (sprintf "Custom date %s mismatch with Withdrawal Date: %s." <| customDate.ToString("yyyy-MMM-dd") <| withdrawalPendingDate.ToString("yyyy-MMM-dd"))

    /// Enforce begBalance on 1st month day
    let private begBalance1stDay (customDate : DateTime) (begBalanceDate : DateTime) : Unit =
        if customDate.Month <> begBalanceDate.Month && begBalanceDate.Day <> 1 then
            failWithLogInvalidArg "[BegBalanceDateMismatch]" (sprintf "Custom date %s mismatch or begBalance Report on first of month : %s." <| customDate.ToString("yyyy-MMM-dd") <| begBalanceDate.ToString("yyyy-MMM-dd"))

    /// Enforce endBalance on 1st of next month (if next month is in the past) or enforce endBalanceDate is downloaded today
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

    /// Check for the existence of required report files for a month by checking matching dates etc
    let areExcelFilenamesValid (rDates : RptDates) : ExcelDatesValid =

        sameDayValidation Pending rDates.customDate rDates.withdrawalsPendingDate
        sameDayValidation Rollback rDates.customDate rDates.withdrawalsRollbackDate

        begBalance1stDay rDates.customDate rDates.begBalanceDate

        endBalanceDateValidation rDates.begBalanceDate rDates.endBalanceDate
        // if no exception occurs to this point:
        { Valid = true; DayToProcess = rDates.customDate.ToYyyyMmDd } 
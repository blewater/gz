namespace DbImport

module GmRptFiles =
    open System
    open System.IO
    open System.Text.RegularExpressions
    open GzCommon
    open ExcelSchemas
    open FSharp.Data.Runtime.WorldBank

    type FolderNameType = string

    // Filename types
    type CustomFilenameType = string
    type Vendor2UserFilenameType = string option
    type WithdrawalsPendingFilenameType = string option
    type WithdrawalsRollingFilenameType = string option
    type BegBalanceFilenameType = string option
    type EndBalanceFilenameType = string option

    // Filename string dates
    type CustomStrDateType = string
    type Vendor2UserStrDateType = string option
    type WithdrawalsPendingStrDateType = string option
    type WithdrawalsRollbackStrDateType = string option
    type BegBalanceStrDateType = string option
    type EndBalanceStrDateType = string option

    // Filename dates
    type CustomDateType = DateTime
    type Vendor2UserDateType = DateTime option
    type WithdrawalsPendingDateType = DateTime option
    type WithdrawalsRollbackDateType = DateTime option
    type BegBalanceDateType = DateTime option
    type EndBalanceDateType = DateTime option

    type InRptFolder = { isProd : bool; folderName : FolderNameType }
    type RptFilenames = { customFilename : CustomFilenameType; Vendor2UserFilename : Vendor2UserFilenameType; withdrawalsPendingFilename : WithdrawalsPendingFilenameType; withdrawalsRollbackFilename : WithdrawalsRollingFilenameType; begBalanceFilename : BegBalanceFilenameType; endBalanceFilename : EndBalanceFilenameType}
    type RptStrDates = { customDtStr : CustomStrDateType; Vendor2UserDtStr : Vendor2UserStrDateType; withdrawalsPendingDtStr : WithdrawalsPendingStrDateType; withdrawalsRollbackDtStr : WithdrawalsRollbackStrDateType; begBalanceDtStr : BegBalanceStrDateType; endBalanceDtStr : EndBalanceStrDateType}
    type RptDates = { customDate : CustomDateType; Vendor2UserDate : Vendor2UserDateType; withdrawalsPendingDate : WithdrawalsPendingDateType; withdrawalsRollbackDate : WithdrawalsRollbackDateType; begBalanceDate : BegBalanceDateType; endBalanceDate : EndBalanceDateType}

    type ExcelDatesValid = { Valid : bool; DayToProcess : string }

    type DirectoryListIndex =
        | FirstAsc
        | LastAsc

    [<Literal>]
    let CustomFileListPrefix = "Custom"
    [<Literal>]
    let Vendor2UserFileListPrefix = "Vendor2User"
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

    let getVendor2UserFilename (inRptFolder : InRptFolder)(customFilename : CustomFilenameType) : Vendor2UserFilenameType = 
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
            Vendor2UserFilename = getVendor2UserFilename inRptFolder customFilename
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
            Vendor2UserDtStr = getVendor2UserDtStr excelFiles.Vendor2UserFilename; 
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

    let getVendor2UserlDate (vendor2UserDtStr : Vendor2UserStrDateType) : Vendor2UserDateType = 
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
            Vendor2UserDate = getVendor2UserlDate excelDatesStr.Vendor2UserDtStr;
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

    /// Check if vendor2User excel report is really that type of excel report
    let private vendor2UserRptContentMatchCheck(excelFilename : Vendor2UserFilenameType) : bool =

        match excelFilename with
        | Some vendor2UserFilename ->
            let vendor2UserExcel = new Vendor2UserExcelSchema(vendor2UserFilename)

            // Keep the first data row
            let vendor2UserAmounts = vendor2UserExcel.Data |> Seq.truncate 2
            let len = vendor2UserAmounts |> Seq.length 
            if len <= 1 then
                true // header + total line only-> empty balance file
            else
                vendor2UserAmounts
                |> Seq.tryFindIndex(fun (excelRow) ->
                    let vendor2UserExcelTransType = excelRow.``Trans type``
                    vendor2UserExcelTransType = "Vendor2User"
                )   
                |> function
                    | None -> false // this is not a Vendor2User excel file
                    | _ -> true // Ok it's Vendor2User file
        | None ->
            true // return ok

    /// Enforce beginning and ending balance Report files title dates match their content
    let vendor2UserRptContentMatch(excelFiles : RptFilenames) : RptFilenames =
        if not (excelFiles.Vendor2UserFilename 
                    |> vendor2UserRptContentMatchCheck) then 
            failWithLogInvalidArg "[Vendor2UserFileNotMatchingContent]" (sprintf "The vendor2User excel report filename: %s does not match its contents." excelFiles.Vendor2UserFilename.Value)
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
            
    let private sameDayCustomVendor2UserValidation (customDate : DateTime) (vendor2UserDate : Vendor2UserDateType) : Unit =
        match vendor2UserDate with
        | Some vendor2UserDate -> 
            if customDate <> vendor2UserDate then
                failWithLogInvalidArg "[MismatchedFilenameDates]" (sprintf "Custom date mismatch with %s Vendor2User Date: %s." <| customDate.ToString("yyyy-MMM-dd") <| vendor2UserDate.ToString("yyyy-MMM-dd"))
        | None -> 
            logger.Warn "No Vendor2User excel file to validate."

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
        sameDayCustomVendor2UserValidation rDates.customDate rDates.Vendor2UserDate

        if rDates.begBalanceDate.IsSome then
            begBalance1stDay rDates.customDate rDates.begBalanceDate.Value

            if rDates.endBalanceDate.IsSome then
                endBalanceDateValidation rDates.begBalanceDate.Value rDates.endBalanceDate.Value
        // if no exception occurs to this point:
        { Valid = true; DayToProcess = rDates.customDate.ToYyyyMmDd } 
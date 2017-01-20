namespace DbImport

module GmRptFiles =
    open System
    open System.IO
    open ErrorHandling
    open System.Text.RegularExpressions
    
    type InRptFolder = { isProd : bool; folderName : string }
    type RptFilenames = { customFilename : string; withdrawalFilename : string; begBalanceFilename : string; endBalanceFilename : string }
    type RptStrDates = { customDtStr : string; withdrawDtStr : string; begBalanceDtStr : string; endBalanceDtStr : string }
    type RptDates = { customDate : DateTime; withdrawDate : DateTime; begBalanceDate : DateTime; endBalanceDate : DateTime }

    let private folderTryF (isProd : bool) f domainException =
        let logInfo = "isProd", isProd
        tryF f domainException (Some logInfo)

    let private getEarliestExcelFile (inRptFolder : InRptFolder) (filenameMask : string Option) (topListIndex : int): string =
        // Deconstruct
        let { isProd = isProd; folderName = folderName} = inRptFolder
        let fileMask = 
            match filenameMask with
            | Some maskToAdd -> if isProd then maskToAdd + "Prod*.xlsx" else maskToAdd + "Stage*.xlsx"
            | None -> if isProd then "Prod*.xlsx" else "Stage*.xlsx"
        Directory.GetFiles(folderName, fileMask) 
            |> Array.toList 
            |> List.sort
            |> List.item topListIndex
    
    /// Get the custom excel rpt filename
    let private getMinCustomExcelRptFilename (inRptFolder : InRptFolder) : string =
        let readDir () = getEarliestExcelFile inRptFolder (Some "Custom ") 0
        folderTryF inRptFolder.isProd readDir MissingCustomReport
    
    /// Get first balance filename
    let private getMinBalanceRptExcelDirList (inRptFolder : InRptFolder) : string = 
        let readDir () = getEarliestExcelFile inRptFolder (Some "Balance ") 0
        folderTryF inRptFolder.isProd readDir Missing1stBalanceReport

    /// Get the 2nd balance filename
    let private getNxtBalanceRptExcelDirList (inRptFolder : InRptFolder) : string = 
        let readDir () = getEarliestExcelFile inRptFolder (Some "Balance ") 1
        folderTryF inRptFolder.isProd readDir Missing2ndBalanceReport

    /// Get first withdrawal filename
    let private getMinWithdrawalExcelRptFilename (inRptFolder : InRptFolder) : string =
        let readDir () = getEarliestExcelFile inRptFolder (Some "pendingwithdraw ") 0
        folderTryF inRptFolder.isProd readDir MissingWithdrawalReport

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

 //-------- public Excel Filename functions

    let getCustomFilename (inRptFolder : InRptFolder) : string = 
        getMinCustomExcelRptFilename inRptFolder

    let getWithdrawalFilename (inRptFolder : InRptFolder) : string = 
        getMinWithdrawalExcelRptFilename inRptFolder

    let getBegBalanceFilename (inRptFolder : InRptFolder) : string = 
        getMinBalanceRptExcelDirList inRptFolder

    let getEndBalanceFilename (inRptFolder : InRptFolder) : string = 
        getNxtBalanceRptExcelDirList inRptFolder
       
    let private getRptFilenames(inRptFolder : InRptFolder) : RptFilenames = 
        { 
            customFilename = getCustomFilename inRptFolder
            withdrawalFilename = getWithdrawalFilename inRptFolder
            begBalanceFilename = getBegBalanceFilename inRptFolder
            endBalanceFilename = getEndBalanceFilename inRptFolder
        }

    let getExcelFilenames (inRptFolder : InRptFolder) : RptFilenames =
        let excelFiles = getRptFilenames inRptFolder
        let datesLogMsg = sprintf "Excel filenames: %A" excelFiles
        logger.Info datesLogMsg
        excelFiles

 //-------- Excel Date String functions

    let getCustomDtStr (customFilename : string) : string = 
        customFilename |> validateDateOnExcelFilename

    let getWithdrawalDtStr (withdrawalFilename : string) : string = 
        withdrawalFilename |> validateDateOnExcelFilename

    let getBegBalanceDtStr (begBalanceFilename : string) : string = 
        begBalanceFilename |> validateDateOnExcelFilename

    let getEndBalanceDtStr (endBalanceFilename : string) : string = 
        endBalanceFilename |> validateDateOnExcelFilename

    let private excelFilenames2DatesStr(excelFiles : RptFilenames) : RptStrDates = 
        { 
            customDtStr = getCustomDtStr excelFiles.customFilename;
            withdrawDtStr = getWithdrawalDtStr excelFiles.withdrawalFilename; 
            begBalanceDtStr = getBegBalanceDtStr excelFiles.begBalanceFilename;
            endBalanceDtStr = getEndBalanceDtStr excelFiles.endBalanceFilename;
        }

    let getExcelDtStr (excelFiles : RptFilenames) : RptStrDates =
        let excelFileStrDates = excelFilenames2DatesStr excelFiles
        let datesLogMsg = sprintf "Parsed excel filename title string dates: %A" excelFileStrDates
        logger.Info datesLogMsg
        excelFileStrDates

 //-------- Excel DateTime functions

    let getCustomDate (customDtStr : string) : DateTime = 
        customDtStr.ToDateWithDay

    let getWithdrawalDate (withdrawalDtStr : string) : DateTime = 
        withdrawalDtStr.ToDateWithDay

    let getBegBalanceDate (begBalanceDtStr : string) : DateTime = 
        begBalanceDtStr.ToDateWithDay

    let getEndBalanceDate (endBalanceDtStr : string) : DateTime = 
        endBalanceDtStr.ToDateWithDay

    let private excelDatesStr2Dt(excelDatesStr : RptStrDates) : RptDates = 
        { 
            customDate = getBegBalanceDate excelDatesStr.customDtStr; 
            withdrawDate = getWithdrawalDate excelDatesStr.withdrawDtStr; 
            begBalanceDate = getBegBalanceDate excelDatesStr.begBalanceDtStr; 
            endBalanceDate = getEndBalanceDate excelDatesStr.endBalanceDtStr
        }

    let getExcelDates (excelDatesStr : RptStrDates) : RptDates =
        let excelFileDates = excelDatesStr2Dt excelDatesStr
        let datesLogMsg = sprintf "Parsed excel filename title dates in DateTime: %A" excelFileDates
        logger.Info datesLogMsg
        excelFileDates

//--------------------- Validation Rules

    /// Enforce same day
    let private sameDayValidation (customDate : DateTime) (withdrawDate : DateTime) : Unit =
        if customDate <> withdrawDate then
            failWithLogInvalidArg "[MismatchedFilenameDates]" (sprintf "Custom date %s mismatch with minWithdrawalDate: %s." <| customDate.ToString("yyyy-mm-dd") <| withdrawDate.ToString("yyyy-mm-dd"))

    /// Enforce begBalance on 1st month day
    let private begBalance1stDay (customDate : DateTime) (begBalanceDate : DateTime) : Unit =
        if customDate.Month <> begBalanceDate.Month && begBalanceDate.Day <> 1 then
            failWithLogInvalidArg "[BegBalanceDateMismatch]" (sprintf "Custom date %s mismatch or begBalance Report on first of month : %s." <| customDate.ToString("yyyy-MMM-dd") <| begBalanceDate.ToString("yyyy-MMM-dd"))

    /// Enforce begBalance on 1st month day
    let private balanceDatesValidation (begBalanceDate : DateTime) (endBalanceDate : DateTime) : Unit =
        if begBalanceDate.AddMonths 1 <> endBalanceDate then
            failWithLogInvalidArg "[EndBalanceDateMismatch]" (sprintf "End balance date: %s is not 1 month greater than begin balance date: %s !" <| endBalanceDate.ToString("yyyy-MMM-dd") <| begBalanceDate.ToString("yyyy-MMM-dd"))

    let areExcelFilenamesValid (rptDates : RptDates) =
        let { customDate = customDate ; begBalanceDate = begBalanceDate ; endBalanceDate = endBalanceDate ; withdrawDate = withdrawDate } = rptDates

        sameDayValidation customDate withdrawDate

        begBalance1stDay customDate begBalanceDate
        
        balanceDatesValidation begBalanceDate endBalanceDate
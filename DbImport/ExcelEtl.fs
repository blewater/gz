namespace DbImport

module BalanceRpt2Db =
    open NLog
    open GzDb.DbUtil
    open ExcelSchemas
    open GzCommon

    let logger = LogManager.GetCurrentClassLogger()

    /// Process all excel lines except Totals and upsert them
    let importBalanceExcelRptRows 
                (balanceType : BalanceType)
                (db : DbContext) 
                (balanceExcelFile : BalanceExcelSchema) 
                (yyyyMmDd : string) =

        // Loop through all excel rows
        for excelRow in balanceExcelFile.Data do
            // Skip totals line & test emails
            if excelRow.``User ID`` > 0.0 && excelRow.Email |> allowedPlayerEmail then
                logger.Info(
                    sprintf "Processing balance email %s on %s/%s/%s" 
                        excelRow.Email <| yyyyMmDd.Substring(6, 2) <| yyyyMmDd.Substring(4, 2) <| yyyyMmDd.Substring(0, 4))

                DbPlayerRevRpt.updDbBalances balanceType db yyyyMmDd excelRow
    
    /// Open an excel file and return its memory schema
    let private openBalanceRptSchemaFile (balanceType : BalanceType) (excelFilename : string) : BalanceExcelSchema =
        let balanceFileExcelSchema = new BalanceExcelSchema(excelFilename)
        logger.Info ""
        logger.Info (sprintf "************ Processing %A Report from filename: %s " balanceType excelFilename)
        logger.Info ""
        balanceFileExcelSchema
    
    /// Process the balance excel files: Extract each row and update database customer balance amounts
    let loadBalanceRpt 
                (balanceType : BalanceType)
                (db : DbContext) 
                (balanceRptFullPath: string) 
                (customYyyyMmDd : string ) =

        // TODO: Check that the passed in Balance filename matches the type

        //Curry with balance Type
        let balanceTypedOpenSchema = openBalanceRptSchemaFile balanceType
        // Open excel report file for the memory schema
        let openFile = balanceRptFullPath |> balanceTypedOpenSchema
        importBalanceExcelRptRows balanceType db openFile customYyyyMmDd

module DepositsRpt2Db =
    open NLog
    open System
    open NLog
    open GzCommon
    open GzDb.DbUtil
    open ExcelSchemas
    open GmRptFiles
    open ExcelUtil

    let logger = LogManager.GetCurrentClassLogger()

    /// Deposits performed in current processing month
    let private isInitiatedInCurrentMonth(initiatedDate : DateTime)(currentYm : DateTime) : bool =
        currentYm.Month = initiatedDate.Month && currentYm.Year = initiatedDate.Year

    let private completedEqThisDateMonth (completedDt : DateTime Nullable)(thisDate : DateTime) : bool =
        if not completedDt.HasValue then
            false
        else 
            thisDate.Month = completedDt.Value.Month && thisDate.Year = completedDt.Value.Year

    /// Deposits completion performed within current processing month
    let private isCompletedInCurrentMonth (completedDt : DateTime Nullable)(currentYm : DateTime) : bool =
        completedEqThisDateMonth completedDt currentYm

    /// Deposits initiated & completed in the reported month
    let private isCompletedInSameMonth (completedDt : DateTime Nullable)(initiatedDate : DateTime) : bool =
        completedEqThisDateMonth completedDt initiatedDate

    let private debit2DepositsType (excelDebitDesc : string) : DepositsAmountType =
        if excelDebitDesc.Contains "BonusGranted" then
            V2UCashBonus

        elif excelDebitDesc.Contains "Main (System)" then
            V2UDeposit
        elif excelDebitDesc.Contains "Ordinary" then
            Deposit
        else
            failwithf "Unknown Deposits debit description: %s." excelDebitDesc

    /// Process all excel lines except Totals and upsert them
    let private updDbDepositsExcelRptRows 
                (db : DbContext) 
                (vendor2UserExcelFile : DepositsExcelSchema) 
                (yyyyMmDd : string) : Unit =

        let currentYearMonth = yyyyMmDd.ToDateWithDay
        // Loop through all excel rows
        for excelRow in vendor2UserExcelFile.Data do
            // Skip totals line
            let userId = excelRow.UserID |> getNonNullableUserId
            if userId > 0 then
                // Assume we always has an initiated date value
                let initiatedDt = (excelRow.Initiated |> excelObj2NullableDt WithdrawalRpt).Value
                let initiatedCurrently = isInitiatedInCurrentMonth initiatedDt currentYearMonth

                let completedDt = excelRow.Completed |> excelObj2NullableDt WithdrawalRpt
                let completedCurrently = isCompletedInCurrentMonth completedDt currentYearMonth 
                let completedInSameMonth = isCompletedInSameMonth completedDt initiatedDt
                
                let depositsAmountType = debit2DepositsType excelRow.Debit

                // initiatedCurrently && CompletedInSameMonth are within TotalWithdrawals in Custom
                if initiatedCurrently && completedInSameMonth && completedCurrently then
                    DbPlayerRevRpt.updDbDepositsPlayerRow depositsAmountType db yyyyMmDd excelRow

                else
                    logger.Warn(sprintf "\nVendor2User row not imported! Initiated: %O, CurrentDt: %s, CompletedDt: %A, initiatedCurrently: %b, completedCurrently: %b, completedInSameMonth: %b" 
                        initiatedDt yyyyMmDd completedDt initiatedCurrently completedCurrently completedInSameMonth)
    
    /// Open an Vendor2User excel file and console out the filename
    let private openDepositsRptSchemaFile excelFilename = 
        let DepositsExcelSchemaFile = DepositsExcelSchema(excelFilename)
        logger.Info ""
        logger.Info (sprintf "************ Processing Deposits Report %s excel file" excelFilename)
        logger.Info ""
        DepositsExcelSchemaFile
    
    /// Process the deposits (normal, vendor2user, cashbonus) excel file: Extract each row and upsert database deposits amounts
    let updDbDepositsRpt (db : DbContext)(depositsRptFullPath: string) =

        // Open excel report file for the memory schema
        let openFile = 
            depositsRptFullPath
            |> openDepositsRptSchemaFile

        let depositsRptDtStr = 
            Some depositsRptFullPath 
                |> getVendor2UserDtStr

        if depositsRptDtStr.IsSome then
            updDbDepositsExcelRptRows db openFile depositsRptDtStr.Value

open DepositsRpt2Db
            
module CustomRpt2Db =
    open NLog
    open GzDb.DbUtil
    open ExcelSchemas
    open GmRptFiles
    open GzCommon

    let logger = LogManager.GetCurrentClassLogger()

    /// Process all excel lines except Totals and upsert them
    let private setDbCustomExcelRptRows 
                (db : DbContext) (customExcelSchemaFile : CustomExcelSchema) 
                (yyyyMmDd : string) : unit =

        // Loop through all excel rows
        for excelRow in customExcelSchemaFile.Data do
            let isActive = excelRow.``Player status`` = "Active"
            let emailAddress = excelRow.``Email address``
            let okEmail =
                match emailAddress with
                | null -> false
                | email -> email |> allowedPlayerEmail
            if 
                isActive && okEmail then

                logger.Info(
                    sprintf "Processing email %s on %s/%s/%s" 
                        excelRow.``Email address`` <| yyyyMmDd.Substring(6, 2) <| yyyyMmDd.Substring(4, 2) <| yyyyMmDd.Substring(0, 4))

                DbPlayerRevRpt.setDbCustomPlayerRow db yyyyMmDd excelRow
    
    /// Open an excel file and console out the filename
    let private openCustomRptSchemaFile excelFilename = 
        let customExcelSchemaFile = new CustomExcelSchema(excelFilename)
        logger.Info ""
        logger.Info (sprintf "************ Processing Custom Report %s excel file" excelFilename)
        logger.Info ""
        customExcelSchemaFile
    
    /// Process the custom excel file: Extract each row and upsert the database PlayerRevRpt table customer rows.
    let loadCustomRpt 
            (db : DbContext) 
            (customRptFullPath: string) : unit =

        // Open excel report file for the memory schema
        let openFile = customRptFullPath |> openCustomRptSchemaFile
        let yyyyMmDd = customRptFullPath |> getCustomDtStr
        try 
            setDbCustomExcelRptRows db openFile yyyyMmDd
        with _ -> 
            reraise()

module Etl =
    open System 
    open System.IO
    open GzDb.DbUtil
    open NLog
    open GmRptFiles
    open ExcelSchemas
    open CustomRpt2Db
    open BalanceRpt2Db
    open WithdrawalRpt2Db
    open DbPlayerRevRpt


    let logger = LogManager.GetCurrentClassLogger()

    let private moveFileWithOverwrite(inFolder : string)(outFolder : string)(fullPathfilename : string) : unit =
        let destFilename = fullPathfilename.Replace(inFolder, outFolder)
        if File.Exists(destFilename) then
            File.Delete(destFilename)
        File.Move(fullPathfilename, destFilename)

    /// Move processed files to out folder except endBalanceFile which is the next month's begBalanceFile
    let private moveRptsToOutFolder 
            (inFolder : string) (outFolder :string)
            (rpts : RptFilenames) : unit =

        let fileDates = 
            rpts 
            |> GmRptFiles.getExcelDtStr 
            |> GmRptFiles.getExcelDates
        
        // Custom
        (inFolder, outFolder, rpts.customFilename) |||> moveFileWithOverwrite
        
        // Move beginning balance report if end balance report is present (end of month clearance)
        let midnightUtc = DateTime.UtcNow.Date
        let processingBalancesWithinMonth = 
            match fileDates.begBalanceDate with
            | Some balanceDt -> midnightUtc.Month = balanceDt.Month && midnightUtc.Year = balanceDt.Year
            | None -> false

        if rpts.begBalanceFilename.IsSome then
            if processingBalancesWithinMonth && rpts.endBalanceFilename.IsSome then
                (inFolder, outFolder, rpts.endBalanceFilename.Value)
                    |||> moveFileWithOverwrite
            elif not processingBalancesWithinMonth then
                (inFolder, outFolder, rpts.begBalanceFilename.Value)
                    |||> moveFileWithOverwrite
        
        // Vendor2User
        match rpts.DepositsFilename with
        | Some excelFilename -> 
            (inFolder, outFolder, excelFilename) |||> moveFileWithOverwrite
        | None -> logger.Warn "No Vendor2User file to move."
        
        // Withdrawals Pending
        match rpts.withdrawalsPendingFilename with
        | Some withdrawalsFilename -> 
            (inFolder, outFolder, withdrawalsFilename) |||> moveFileWithOverwrite
        | None -> logger.Warn "No Pending withdrawal file to move."
        
        // Withdrawals Rollback
        match rpts.withdrawalsRollbackFilename with
        | Some withdrawalsFilename -> 
            (inFolder, outFolder, withdrawalsFilename) |||> moveFileWithOverwrite
        | None -> logger.Warn "No Rollback withdrawal file to move."

    let private setDbimportExcel (db : DbContext)(reportFilenames : RptFilenames) : unit =
        let { 
                customFilename = customFilename; 
                DepositsFilename = depositsFilename;
                withdrawalsPendingFilename = withdrawalsPendingFilename;
                withdrawalsRollbackFilename = withdrawalsRollbackFilename;
                begBalanceFilename = begBalanceFilename; 
                endBalanceFilename = endBalanceFilename 
            }   = reportFilenames

        let customDtStr = getCustomDtStr customFilename
        logger.Debug (sprintf "Starting processing excel report files for %O" customDtStr)

        // Custom import
        (db, customFilename) 
            ||> loadCustomRpt 
        
        // Pending withdrawals import
        match depositsFilename with
        | Some depositsFilename -> 
                (db, depositsFilename) 
                    ||> updDbDepositsRpt
        | None -> logger.Warn "No Deposits file to import."
        
        // Beg, end balance import
        if begBalanceFilename.IsSome then
            loadBalanceRpt BeginingBalance db begBalanceFilename.Value customDtStr

        // In present month there's no end balance file
        if endBalanceFilename.IsSome then
            loadBalanceRpt EndingBalance db endBalanceFilename.Value customDtStr
        
        // Pending withdrawals import
        match withdrawalsPendingFilename with
        | Some withdrawalFilename -> 
                (db, withdrawalFilename, Pending) 
                    |||> updDbWithdrawalsRpt
        | None -> logger.Warn "No pending withdrawal file to import."
        
        // Rollback withdrawals import
        match withdrawalsRollbackFilename with
        | Some withdrawalFilename -> 
                (db, withdrawalFilename, Rollback) 
                    |||> updDbWithdrawalsRpt
        | None -> logger.Warn "No rollback withdrawal file to import."
        
        (db, customDtStr) 
            ||> DbPlayerRevRpt.setDbMonthyGainLossAmounts 

        // Finally upsert GzTrx with the balance, credit amounts
        (db, customDtStr) 
            ||> DbGzTrx.setDbPlayerRevRpt2GzTrx

    /// <summary>
    ///
    ///     Read all excel files from input folder
    ///     Save files into the database
    ///
    /// </summary>
    /// <param name="db">The database context</param>
    /// <param name="inFolder">The input file folder value</param>
    /// <param name="outFolder">The processed file folder value</param>
    /// <returns>Unit</returns>
    let ProcessExcelFolder
            (isProd : bool) 
            (db : DbContext) 
            (inFolder : string)
            (outFolder : string) =
        
        //------------ Read filenames
        logger.Info (sprintf "Reading the %s folder" inFolder)
        
        let reportfileNames = 
            { isProd = isProd ; folderName = inFolder } 
            |> getExcelFilenames

        /// Try 3 times to upload reports
        let dbOperation() = 
            (db, reportfileNames) 
                ||> setDbimportExcel
        (db, dbOperation) 
            ||> tryDBCommit3Times

        // Archive / move excel files
        moveRptsToOutFolder 
            inFolder 
            outFolder 
            reportfileNames

        // Update null gmCustomerids
        setDbGmCustomerId db


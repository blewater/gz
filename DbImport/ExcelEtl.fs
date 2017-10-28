namespace DbImport

module BalanceRpt2Db =
    open NLog
    open GzDb.DbUtil
    open ExcelSchemas
    open GzBatchCommon

    let logger = LogManager.GetCurrentClassLogger()


    let importBalanceExcelRptRow 
                (balanceType : BalanceType)
                (db : DbContext) 
                (excelRow : BalanceExcelSchema.Row) 
                (yyyyMmDd : string) =

        logger.Info(
            sprintf "Processing balance email %s on %s/%s/%s" 
                excelRow.Email <| yyyyMmDd.Substring(6, 2) <| yyyyMmDd.Substring(4, 2) <| yyyyMmDd.Substring(0, 4))

        DbPlayerRevRpt.updDbBalances balanceType db yyyyMmDd excelRow

    /// Process all excel lines except Totals and upsert them
    let importBalanceExcelRptRows 
                (balanceType : BalanceType)
                (db : DbContext) 
                (balanceExcelFile : BalanceExcelSchema) 
                (yyyyMmDd : string) 
                (emailToProcAlone : string option) =

        // Loop through all excel rows
        for excelRow in balanceExcelFile.Data do
            // Skip totals line & test emails
            if excelRow.``User ID`` > 0.0 && excelRow.Email |> allowedPlayerEmail then
                
                // Normal all user processing case
                if emailToProcAlone.IsNone then
                    importBalanceExcelRptRow balanceType db excelRow yyyyMmDd
                
                elif emailToProcAlone.IsSome && emailToProcAlone.Value = excelRow.Email then
                    importBalanceExcelRptRow balanceType db excelRow yyyyMmDd
                
                // Skipping emailToProcAlone.IsSome && emailToProcAlone.Value <> excelRow.Email

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
                (customYyyyMmDd : string ) 
                (emailToProcAlone : string option)=

        // TODO: Check that the passed in Balance filename matches the type

        //Curry with balance Type
        let balanceTypedOpenSchema = openBalanceRptSchemaFile balanceType
        // Open excel report file for the memory schema
        let openFile = balanceRptFullPath |> balanceTypedOpenSchema
        importBalanceExcelRptRows balanceType db openFile customYyyyMmDd emailToProcAlone

module DepositsRpt2Db =
    open NLog
    open System
    open GzBatchCommon
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

    /// Separate between Deposits, V2U and BonusGranted
    let private debit2DepositsType (excelDebitDesc : string) : DepositsAmountType =

        // Free spins... so far appear as 0 amounts
        if excelDebitDesc.Contains "BonusGranted" then
            V2UDeposit

        // Gz deposited cash to user
        elif excelDebitDesc.Contains "Main (System) Bonus" || excelDebitDesc = "CasinoWallet (CasinoWallet) Bonus" then
            V2UCashBonus

        // Normal user cash deposi
        elif excelDebitDesc.Contains "Ordinary" then
            Deposit
        else
            failwithf "Unknown Deposits debit description: %s." excelDebitDesc

    /// Process (upload to database) a single excel deposit
    let private updDbDepositsExcelRptRow
                (db : DbContext) 
                (excelRow : DepositsExcelSchema.Row) 
                (yyyyMmDd : string) =

        if excelRow.``Credit real  amount`` <> 0.0 || excelRow.``Debit real amount`` <> 0.0 then

            let currentYearMonth = yyyyMmDd.ToDateWithDay
            // Assume we always has an initiated date value
            let initiatedDt = (excelRow.Initiated |> excelObj2NullableDt WithdrawalRpt).Value
            let initiatedCurrently = isInitiatedInCurrentMonth initiatedDt currentYearMonth

            let completedDt = excelRow.Completed |> excelObj2NullableDt WithdrawalRpt
            let completedYyyyMmDd = if completedDt.HasValue then completedDt.Value.ToYyyyMmDd else "Null"
            let completedCurrently = isCompletedInCurrentMonth completedDt currentYearMonth 
            let completedInSameMonth = isCompletedInSameMonth completedDt initiatedDt
                
            let depositsAmountType = debit2DepositsType excelRow.Debit

            let dateLogMsg = sprintf "Deposit for email %s initiatedDt: %s, completedDt: %s, completedCurrenntly: %b, completedInSameMonth: %b" excelRow.Email (initiatedDt.ToYyyyMmDd) completedYyyyMmDd completedCurrently completedInSameMonth
            logger.Debug dateLogMsg

            // less restrictive than withdrawals... completed in current processing month?
            if completedCurrently then
                DbPlayerRevRpt.updDbDepositsPlayerRow depositsAmountType db yyyyMmDd excelRow

            else
                logger.Warn(sprintf "\nVendor2User row not imported! Initiated: %O, CurrentDt: %s, CompletedDt: %s, initiatedCurrently: %b, completedCurrently: %b, completedInSameMonth: %b" 
                    initiatedDt yyyyMmDd completedYyyyMmDd initiatedCurrently completedCurrently completedInSameMonth)

    /// Process all excel lines except Totals and upsert them
    let private updDbDepositsExcelRptRows 
                (db : DbContext) 
                (vendor2UserExcelFile : DepositsExcelSchema) 
                (yyyyMmDd : string) 
                (emailToProcAlone: string option) : Unit =

        // Loop through all excel rows
        for excelRow in vendor2UserExcelFile.Data do
            // Skip totals line
            let userId = excelRow.UserID |> getNonNullableUserId
            if userId > 0 then
                let excelUserEmail = excelRow.Email
                
                if emailToProcAlone.IsNone then
                    updDbDepositsExcelRptRow db excelRow yyyyMmDd
                
                elif emailToProcAlone.IsSome && emailToProcAlone.Value = excelUserEmail then 
                    updDbDepositsExcelRptRow db excelRow yyyyMmDd
    
    /// Open an Vendor2User excel file and console out the filename
    let private openDepositsRptSchemaFile excelFilename = 
        let depositsExcelSchemaFile = DepositsExcelSchema(excelFilename)
        logger.Info ""
        logger.Info (sprintf "************ Processing Deposits Report %s excel file" excelFilename)
        logger.Info ""
        depositsExcelSchemaFile
    
    /// Process the deposits (normal, vendor2user, cashbonus) excel file: Extract each row and upsert database deposits amounts
    let updDbDepositsRpt 
        (db : DbContext)
        (depositsRptFullPath: string) 
        (emailToProcAlone: string option) =

        // Open excel report file for the memory schema
        let openFile = 
            depositsRptFullPath
            |> openDepositsRptSchemaFile

        let depositsRptDtStr = 
            Some depositsRptFullPath 
                |> getVendor2UserDtStr

        if depositsRptDtStr.IsSome then
            updDbDepositsExcelRptRows db openFile depositsRptDtStr.Value emailToProcAlone

open DepositsRpt2Db
            
module CustomRpt2Db =
    open NLog
    open GzDb.DbUtil
    open ExcelSchemas
    open GmRptFiles
    open GzBatchCommon

    let logger = LogManager.GetCurrentClassLogger()


    /// Process a single custom user row
    let procCustomUser (db : DbContext) (yyyyMmDd : string) (excelRow : CustomExcelSchema.Row) =
        logger.Info(
            sprintf "Processing email %s on %s/%s/%s" 
                excelRow.``Email address`` <| yyyyMmDd.Substring(6, 2) <| yyyyMmDd.Substring(4, 2) <| yyyyMmDd.Substring(0, 4))

        DbPlayerRevRpt.setDbCustomPlayerRow db yyyyMmDd excelRow

    /// Process all excel lines except Totals and upsert them
    let private setDbCustomExcelRptRows 
                (db : DbContext) (customExcelSchemaFile : CustomExcelSchema) 
                (yyyyMmDd : string) 
                (emailToProcAlone: string option) : unit =

        // Loop through all excel rows
        for excelRow in customExcelSchemaFile.Data do
            let isActive = 
                match excelRow.``Player status`` with 
                | "Active" -> true
                | "Blocked" -> true // They may be unblocked in the future
                | _ -> false
            let excelEmailAddress = excelRow.``Email address``
            let okEmail =
                match excelEmailAddress with
                | null -> false
                | email -> email |> allowedPlayerEmail
            if 
                isActive && okEmail then

                // Normal all users processing mode
                if emailToProcAlone.IsNone then
                    procCustomUser db yyyyMmDd excelRow
    
                // single user processing
                elif emailToProcAlone.IsSome && emailToProcAlone.Value = excelEmailAddress then
                    procCustomUser db yyyyMmDd excelRow
                
                // Skipping emailToProcAlone.IsSome && emailToProcAlone.Value <> excelRow.Email

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
            (customRptFullPath: string) 
            (emailToProcOnly: string option) : unit =

        // Open excel report file for the memory schema
        let openFile = customRptFullPath |> openCustomRptSchemaFile
        let yyyyMmDd = customRptFullPath |> getCustomDtStr
        try 
            setDbCustomExcelRptRows db openFile yyyyMmDd emailToProcOnly
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

    let private setDbimportExcel (db : DbContext)(reportFilenames : RptFilenames)(emailToProcAlone : string option) : unit =
        let { 
                customFilename = customFilename; 
                DepositsFilename = depositsFilename;
                withdrawalsPendingFilename = withdrawalsPendingFilename;
                withdrawalsRollbackFilename = withdrawalsRollbackFilename;
                begBalanceFilename = begBalanceFilename; 
                endBalanceFilename = endBalanceFilename 
            }   = reportFilenames

        let customDtStr = getCustomDtStr customFilename
        match emailToProcAlone with
        | None -> logger.Debug (sprintf "Starting processing excel report files for %s" customDtStr)
        | Some singleUserEmail -> 
            logger.Warn("********************************************************")
            logger.Warn(sprintf "Processing excel files in single user mode for %s." singleUserEmail)
            logger.Warn("********************************************************")

        // Custom import
        (db, customFilename, emailToProcAlone) 
            |||> loadCustomRpt 
        
        // Beg, end balance import
        if begBalanceFilename.IsSome then
            loadBalanceRpt BeginingBalance db begBalanceFilename.Value customDtStr emailToProcAlone

        // In present month there's no end balance file
        if endBalanceFilename.IsSome then
            loadBalanceRpt EndingBalance db endBalanceFilename.Value customDtStr emailToProcAlone
        
        // Deposits import
        match depositsFilename with
        | Some depositsFilename -> 
                (db, depositsFilename, emailToProcAlone) 
                    |||> updDbDepositsRpt
        | None -> logger.Warn "No Deposits file to import."
        
        // Pending withdrawals import
        match withdrawalsPendingFilename with
        | Some withdrawalFilename -> 
            updDbWithdrawalsRpt db withdrawalFilename Pending emailToProcAlone
        | None -> logger.Warn "No pending withdrawal file to import."
        
        // Rollback withdrawals import
        match withdrawalsRollbackFilename with
        | Some withdrawalFilename -> 
                updDbWithdrawalsRpt db withdrawalFilename Rollback emailToProcAlone
        | None -> logger.Warn "No rollback withdrawal file to import."
        
        (db, customDtStr, emailToProcAlone) 
            |||> DbPlayerRevRpt.setDbMonthyGainLossAmounts 

        // Finally upsert GzTrx with the balance, credit amounts
        (db, customDtStr, emailToProcAlone)
            |||> DbGzTrx.setDbPlayerRevRpt2GzTrx

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
            (outFolder : string) 
            (emailToProcAlone : string option) =
        
        //------------ Read filenames
        logger.Info (sprintf "Reading the %s folder" inFolder)
        
        let reportfileNames = 
            { isProd = isProd ; folderName = inFolder } 
            |> getExcelFilenames

        /// Try 3 times to upload reports
        let dbOperation() = 
            (db, reportfileNames, emailToProcAlone) 
                |||> setDbimportExcel
        (db, dbOperation) 
            ||> tryDBCommit3Times

        // Archive / move excel files
        moveRptsToOutFolder 
            inFolder 
            outFolder 
            reportfileNames

        // Update null gmCustomerids
        setDbGmCustomerId db


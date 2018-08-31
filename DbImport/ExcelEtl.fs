namespace DbImport

module BalanceRpt2Db =
    open NLog
    open GzDb.DbUtil
    open GzBatchCommon
    open ExcelSchemas

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

    /// Process (upload to database) a single excel deposit
    let private updDbDepositsExcelRptRow
                (db : DbContext) 
                (excelRow : DepositsExcelSchema.Row) 
                (yyyyMmDd : string) =

        let currentYearMonth = yyyyMmDd.ToDateWithDay
        // Assume we always has an initiated date value
        let initiatedDt = (excelRow.Initiated |> excelObj2NullableDt WithdrawalRpt).Value
        let initiatedCurrently = isInitiatedInCurrentMonth initiatedDt currentYearMonth

        let completedDt = excelRow.Completed |> excelObj2NullableDt WithdrawalRpt
        let completedYyyyMmDd = if completedDt.HasValue then completedDt.Value.ToYyyyMmDd else "Null"
        let completedCurrently = isCompletedInCurrentMonth completedDt currentYearMonth 
        let completedInSameMonth = isCompletedInSameMonth completedDt initiatedDt
                
        let dateLogMsg = sprintf "Deposit for email %s initiatedDt: %s, completedDt: %s, completedCurrenntly: %b, completedInSameMonth: %b" excelRow.Email (initiatedDt.ToYyyyMmDd) completedYyyyMmDd completedCurrently completedInSameMonth
        logger.Debug dateLogMsg

        // less restrictive than withdrawals... completed in current processing month?
        if completedCurrently then
            DbPlayerRevRpt.updDbDepositsPlayerRow db yyyyMmDd excelRow

        else
            logger.Warn(sprintf "\nDeposits row not imported! Initiated: %O, CurrentDt: %s, CompletedDt: %s, initiatedCurrently: %b, completedCurrently: %b, completedInSameMonth: %b" 
                initiatedDt yyyyMmDd completedYyyyMmDd initiatedCurrently completedCurrently completedInSameMonth)

    /// Process all excel lines except Totals and upsert them
    let private updDbDepositsExcelRptRows 
                (db : DbContext) 
                (depositsExcelFile : DepositsExcelSchema) 
                (yyyyMmDd : string) 
                (emailToProcAlone: string option) : Unit =

        query {
            for excelRow in depositsExcelFile.Data do
            where (excelRow.``Debit real amount`` > 0.0 || excelRow.``Credit real  amount`` > 0.0)
            select excelRow
        }        
        |> Seq.iter (
            fun (excelRow) ->
                // Skip totals line
                let userId = excelRow.UserID |> getNonNullableUserId
                if userId > 0 then
                    let excelUserEmail = excelRow.Email
                
                    if emailToProcAlone.IsNone then
                        updDbDepositsExcelRptRow db excelRow yyyyMmDd
                
                    elif emailToProcAlone.IsSome && emailToProcAlone.Value = excelUserEmail then 
                        updDbDepositsExcelRptRow db excelRow yyyyMmDd
            )
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
                |> getDepositDtStr

        if depositsRptDtStr.IsSome then
            updDbDepositsExcelRptRows db openFile depositsRptDtStr.Value emailToProcAlone

module BonusRpt2Db =
    open NLog
    open System
    open GzBatchCommon
    open GzDb.DbUtil
    open ExcelSchemas
    open GmRptFiles
    open ExcelUtil

    let logger = LogManager.GetCurrentClassLogger()

    /// Process (upload to database) a single excel bonus deposit
    let private updDbBonusExcelRptRow
                (db : DbContext) 
                (excelRow : BonusExcel) 
                (yyyyMmDd : string) =

        let year = yyyyMmDd.Substring(0, 4) |> int
        let month = yyyyMmDd.Substring(4, 2) |> int

        if int excelRow.Year <> year && int excelRow.Month <> month then
            //logger.Warn( sprintf "%s" excelRow.
            logger.Warn(sprintf "\nBonus row for username %s Year %d Month %d not in sync with current YearMonthDay %s!" 
                excelRow.Username excelRow.Year excelRow.Month yyyyMmDd)

        logger.Debug(sprintf "\nBonus row for username %s Amount %f Currency %s" 
            excelRow.Username excelRow.Amount excelRow.Currency)
        // less restrictive than withdrawals... completed in current processing month?
        DbPlayerRevRpt.updDbBonusPlayerRow db yyyyMmDd excelRow

    let getEmailFromUsername(db : DbContext)(userId : int) : string =
        query {
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.UserID = userId)
                select playerDbRow.EmailAddress
                take 1
                exactlyOne
        }

    [<Literal>]
    let InvBonusID = "1029498"
    [<Literal>]
    let InvBonusName = "CASHBACK"
    [<Literal>]
    let GrantedStatus = "Cashout"

    /// Process all excel lines except Totals and upsert them
    let private updDbBonusExcelRptRows 
                (db : DbContext) 
                (bonusExcelFile : BonusExcelSchema) 
                (yyyyMmDd : string) 
                (emailToProcAlone: string option) : Unit =

        // Loop through all excel rows
        let invBonuses =
            query {
                for excelRow in bonusExcelFile.Data do
                where (
                    (excelRow.``Bonus program ID`` = InvBonusID || excelRow.``Bonus program`` = InvBonusName)
                    && excelRow.``Bonus amount`` > 0.0 
                    && excelRow.``bonus status`` = GrantedStatus
                    //&& excelRow.``Completed year`` = year
                    //&& excelRow.``granted month`` = month
                    )
                select {
                    Amount = excelRow.``Bonus amount``;
                    UserId = int excelRow.UserID;
                    Username = excelRow.Username;
                    Currency = excelRow.Currency;
                    Year = int excelRow.``Completed year``;
                    Month = int excelRow.``Completed month``;
                }
            }
        for excelRow in invBonuses do
            let userId = excelRow.UserId
            if userId > 0 then
                match emailToProcAlone with
                | None ->
                    updDbBonusExcelRptRow db excelRow yyyyMmDd
                
                | Some email ->
                    let excelPlayerEmail = 
                        excelRow.UserId
                        |> getEmailFromUsername db
                    if email = excelPlayerEmail then
                        updDbBonusExcelRptRow db excelRow yyyyMmDd
    
    /// Open a deposits excel file and console out the filename
    let private openBonusRptSchemaFile excelFilename = 
        let bonusExcelSchemaFile = BonusExcelSchema(excelFilename)
        logger.Info ""
        logger.Info (sprintf "************ Processing Bonus Report %s excel file" excelFilename)
        logger.Info ""
        bonusExcelSchemaFile
    
    /// Process the deposits (normal, vendor2user, cashbonus) excel file: Extract each row and upsert database deposits amounts
    let updDbBonusRpt 
        (db : DbContext)
        (bonusRptFullPath: string) 
        (emailToProcAlone: string option) =

        // Open excel report file for the memory schema
        let openFile = 
            bonusRptFullPath
            |> openBonusRptSchemaFile

        let bonusRptDtStr = 
            Some bonusRptFullPath 
                |> getBonusDtStr

        if bonusRptDtStr.IsSome then
            updDbBonusExcelRptRows db openFile bonusRptDtStr.Value emailToProcAlone

module CustomRpt2Db =
    open NLog
    open GzDb.DbUtil
    open GzBatchCommon
    open ExcelSchemas
    open GmRptFiles

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

module PlayerGamingRpt2Db =
    open NLog
    open GzDb.DbUtil
    open GzBatchCommon
    open ExcelSchemas
    open GmRptFiles

    let logger = LogManager.GetCurrentClassLogger()

    /// Open an player gaming excel file and console out the filename
    let private openPlayingActivityRptSchemaFile (excelFilename : string) : CasinoGameExcelSchema option = 
        try
            let playerGamingActivity = new CasinoGameExcelSchema(excelFilename)

            logger.Info ""
            logger.Info (sprintf "************ Processing Player Gaming Report %s excel file" excelFilename)
            logger.Info ""
            Some playerGamingActivity
            // There may be no activity
        with _ ->
            None
    
    let getUserIdFromUsername(db : DbContext)(username : string) : int =
        query {
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.Username = username)
                select playerDbRow.UserID
                take 1
                exactlyOne
        }

    /// Process all excel lines except Totals and upsert them
    let private insDbPlayerGamingExcelRptRows 
                (db : DbContext) 
                (gamingExcelFile : CasinoGameExcelSchema) 
                (yyyyMmDd : string) 
                (emailToProcAlone: string option) : Unit =

        //for excelRow in gamingExcelFile.Data do
        for excelRow in gamingExcelFile.Data do
            let username = excelRow.Item
            if objIsNotNull username then
                match username.Length with
                | 0 -> ()
                | _ -> DbPlayerRevRpt.setDbPlayerGamingRow db username yyyyMmDd excelRow
            else
                ()

    /// Process the custom excel file: Extract each row and upsert the database PlayerRevRpt table customer rows.
    let loadGamingActivityRpt 
            (db : DbContext) 
            (playerGamingRptFullPath: string) 
            (emailToProcOnly: string option) : unit =

        // Open excel report file for the memory schema
        match openPlayingActivityRptSchemaFile playerGamingRptFullPath with
        | Some openCasinoGamingExcelSchema -> 
            
            let playerGamingDtStr = getPlayerGamingDtStr playerGamingRptFullPath
            insDbPlayerGamingExcelRptRows db openCasinoGamingExcelSchema playerGamingDtStr emailToProcOnly

        | None -> logger.Info "No Casino Gaming Excel Activity."

module Etl =
    open System 
    open System.IO
    open GzDb.DbUtil
    open NLog
    open CustomRpt2Db
    open BalanceRpt2Db
    open WithdrawalRpt2Db
    open DbPlayerRevRpt
    open DepositsRpt2Db
    open BonusRpt2Db
    open GzBatchCommon
    open ExcelSchemas
    open GmRptFiles
    open PlayerGamingRpt2Db

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
        
        // Deposits
        match rpts.DepositsFilename with
        | Some excelFilename -> 
            (inFolder, outFolder, excelFilename) |||> moveFileWithOverwrite
        | None -> logger.Warn "No Vendor2User file to move."
        
        // Bonus
        match rpts.BonusFilename with
        | Some excelFilename -> 
            (inFolder, outFolder, excelFilename) |||> moveFileWithOverwrite
        | None -> logger.Warn "No Bonus file to move."
        
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

//--- Non investment files
        // Gaming Activity
        (inFolder, outFolder, rpts.casinoGameFilename) |||> moveFileWithOverwrite

        // Past Days Deposit Filename
        match rpts.PastDaysDepositsFilename with
        | Some excelFilename -> 
            (inFolder, outFolder, excelFilename) |||> moveFileWithOverwrite
        | None -> logger.Warn "No Past Days Deposits file to move."

    let private setDbimportExcel (db : DbContext)(reportFilenames : RptFilenames)(emailToProcAlone : string option) : unit =

        let { 
                customFilename = customFilename; 
                DepositsFilename = depositsFilename;
                BonusFilename = bonusFilename;
                withdrawalsPendingFilename = withdrawalsPendingFilename;
                withdrawalsRollbackFilename = withdrawalsRollbackFilename;
                begBalanceFilename = begBalanceFilename; 
                endBalanceFilename = endBalanceFilename;
                casinoGameFilename = casinoGameFilename;
            } : RptFilenames 
                    = reportFilenames

        let customDtStr = getCustomDtStr customFilename
        match emailToProcAlone with
        | None -> logger.Debug (sprintf "Starting processing excel report files for %s" customDtStr)
        | Some singleUserEmail -> 
            logger.Warn("********************************************************")
            logger.Warn(sprintf "Processing excel files in single user mode for %s." singleUserEmail)
            logger.Warn("********************************************************")

        // Try 3 times to upload
        // Bettings table non-investment related
        let loadGamingActivityRptOper() = loadGamingActivityRpt db casinoGameFilename emailToProcAlone
        tryDBCommit3Times db loadGamingActivityRptOper

        // Custom import
        // Try 3 times to upload
        let loadCustomRptOper() = loadCustomRpt db customFilename emailToProcAlone
        tryDBCommit3Times db loadCustomRptOper
        
        // Beg, end balance import
        if begBalanceFilename.IsSome then
            let loadBegBalanceRptOper() = loadBalanceRpt BeginingBalance db begBalanceFilename.Value customDtStr emailToProcAlone
            tryDBCommit3Times db loadBegBalanceRptOper

        // In present month there's no end balance file
        if endBalanceFilename.IsSome then
            let loadEndBalanceRptOper() = loadBalanceRpt EndingBalance db endBalanceFilename.Value customDtStr emailToProcAlone
            tryDBCommit3Times db loadEndBalanceRptOper
        
        // Deposits import
        match depositsFilename with
        | Some depositsFilename -> 
            let dbDepositsRptOper() = updDbDepositsRpt db depositsFilename emailToProcAlone
            tryDBCommit3Times db dbDepositsRptOper
        | None -> logger.Warn "No Deposits file to import."
        
        // Bonus import
        match bonusFilename with
        | Some bonusFilename -> 
            let dbBonusRptOper() = updDbBonusRpt db bonusFilename emailToProcAlone
            tryDBCommit3Times db dbBonusRptOper
        | None -> logger.Warn "No Bonus file to import."
        
        // Pending withdrawals import
        match withdrawalsPendingFilename with
        | Some withdrawalFilename -> 
            let dbWithdrawalsRptOper() = updDbWithdrawalsRpt db withdrawalFilename Pending emailToProcAlone
            tryDBCommit3Times db dbWithdrawalsRptOper
        | None -> logger.Warn "No pending withdrawal file to import."
        
        // Rollback withdrawals import
        match withdrawalsRollbackFilename with
        | Some withdrawalFilename -> 
            let dbWithdrawalsRptOper() = updDbWithdrawalsRpt db withdrawalFilename Rollback emailToProcAlone
            tryDBCommit3Times db dbWithdrawalsRptOper
        | None -> logger.Warn "No rollback withdrawal file to import."
        
        // User Gain Loss Investment Bonus
        let dbPlayerRevRptOper() = DbPlayerRevRpt.setDbMonthyGainLossAmounts db customDtStr emailToProcAlone
        tryDBCommit3Times db dbPlayerRevRptOper

        // Finally upsert GzTrx with the balance, credit amounts
        let dbGzTrxOper() = DbGzTrx.setDbPlayerRevRpt2GzTrx db customDtStr emailToProcAlone
        tryDBCommit3Times db dbGzTrxOper

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

        (db, reportfileNames, emailToProcAlone) 
            |||> setDbimportExcel

        // Update null gmCustomerids
        setDbGmCustomerId db

        // Archive / move excel files
        moveRptsToOutFolder 
            inFolder 
            outFolder 
            reportfileNames
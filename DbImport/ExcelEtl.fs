namespace DbImport

module DbGzTrx =
    open System
    open NLog
    open GzDb.DbUtil

    let logger = LogManager.GetCurrentClassLogger()

    /// Update a trx row with PlayerRevRpt as the source
    let updDbGzTrxRowValues 
            (amount : decimal)
            (creditPcntApplied : float32)
            (playerRevRpt : DbPlayerRevRpt)
            (trxRow : DbGzTrx) =

        trxRow.PlayerRevRptId <- Nullable playerRevRpt.Id
        trxRow.Amount <- amount
        trxRow.CreditPcntApplied <- Nullable creditPcntApplied
        trxRow.BegGmBalance <- playerRevRpt.BegBalance
        trxRow.EndGmBalance <- playerRevRpt.EndBalance
        trxRow.Deposits <- playerRevRpt.TotalDepositsAmount
        trxRow.Withdrawals <- Nullable <| playerRevRpt.WithdrawsMade.Value + playerRevRpt.PendingWithdrawals.Value
        trxRow.GainLoss <- playerRevRpt.PlayerGainLoss

    /// Create & Insert a GzTrxs row
    let insDbGzTrxRowValues
            (db : DbContext) 
            (yearMonth : string)
            (amount : decimal)
            (creditPcntApplied : float32)
            (gzUserId : int)
            (playerRevRpt : DbPlayerRevRpt) =

        let newGzTrxRow = 
            new DbGzTrx(
                CustomerId=gzUserId,
                YearMonthCtd = yearMonth,
                CreatedOnUTC = DateTime.UtcNow,
                TypeId = int GzTransactionType.CreditedPlayingLoss)

        updDbGzTrxRowValues amount creditPcntApplied playerRevRpt newGzTrxRow
        db.GzTrxs.InsertOnSubmit(newGzTrxRow)

    /// Get the greenzorro used id by email
    let getGzUserId (db : DbContext) (gmUserEmail : string) : int option =
        query {
            for user in db.AspNetUsers do
            where (user.Email = gmUserEmail)
            select user.Id
            exactlyOneOrDefault 
        }
        |> (fun userId ->
            if userId = 0 then
                logger.Warn (sprintf "*** Everymatrix email %s not found in the AspNetUsers table of db: %s. Cannot award..." gmUserEmail db.DataContext.Connection.DataSource)
                None
            else
                Some userId
        )

    /// Get the credited loss Percentage in human form % i.e. 50 from gzConfiguration
    /// <param name="db"></param>
    let getCreditLossPcnt (db : DbContext) : float32 =
        query {
            for c in db.GzConfigurations do
            exactlyOne
        }
        |> (fun conf -> conf.CREDIT_LOSS_PCNT)

    /// Main formula calculating the amount that will be credited to the users account
    /// (**** Player Loss in GzTrx is a positive amount. No point with negatives in there. ***)
    let getCreditedPlayerAmount (creditLossPcnt : float32)
                                (playerGainLoss : decimal) : decimal =

        let (|Gain|Loss|) (amount : decimal) = if amount >= 0M then Gain 0M else Loss amount
        match playerGainLoss with
        | Gain _ -> 0M
        | Loss lossAmount -> (decimal creditLossPcnt / 100m) * -lossAmount
    
    /// Upsert a GzTrxs transaction row with the credited amount
    let private setDbGzTrxRow(db : DbContext)(yyyyMmDd :string)(playerRevRpt : DbPlayerRevRpt) =

        let playerGainLoss = playerRevRpt.PlayerGainLoss.Value
        let yyyyMm = yyyyMmDd.Substring(0, 6) 
        let gmEmail = playerRevRpt.EmailAddress
        let gzUserId = getGzUserId db gmEmail

        if gzUserId.IsSome then
            query { 
                for trxRow in db.GzTrxs do
                    where (
                        trxRow.YearMonthCtd = yyyyMm
                        && trxRow.CustomerId = gzUserId.Value
                        && trxRow.GzTrxTypes.Code = int GzTransactionType.CreditedPlayingLoss
                    )
                    select trxRow
                    exactlyOneOrDefault
            }
            |> (fun trxRow ->
                let creditLossPcnt = getCreditLossPcnt db 
                let playerLossToInvest = getCreditedPlayerAmount creditLossPcnt playerGainLoss

                if isNull trxRow then
                    insDbGzTrxRowValues db yyyyMm playerLossToInvest creditLossPcnt gzUserId.Value playerRevRpt
                else 
                    updDbGzTrxRowValues playerLossToInvest creditLossPcnt playerRevRpt trxRow
            )
            db.DataContext.SubmitChanges()

    /// Read all the playerRevRpt latest monthly row and Upsert them as monthly GzTrxs transaction rows
    let setDbPlayerRevRpt2GzTrx (db : DbContext)(yyyyMmDd : string) =

        query { 
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.YearMonthDay = yyyyMmDd)
                select playerDbRow
        }
        |> Seq.iter (fun playerDbRow -> setDbGzTrxRow db yyyyMmDd playerDbRow)

module DbPlayerRevRpt =
    open System
    open NLog
    open GzDb.DbUtil
    open ExcelSchemas
    open ExcelUtil
    let logger = LogManager.GetCurrentClassLogger()

    /// Updated db player Gain Loss
    let private setDbPlayerGainLoss (row : DbSchema.ServiceTypes.PlayerRevRpt) : unit =
        let totalWithdrawals = row.WithdrawsMade.Value + row.PendingWithdrawals.Value
        // Formula to get player losses as negative amounts
        let gainLoss = 
            row.EndBalance.Value
            + totalWithdrawals
            - row.TotalDepositsAmount.Value 
            - row.BegBalance.Value 
        row.PlayerGainLoss <- Nullable gainLoss
        row.UpdatedOnUtc <- DateTime.UtcNow
        row.Processed <- int GmRptProcessStatus.GainLossRptUpd

    /// Query non zero balance affecting amounts and update the player GailLoss
    let setDbMonthyGainLossAmounts
                        (db : DbContext)
                        (yyyyMmDd :string) =

        query { 
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.YearMonthDay = yyyyMmDd 
                        && (
                            playerDbRow.BegBalance <> Nullable 0M 
                            || playerDbRow.EndBalance <> Nullable 0M
                            || playerDbRow.TotalDepositsAmount <> Nullable 0M
                            // Withdrawals that deduct balance but have not completed yet
                            || playerDbRow.PendingWithdrawals <> Nullable 0M
                            // Completed Withdrawals like TotalDepositsAmount is for Deposits
                            || playerDbRow.WithdrawsMade <> Nullable 0M
                        ))
                select playerDbRow
        }
        |> Seq.iter setDbPlayerGainLoss
        db.DataContext.SubmitChanges()

    /// Update the withdrawal amount with an addition for pending withdrawal amounts or deducting rollback withdrawal amounts
    /// Note: they may be multiple pending / rollback withdrawal transactions per user/month
    let private updDbRowWithdrawalsValues 
                    (withdrawalType : WithdrawalType) 
                    (withdrawalsExcelRow : WithdrawalsExcelSchema.Row) 
                    (playerRow : DbPlayerRevRpt) = 

        let dbWithdrawalAmount = playerRow.PendingWithdrawals.Value
        if withdrawalType = Pending then
            let newWithdrawalAmount = dbWithdrawalAmount + decimal withdrawalsExcelRow.``Debit amount``
            playerRow.PendingWithdrawals <- Nullable newWithdrawalAmount
        else
            let newWithdrawalAmount = dbWithdrawalAmount - decimal withdrawalsExcelRow.``Debit amount``
            playerRow.PendingWithdrawals <- Nullable newWithdrawalAmount
        //Non-excel content
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
        playerRow.Processed <- int GmRptProcessStatus.WithdrawsRptUpd

    /// Update begining balance amount of the selected month
    let private updDbRowBegBalanceValues 
            (begBalanceExcelRow : BalanceExcelSchema.Row) 
            (playerRow : DbPlayerRevRpt) = 

        playerRow.BegBalance <- begBalanceExcelRow.``Account balance`` |> float2NullableDecimal
        //Non-excel content
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
        playerRow.Processed <- int GmRptProcessStatus.BegBalanceRptUpd
    
    /// Update ending balance amount of the selected month
    let private updDbRowEndBalanceValues 
                (endBalanceExcelRow : BalanceExcelSchema.Row) (playerRow : DbPlayerRevRpt) = 

        // Zero out balance amounts, playerloss
        playerRow.EndBalance <- endBalanceExcelRow.``Account balance`` |> float2NullableDecimal
        //Non-excel content
        playerRow.Processed <- int GmRptProcessStatus.EndBalanceRptUpd
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
    
    /// Update most PlayerRevRpt row values from the CustomRpt values but without touching the id or insert time stamp.
    let private setDbRowCustomValues 
            (yearMonthDay : string) 
            (customExcelRow : CustomExcelSchema.Row) 
            (playerRow : DbPlayerRevRpt) = 

        playerRow.Username <- customExcelRow.Username
        playerRow.EmailAddress <- customExcelRow.``Email address``
        playerRow.TotalDepositsAmount <- customExcelRow.``Total deposits amount`` |> float2NullableDecimal
        playerRow.WithdrawsMade <- customExcelRow.``Withdraws made`` |> float2NullableDecimal
        playerRow.Currency <- customExcelRow.Currency.ToString()

        // Zero out gaming balance amounts, playerloss
        playerRow.BegBalance <- Nullable 0m
        playerRow.EndBalance <- Nullable 0m
        playerRow.PlayerGainLoss <- Nullable 0m

        // Withdrawals that deduct balance but have not completed yet they come from the pending report
        playerRow.PendingWithdrawals <- Nullable 0m

        //Non-excel content
        playerRow.YearMonth <- yearMonthDay.Substring(0, 6)
        playerRow.YearMonthDay <- yearMonthDay
        playerRow.UpdatedOnUtc <- DateTime.UtcNow
        playerRow.Processed <- int GmRptProcessStatus.CustomRptUpd
    
    /// Insert custom excel row values in db Row but don't touch the id and set the createdOnUtc time stamp.
    let insDbNewRowCustomValues
            (db : DbContext) 
            (yearMonthDay : string) 
            (excelRow : CustomExcelSchema.Row) = 

        let newPlayerRow = 
            new DbPlayerRevRpt(UserID = (int) excelRow.``User ID``, CreatedOnUtc = DateTime.UtcNow)
        setDbRowCustomValues yearMonthDay excelRow newPlayerRow
        db.PlayerRevRpt.InsertOnSubmit(newPlayerRow)

    /// Set withdrawal amount in a db PlayerRevRpt Row
    let updDbWithdrawalsPlayerRow 
                        (withdrawalType : WithdrawalType)
                        (db : DbContext)
                        (yyyyMmDd :string)
                        (withdrawalRow : WithdrawalsExcelSchema.Row) = 

        let gmUserId = (int) withdrawalRow.``User ID``
        logger.Info(sprintf "Importing %A withdrawal user id %d on %s/%s/%s" 
                withdrawalType <| gmUserId <| yyyyMmDd.Substring(6, 2) <| yyyyMmDd.Substring(4, 2) <| yyyyMmDd.Substring(0, 4))
        query { 
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.YearMonthDay = yyyyMmDd && playerDbRow.UserID = gmUserId)
                select playerDbRow
                exactlyOneOrDefault
        }
        |> (fun playerDbRow -> 
            if isNull playerDbRow then 
                let failMsg = sprintf "When importing a withdrawals file you can't have a user Id %d that is not found in the PlayerRevRpt table!" gmUserId
                failwith failMsg
            else
                updDbRowWithdrawalsValues withdrawalType withdrawalRow playerDbRow
        )
        db.DataContext.SubmitChanges()

    /// Set beginning or ending amount in a db PlayerRevRpt Row
    let updDbBalances 
            (balanceType : BalanceType)
            (db : DbContext)
            (yyyyMmDd :string) 
            (balanceRow : BalanceExcelSchema.Row) = 

        let gmUserId = (int) balanceRow.``User ID``
        query { 
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.YearMonthDay = yyyyMmDd && playerDbRow.UserID = gmUserId)
                select playerDbRow
                exactlyOneOrDefault
        }
        |> (fun playerDbRow -> 
            if isNull playerDbRow then 
                let failMsg = sprintf "When importing a balance file you can't have a user Id %d that is not found in the PlayerRevRpt table!" gmUserId
                failwith failMsg
            else
                match balanceType with
                | BeginingBalance -> updDbRowBegBalanceValues balanceRow playerDbRow
                | EndingBalance -> updDbRowEndBalanceValues balanceRow playerDbRow
        )
        db.DataContext.SubmitChanges()

    /// Upsert excel row values in a db PlayerRevRpt Row
    let setDbCustomPlayerRow (db : DbContext) 
                        (yyyyMmDd :string) 
                        (customExcelRow : CustomExcelSchema.Row) = 

        let gmUserId = (int) customExcelRow.``User ID``
        query { 
            for playerDbRow in db.PlayerRevRpt do
                where (playerDbRow.YearMonthDay = yyyyMmDd && playerDbRow.UserID = gmUserId)
                select playerDbRow
                exactlyOneOrDefault
        }
        |> (fun playerDbRow -> 
            if isNull playerDbRow then 
                insDbNewRowCustomValues db yyyyMmDd customExcelRow
            else 
                setDbRowCustomValues yyyyMmDd customExcelRow playerDbRow
        )
        db.DataContext.SubmitChanges()

module WithdrawalRpt2Db =
    open System
    open NLog
    open GzCommon
    open GzDb.DbUtil
    open ExcelSchemas
    open GmRptFiles
    open ExcelUtil

    let logger = LogManager.GetCurrentClassLogger()

    /// Withdrawal performed in current processing month
    let private isInitiatedInCurrentMonth(initiatedDate : DateTime)(currentYm : DateTime) : bool =
        currentYm.Month = initiatedDate.Month && currentYm.Year = initiatedDate.Year

    let private completedEqThisDateMonth (completedDt : DateTime Nullable)(thisDate : DateTime) : bool =
        if not completedDt.HasValue then
            false
        else 
            thisDate.Month = completedDt.Value.Month && thisDate.Year = completedDt.Value.Year

    /// Withdrawal completion performed within current processing month
    let private isCompletedInCurrentMonth (completedDt : DateTime Nullable)(currentYm : DateTime) : bool =
        completedEqThisDateMonth completedDt currentYm

    /// Withdrawal initiated & completed in the reported month
    let private isCompletedInSameMonth (completedDt : DateTime Nullable)(initiatedDate : DateTime) : bool =
        completedEqThisDateMonth completedDt initiatedDate

    /// Process all excel lines except Totals and upsert them
    let private updDbWithdrawalsExcelRptRows 
                (db : DbContext) 
                (withdrawalExcelFile : WithdrawalsExcelSchema) 
                (yyyyMmDd : string) =

        let currentYearMonth = yyyyMmDd.ToDateWithDay
        // Loop through all excel rows
        for excelRow in withdrawalExcelFile.Data do
            // Skip totals line and 
            if int excelRow.``User ID`` > 0 then
                // Assume we always has an initiated date value
                let initiatedDt = (excelRow.Initiated |> excelObj2NullableDt WithdrawalRpt).Value
                let initiatedCurrently = isInitiatedInCurrentMonth initiatedDt currentYearMonth

                let completedDt = excelRow.Completed |> excelObj2NullableDt WithdrawalRpt
                let completedCurrently = isCompletedInCurrentMonth completedDt currentYearMonth 
                let completedInSameMonth = isCompletedInSameMonth completedDt initiatedDt 

                let transStatus = excelRow.``Trans status``.ToString().ToLower()

                // initiatedCurrently && CompletedInSameMonth are within TotalWithdrawals in Custom
                if initiatedCurrently && not completedInSameMonth then
                    DbPlayerRevRpt.updDbWithdrawalsPlayerRow Pending db yyyyMmDd excelRow

                else if not initiatedCurrently && completedCurrently && transStatus = "rollback" then
                    DbPlayerRevRpt.updDbWithdrawalsPlayerRow Rollback db yyyyMmDd excelRow
                else
                    logger.Warn(sprintf "Withdrawal row not imported! Initiated: %O, CurrentDt: %s, CompletedDt Val: %A, trans status: %s, initiatedCurrently: %b, completedCurrently: %b, completedInSameMonth: %b" initiatedDt yyyyMmDd completedDt.Value transStatus initiatedCurrently completedCurrently completedInSameMonth)
    
    /// Open an excel file and return its memory schema
    let private openWithdrawalRptSchemaFile (excelFilename : string) : WithdrawalsExcelSchema =
        let withdrawalsFileExcelSchema = new WithdrawalsExcelSchema(excelFilename)
        logger.Info ""
        logger.Info (sprintf "************ Processing Withdrawals Report from filename: %s " excelFilename)
        logger.Info ""
        withdrawalsFileExcelSchema
    
    /// Process the pending withdrawal excel files: Extract each row and update database customer pending withdrawal amounts
    let updDbWithdrawalsRpt 
                (db : DbContext) 
                (withdrawalsRptFullPath: string) =

        // Open excel report file for the memory schema
        let openFile = withdrawalsRptFullPath |> openWithdrawalRptSchemaFile
        let withdrawalRptDtStr = withdrawalsRptFullPath |> getWithdrawalDtStr
        try 
            updDbWithdrawalsExcelRptRows db openFile withdrawalRptDtStr
        with _ -> 
            reraise()
            
module BalanceRpt2Db =
    open NLog
    open GzDb.DbUtil
    open ExcelSchemas

    let logger = LogManager.GetCurrentClassLogger()

    /// Process all excel lines except Totals and upsert them
    let importBalanceExcelRptRows 
                (balanceType : BalanceType)
                (db : DbContext) 
                (balanceExcelFile : BalanceExcelSchema) 
                (yyyyMmDd : string) =

        // Loop through all excel rows
        for excelRow in balanceExcelFile.Data do
            // Skip totals line
            if excelRow.``User ID`` > 0.0 then
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

        //Curry with balance Type
        let balanceTypedOpenSchema = openBalanceRptSchemaFile balanceType
        // Open excel report file for the memory schema
        let openFile = balanceRptFullPath |> balanceTypedOpenSchema
        try 
            importBalanceExcelRptRows balanceType db openFile customYyyyMmDd
        with _ -> 
            reraise()

module CustomRpt2Db =
    open NLog
    open GzDb.DbUtil
    open ExcelSchemas
    open GmRptFiles
    let logger = LogManager.GetCurrentClassLogger()

    /// Process all excel lines except Totals and upsert them
    let private setDbCustomExcelRptRows 
                (db : DbContext) (customExcelSchemaFile : CustomExcelSchema) 
                (yyyyMmDd : string) =

        // Loop through all excel rows
        for excelRow in customExcelSchemaFile.Data do
            if not <| isNull excelRow.``Email address`` then
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
            (customRptFullPath: string) =

        // Open excel report file for the memory schema
        let openFile = customRptFullPath |> openCustomRptSchemaFile
        let yyyyMmDd = customRptFullPath |> getCustomDtStr
        try 
            setDbCustomExcelRptRows db openFile yyyyMmDd
        with _ -> 
            reraise()

module Etl = 
    open System.IO
    open GzDb.DbUtil
    open NLog
    open GmRptFiles
    open ExcelSchemas
    open CustomRpt2Db
    open BalanceRpt2Db
    open WithdrawalRpt2Db

    let logger = LogManager.GetCurrentClassLogger()

    /// Move processed files to out folder except endBalanceFile which is the next month's begBalanceFile
    let private moveRptsToOutFolder 
            (inFolder : string) (outFolder :string) 
            (customFilename : string) (begBalanceFilename : string) (withdrawalFilename : string) : unit =
        
        File.Move(customFilename, customFilename.Replace(inFolder, outFolder))
        // Move only the beginning balance file.
        File.Move(begBalanceFilename, begBalanceFilename.Replace(inFolder, outFolder))
        File.Move(withdrawalFilename, withdrawalFilename.Replace(inFolder, outFolder))


    let private setDbimportExcel (db : DbContext)(reportFilenames : RptFilenames) : unit =
        let { 
                customFilename = customFilename; 
                withdrawalFilename = withdrawalFilename; 
                begBalanceFilename = begBalanceFilename; 
                endBalanceFilename = endBalanceFilename 
            }   = reportFilenames

        let customDtStr = getCustomDtStr customFilename
        logger.Debug (sprintf "Starting processing excel report files for %O" customDtStr)

        (db, customFilename) ||> loadCustomRpt 
        loadBalanceRpt BeginingBalance db begBalanceFilename customDtStr
        loadBalanceRpt EndingBalance db endBalanceFilename customDtStr
        (db, withdrawalFilename) ||> updDbWithdrawalsRpt
        (db, customDtStr) ||> DbPlayerRevRpt.setDbMonthyGainLossAmounts 
        // Finally upsert GzTrx with the balance, credit amounts
        (db, customDtStr) ||> DbGzTrx.setDbPlayerRevRpt2GzTrx

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

        /// Box function pointer with required parameters
        let dbOperation() = (db, reportfileNames) ||> setDbimportExcel
        (db, dbOperation) ||> tryDBCommit3Times

        moveRptsToOutFolder inFolder outFolder reportfileNames.customFilename reportfileNames.begBalanceFilename reportfileNames.withdrawalFilename


namespace DbImport

module WithdrawalRpt2Db =
    open System
    open NLog
    open GzBatchCommon
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

    let private updDbWithdrawalsExcelRptRow
        (db : DbContext) 
        (excelRow : WithdrawalsPendingExcelSchema.Row) 
        (yyyyMmDd : string) 
        (withdrawalType : WithdrawalType) =

        let currentYearMonth = yyyyMmDd.ToDateWithDay

        // Assume we always have an initiated date value
        let initiatedDt = (excelRow.Initiated |> excelObj2NullableDt WithdrawalRpt).Value
        let initiatedCurrently = isInitiatedInCurrentMonth initiatedDt currentYearMonth

        let completedDt = excelRow.Completed |> excelObj2NullableDt WithdrawalRpt
        let completedYyyyMmDd = if completedDt.HasValue then completedDt.Value.ToYyyyMmDd else "Null"
        let completedCurrently = isCompletedInCurrentMonth completedDt currentYearMonth 
        let completedInSameMonth = isCompletedInSameMonth completedDt initiatedDt
                
        let rollbackNote = excelRow.``Last note``.IndexOf("Rollback") <> -1

        let dateLogMsg = 
            sprintf "Withdrawal for email: %s initiatedDt: %s, completedDt: %s, completedCurrenntly: %b, completedInSameMonth: %b, rollbackNote: %b" 
                excelRow.Email
                (initiatedDt.ToYyyyMmDd) 
                completedYyyyMmDd 
                completedCurrently 
                completedInSameMonth 
                rollbackNote
        logger.Debug dateLogMsg

        // initiatedCurrently && not CompletedInSameMonth are true pending
        if withdrawalType = Pending && initiatedCurrently && not completedInSameMonth && not rollbackNote then
            DbPlayerRevRpt.updDbWithdrawalsPlayerRow Pending db yyyyMmDd excelRow.Email excelRow

        // initiatedCurrently && CompletedInSameMonth are Completed Withdrawals and we track in the user currency
        elif withdrawalType = Pending && initiatedCurrently && completedInSameMonth && not rollbackNote then
            DbPlayerRevRpt.updDbWithdrawalsPlayerRow Completed db yyyyMmDd excelRow.Email excelRow

        elif withdrawalType = Rollback && not initiatedCurrently && completedCurrently then
            DbPlayerRevRpt.updDbWithdrawalsPlayerRow Rollback db yyyyMmDd excelRow.Email excelRow

        elif withdrawalType = Rollback && initiatedCurrently && completedCurrently then
            logger.Warn(sprintf "\nIgnoring rollback withdrawal because it's initiated and completed in the same month for email: %s Initiated: %s, CompletedDt: %s, amount: %f, last note: %s" 
                excelRow.Email 
                initiatedDt.ToYyyyMmDd 
                completedYyyyMmDd
                excelRow.``Credit real  amount``
                excelRow.``Last note``)
        else
            logger.Warn(sprintf "\nUnknown withdrawal for email: %s it's not imported! Type %s, Initiated: %s, CurrentDt: %s, CompletedDt: %s, rollbackNote: %s" 
                excelRow.Email 
                (WithdrawalTypeString withdrawalType)
                initiatedDt.ToYyyyMmDd
                yyyyMmDd 
                completedYyyyMmDd
                excelRow.``Last note``)

    /// Process all excel lines except Totals and upsert them
    let private updDbWithdrawalsExcelRptRows 
                (db : DbContext) 
                (withdrawalExcelFile : WithdrawalsPendingExcelSchema) 
                (yyyyMmDd : string) 
                (withdrawalType : WithdrawalType)
                (emailToProcAlone: string option) =

        // Loop through all excel rows
        for excelRow in withdrawalExcelFile.Data do
            // Skip totals line
            let userId = excelRow.UserID |> getNonNullableUserId
            if userId > 0 then
                let excelUserEmail = excelRow.Email

                // Normal processing case
                if emailToProcAlone.IsNone then
                    updDbWithdrawalsExcelRptRow db excelRow yyyyMmDd withdrawalType

                elif emailToProcAlone.IsSome && emailToProcAlone.Value = excelUserEmail then
                    updDbWithdrawalsExcelRptRow db excelRow yyyyMmDd withdrawalType

                // Skip case of emailToProcAlone.IsSome && emailToProcAlone.Value <> excelUserEmail
    
    /// Open an excel file and return its memory schema
    let private openWithdrawalRptSchemaFile (excelFilename : string) : WithdrawalsPendingExcelSchema =
        let withdrawalsFileExcelSchema = new WithdrawalsPendingExcelSchema(excelFilename)
        logger.Info ""
        logger.Info (sprintf "************ Processing Withdrawals Report from filename: %s " excelFilename)
        logger.Info ""
        withdrawalsFileExcelSchema
    
    /// Process the pending withdrawal excel files: Extract each row and update database customer pending withdrawal amounts
    let updDbWithdrawalsRpt 
                (db : DbContext) 
                (withdrawalsRptFullPath: string) 
                (withdrawalType : WithdrawalType)
                (emailToProcAlone: string option) =

        // Open excel report file for the memory schema
        let openFile = 
            withdrawalsRptFullPath 
            |> openWithdrawalRptSchemaFile

        let withdrawalRptDtStr = 
            Some withdrawalsRptFullPath 
                |> getWithdrawalDtStr

        if withdrawalRptDtStr.IsSome then
            updDbWithdrawalsExcelRptRows db openFile withdrawalRptDtStr.Value withdrawalType emailToProcAlone
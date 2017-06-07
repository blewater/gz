namespace DbImport

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
                (withdrawalExcelFile : WithdrawalsPendingExcelSchema) 
                (yyyyMmDd : string) 
                (withdrawalType : WithdrawalType) =

        let currentYearMonth = yyyyMmDd.ToDateWithDay
        // Loop through all excel rows
        for excelRow in withdrawalExcelFile.Data do
            // Skip totals line
            let userId = excelRow.UserID |> getNonNullableUserId
            if userId > 0 then
                // Assume we always has an initiated date value
                let initiatedDt = (excelRow.Initiated |> excelObj2NullableDt WithdrawalRpt).Value
                let initiatedCurrently = isInitiatedInCurrentMonth initiatedDt currentYearMonth

                let completedDt = excelRow.Completed |> excelObj2NullableDt WithdrawalRpt
                let completedCurrently = isCompletedInCurrentMonth completedDt currentYearMonth 
                let completedInSameMonth = isCompletedInSameMonth completedDt initiatedDt 

                // initiatedCurrently && CompletedInSameMonth are within TotalWithdrawals in Custom
                if withdrawalType = Pending && initiatedCurrently && not completedInSameMonth then
                    DbPlayerRevRpt.updDbWithdrawalsPlayerRow Pending db yyyyMmDd excelRow

                else if withdrawalType = Rollback && not initiatedCurrently && completedCurrently then
                    DbPlayerRevRpt.updDbWithdrawalsPlayerRow Rollback db yyyyMmDd excelRow
                else
                    logger.Warn(sprintf "\nWithdrawal row not imported! Initiated: %O, CurrentDt: %s, CompletedDt: %A, initiatedCurrently: %b, completedCurrently: %b, completedInSameMonth: %b" 
                        initiatedDt yyyyMmDd completedDt initiatedCurrently completedCurrently completedInSameMonth)
    
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
                (withdrawalType : WithdrawalType) =

        // Open excel report file for the memory schema
        let openFile = 
            withdrawalsRptFullPath 
            |> openWithdrawalRptSchemaFile

        let withdrawalRptDtStr = 
            Some withdrawalsRptFullPath 
                |> getWithdrawalDtStr

        if withdrawalRptDtStr.IsSome then
            updDbWithdrawalsExcelRptRows db openFile withdrawalRptDtStr.Value withdrawalType
namespace DbImport

module ExcelSchemas =
    open System
    open FSharp.ExcelProvider
    open NLog

    // LGA Reports...Player Balances
    type BalanceExcelSchema = ExcelFile< "Balance Prod 20161001.xlsx" >
    type CustomExcelSchema = ExcelFile< "Custom Prod 20170601.xlsx" >
    // Transactions Rpt: Withdrawal Type + Initiated + Pending + Rollback
    type WithdrawalsPendingExcelSchema = ExcelFile< "WithdrawalsPending prod 20160930.xlsx" >
    // Transactions Rpt: Withdrawal Type + Completed + Rollback
    type WithdrawalsRollbackExcelSchema = WithdrawalsPendingExcelSchema

    type RptType =
    | CustomRptExcel
    | BalanceRptExcel
    | WithdrawalsRptExcel

    type BalanceType =
    | BeginingBalance
    | EndingBalance

    type WithdrawalType =
    | Pending
    | Rollback

    let WithdrawalTypeString = function 
        | Pending -> "Pending"
        | Rollback -> "Rollback"

    let logger = LogManager.GetCurrentClassLogger()    


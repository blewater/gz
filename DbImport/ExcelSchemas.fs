namespace DbImport

module ExcelSchemas =
    open System
    open FSharp.ExcelProvider

    // LGA Reports...Player Balances
    type BalanceExcelSchema = ExcelFile< "Balance Prod 20161001.xlsx" >
    type CustomExcelSchema = ExcelFile< "Custom Prod 20170601.xlsx" >
    // Transactions Rpt: Withdrawal Type + Initiated + Pending + Rollback
    type WithdrawalsPendingExcelSchema = ExcelFile< "WithdrawalsPending prod 20160930.xlsx" >
    // Transactions Rpt: Withdrawal Type + Completed + Rollback
    type WithdrawalsRollbackExcelSchema = WithdrawalsPendingExcelSchema
    type Vendor2UserExcelSchema = ExcelFile< "Vendor2User Prod 20170606.xlsx" >

    type RptType =
    | CustomRptExcel
    | BalanceRptExcel
    | WithdrawalsRptExcel
    | Vendor2UserRptExcel

    type BalanceType =
    | BeginingBalance
    | EndingBalance

    type WithdrawalType =
    | Pending
    | Rollback

    type Vendor2UserAmountType =
    | V2UDeposit
    | V2UCashBonus

    let WithdrawalTypeString = function 
        | Pending -> "Pending"
        | Rollback -> "Rollback"
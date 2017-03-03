namespace DbImport

module ExcelSchemas =
    open System
    open FSharp.ExcelProvider
    open NLog

    // Compile type
    type BalanceExcelSchema = ExcelFile< "Balance Prod 20161001.xlsx" >
    type CustomExcelSchema = ExcelFile< "Custom Prod 20160930.xlsx" >
    type WithdrawalsExcelSchema = ExcelFile< "pendingwithdraw prod 20160930.xlsx" >

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

    let logger = LogManager.GetCurrentClassLogger()    


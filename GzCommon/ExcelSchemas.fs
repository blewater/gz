namespace GzBatchCommon

module ExcelSchemas =
    open FSharp.ExcelProvider

    // LGA Reports...Player Balances
    type BalanceExcelSchema = ExcelFile< "Balance Prod 20161001.xlsx" >
    type CustomExcelSchema = ExcelFile< "Custom Prod 20180309.xlsx" >
    // Transactions Rpt: Withdrawal Type + Initiated + Pending + Rollback
    type WithdrawalsPendingExcelSchema = ExcelFile< "WithdrawalsPending prod 20160930.xlsx" >
    // Transactions Rpt: Withdrawal Type + Completed + Rollback
    type WithdrawalsRollbackExcelSchema = WithdrawalsPendingExcelSchema
    type DepositsExcelSchema = ExcelFile< "Vendor2User Prod 20170606.xlsx" >
    type PastDepositsExcelSchema = ExcelFile< "Vendor2User Prod 20170606.xlsx" >
    type BonusExcelSchema = ExcelFile< "Bonus Prod 20171212.xlsx" >
    type CurrencyExchanges = ExcelFile< "ExchangeRate-31-08-2017.xlsx" >
    type CasinoGameReport = ExcelFile< "CasinoGameReport 20170509.xlsx" >

    type BonusExcel = 
        {
            Amount : float;
            UserId : int;
            Username : string;
            Currency : string;
            Year : int;
            Month : int;
        }

    type DepositOrWithdrawalTrxType =
        | DepositTrx
        | WithdrawalTrx

    type RptType =
    | CustomRptExcel
    | BalanceRptExcel
    | WithdrawalsRptExcel
    | Vendor2UserRptExcel
    | PastDepositsExcel
    | ExchRateRptExcel
    | CasinoGameReport

    type BalanceType =
    | BeginingBalance
    | EndingBalance

    type WithdrawalType =
    | Pending
    | Rollback
    | Completed

    type DepositsAmountType =
    | V2UDeposit    // Vendor2User Deposit (free spins bonus? usually 0)
    | V2UCashBonus  // Vendor2User Cash Bonus (vintage withdrawal or a Gz cash gift to user)
    | Deposit       // Normal User Deposit

    let WithdrawalTypeString = function 
        | Pending -> "Pending"
        | Rollback -> "Rollback"
        | Completed -> "Completed"
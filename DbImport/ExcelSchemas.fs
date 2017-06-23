namespace DbImport

module ExcelSchemas =
    open System
    open FSharp.ExcelProvider

    // LGA Reports...Player Balances
    type BalanceExcelSchema = ExcelFile< "Balance Prod 20161001.xlsx" >
    type CustomExcelSchema = ExcelFile< "Custom Prod 20170611.xlsx" >
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

    /// Everymatrix Portal
    type EverymatrixPortalArgsType = {
        EmailReportsUser : string;
        EmailReportsPwd : string;
        EverymatrixPortalUri : Uri;
        EverymatrixUsername : string;
        EverymatrixPassword : string;
        EverymatrixToken : string;
    }

    /// Reports download folders & move location
    type ReportsFoldersType = {
        BaseFolder : string;
        ExcelInFolder : string; 
        ExcelOutFolder : string;
        reportsDownloadFolder : string;
    }

    /// Reports Files & Download Args
    type ReportsFilesArgsType = {
        DownloadedCustomFilter : string;
        DownloadedBalanceFilter : string;
        DownloadedWithdrawalsFilter : string;
        DownloadedVendor2UserFilter : string;
        CustomRptFilenamePrefix : string;
        EndBalanceRptFilenamePrefix : string;
        WithdrawalsPendingRptFilenamePrefix : string;
        WithdrawalsRollbackRptFilenamePrefix : string;
        Vendor2UserRptFilenamePrefix : string;
        Wait_For_File_Download_MS : int;
    }

    type EverymatriReportsArgsType = {
        EverymatrixPortalArgs : EverymatrixPortalArgsType;
        ReportsFoldersArgs : ReportsFoldersType;
        ReportsFilesArgs : ReportsFilesArgsType;
    }

    type BalanceFilesUsageType =
    | SkipBalanceFiles = 1
    | SkipEndBalanceFile = 2
    | UseBothBalanceFiles = 3


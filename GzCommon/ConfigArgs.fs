namespace GzBatchCommon

module ConfigArgs =
    open System

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
        downloadedDepositsFilter : string;
        CustomRptFilenamePrefix : string;
        EndBalanceRptFilenamePrefix : string;
        WithdrawalsPendingRptFilenamePrefix : string;
        WithdrawalsRollbackRptFilenamePrefix : string;
        depositsRptFilenamePrefix : string;
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

    type UserEmailProcOnlyType = string option

    type ProcessingModeType =
    | ExcelDownloading
    | ExcelUploading
    | FullExcelProcessing
    | AwardStock
    | FullProcessing

    type CmdArgs = {
        BalanceFilesUsage : BalanceFilesUsageType;
        UserEmailProcAlone : UserEmailProcOnlyType;
        ProcessingMode : ProcessingModeType;
    }
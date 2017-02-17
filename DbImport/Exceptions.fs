namespace DbImport

module Exceptions =
    open System

    type DomainException = 
    | DbUpdateFailure of Exception
    | MissingCustomReport of Exception
    | Missing1stBalanceReport of Exception
    | Missing2ndBalanceReport of Exception
    | MissingWithdrawalReport of Exception
    | MissingDateInFilename of Exception
    | NoparsableDateInFilename of Exception
    | MismatchedFilenameDates of Exception
    | BegBalanceDateMismatch of Exception
    | EndBalanceDateMismatch of Exception
    | RptDateProcessed of Exception


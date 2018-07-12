namespace GzBatchCommon

[<AutoOpen>]
module ErrorHandling =
    open System
    open NLog

    type DomainException = 
    | MissingCustomReport of Exception
    | Missing1stBalanceReport of Exception
    | Missing2ndBalanceReport of Exception
    | MissingWithdrawalReport of Exception
    | MissingDateInFilename of Exception // Reserved for future use
    | NoparsableDateInFilename of Exception // Reserved for future use
    | MismatchedFilenameDates of Exception // Reserved for future use
    | BegBalanceDateMismatch of Exception // Reserved for future use
    | NoCurrentMarketQuote of Exception
    // Non-investment files
    | MissingPlayerGamingReport of Exception

    let logger = LogManager.GetCurrentClassLogger()

    /// Log error with domain exception, log message and runtime exception
    let private logError taskExc (logInfo : 'I) (ex : Exception) =
        let logMsg = sprintf "Operation failed with %A. Here's additional info: %A" taskExc logInfo
        logger.Fatal (ex, logMsg)
    
    // try to call with a DomailException and Option-al logInfo
    let tryF f taskExc (logInfo : 'I) =
        try 
            f()
        with ex ->
            logError taskExc logInfo ex
            raise ex

    /// community breakpoint for peeking intermediate transformations
    let breakpoint fn value = 
        let result = fn value
        printfn "%A" result
        result        

    /// invalidArg with logging
    let failWithLogInvalidArg excMsg logMsg =
        logger.Fatal (excMsg + " : " + logMsg)
        invalidArg excMsg logMsg

    /// Assert with runtime exception
    let traceExc (condition : bool)(message : string) : unit =
        if not condition then
            ("Assert failed: ", message) ||> failWithLogInvalidArg

    /// Emails not allowed
    let allowedPlayerEmail (emailAddress : string) : bool =
        match emailAddress.ToLowerInvariant() with
        | playerAddress when playerAddress.Contains("test@") -> false
        | playerAddress when playerAddress.Contains("@noemail.com") -> false
        | playerAddress when playerAddress.Contains("@everymatrix.com") -> false
        //| playerAddress when playerAddress.Contains("@mailinator.com") -> false
        //| playerAddress when playerAddress.Contains("@sharklasers.com") -> false
        | playerAddress when playerAddress.Contains("@asdasd") -> false
        | playerAddress when playerAddress.Contains("@test.gr") -> false
        | playerAddress when playerAddress.Contains("test1@yopmail") -> false
        | _ -> true




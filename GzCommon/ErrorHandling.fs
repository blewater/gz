namespace GzCommon

[<AutoOpen>]
module ErrorHandling =

    open System
    open NLog

    let logger = LogManager.GetCurrentClassLogger()

    let private logError taskExc (logInfo : 'I Option) (ex : Exception) =
        let logMsg = 
            match logInfo with
            | Some info -> sprintf "Operation failed with %A. Here's additional info: %A" taskExc info
            | None -> sprintf "Operation failed with %A." taskExc
        logger.Fatal logMsg
        logger.Error ("**Exception: " + ex.Message)
    
    // try to call with a DomailException and Option-al logInfo
    let tryF f taskExc (logInfo : 'I Option) =
        try 
            f()
        with ex ->
            logError taskExc logInfo ex
            raise ex

    // invalidArg with logging
    let failWithLogInvalidArg excMsg logMsg =
        logger.Fatal (excMsg + " : " + logMsg)
        invalidArg excMsg logMsg

    let traceExc (condition : bool)(message : string) : unit =
        Diagnostics.Trace.Assert(condition, message)
        if not condition then
            ("Assert failed: ", message) ||> failWithLogInvalidArg


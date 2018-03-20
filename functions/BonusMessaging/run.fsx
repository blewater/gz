#r "./packages/MailKit/lib/net45/MailKit.dll"
#r "./packages/MimeKit/lib/net45/MimeKit.dll"

let Run(inputMessage: string, log: TraceWriter) =
    log.Info(sprintf "F# Queue trigger function processed: '%s'" inputMessage)

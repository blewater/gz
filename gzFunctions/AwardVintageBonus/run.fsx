open AngleSharp;
open AngleSharp.Dom.Html;
open AngleSharp.Extensions;

let Run(inputMessage: string, log: TraceWriter) =
    log.Info(sprintf "F# Queue trigger function processed: '%s'" inputMessage)

#if INTERACTIVE

#I @"C:/Users/Mario/AppData/Roaming/npm/node_modules/azure-functions-core-tools/bin/"
#r "Microsoft.Azure.Webjobs.Host.dll"
#r @"C:/Users/Mario/data/Functions/packages/nuget/mailkit/2.0.2/lib/net45/MailKit.dll"
#r @"C:/Users/Mario/data/Functions/packages/nuget/mimekit/2.0.2/lib/net45/MimeKit.dll"
#r @"C:/Users/Mario/data/Functions/packages/nuget/fsharp.data.typeproviders/5.0.0.2/lib/net40/FSharp.Data.TypeProviders.dll"
#r @"C:/Users/Mario/data/Functions/packages/nuget/fsharp.azure.storagetypeprovider/1.9.5/lib/net452/Microsoft.WindowsAzure.Storage.dll"
#r @"C:/Users/Mario/data/Functions/packages/nuget/fsharp.azure.storagetypeprovider/1.9.5/lib/net452/FSharp.Azure.StorageTypeProvider.dll"
open Microsoft.Azure.WebJobs.Host

#endif
open System
open FSharp.Azure.StorageTypeProvider
//open FSharp.Azure.StorageTypeProvider.Table
open FSharp.Data.TypeProviders
open MimeKit
open MailKit.Net.Smtp
open MailKit.Security

type AzStorage = AzureTypeProvider<"DefaultEndpointsProtocol=https;AccountName=gzazurefunctionsstorage;AccountKey=70GYx9psHLDTh+y4TBIYNb2OSkXRWy5JTIdFFGL7GY3EsrTnebs8ztsyEu3ro5ZEnOm1g8RmgbrlORpSQuJEjQ==;EndpointSuffix=core.windows.net">

type BonusReqType = {
    AdminEmailRecipients : string[];
    Currency : string;
    Amount : decimal;
    Fees : decimal;
    GmUserId : int;
    InvBalIds : int[];
    UserEmail : string;
    UserFirstName : string;
    YearMonthSold : string;
    ProcessedCnt : int;
    CreatedOn: DateTime;
    LastProcessedTime: DateTime;
}

let log = AzStorage.Tables.BonusLog

type BonusLogType = {
    Exn : string option;
    ExnSf : string option;
    GmUserId : int;
    UserEmail : string;
    UserFirstName : string;
    Currency : string;
    Amount : decimal;
    Fees : decimal;
    InvBalIds : string;
    ProcessedCnt : int;
    CreatedOn: DateTime;
    LastProcessedTime: DateTime;
}

let bonusReq2Log(bonusReq : BonusReqType)(exn : exn option) : BonusLogType =
    {
        Exn = exn 
                |> function 
                    | Some exn -> Some exn.Message
                    | _ -> None
        ExnSf = exn 
                |> function 
                    | Some exn -> Some exn.StackTrace
                    | _ -> None
        GmUserId = bonusReq.GmUserId;
        UserEmail = bonusReq.UserEmail;
        UserFirstName = bonusReq.UserFirstName;
        Currency = bonusReq.Currency;
        Amount = bonusReq.Amount;
        Fees = bonusReq.Fees;
        InvBalIds = bonusReq.InvBalIds 
                    |> Array.map(fun i -> i.ToString()) 
                    |> String.concat ",";
        ProcessedCnt = bonusReq.ProcessedCnt;
        CreatedOn = bonusReq.CreatedOn;
        LastProcessedTime = bonusReq.LastProcessedTime;
    }

// let private handleResponse =
//     function
//     | SuccessfulResponse(entityId, errorCode) -> log.Trace(sprintf "Entity %A succeeded: %d." entityId errorCode)
//     | EntityError(entityId, httpCode, errorCode, log) -> log.Error(sprintf "Entity %A failed: %d - %s." entityId httpCode errorCode)
//     | BatchOperationFailedError(entityId, log) -> log.Error(sprintf "Entity %A was ignored as part of a failed batch operation." entityId)
//     | BatchError(entityId, httpCode, errorCode, log) -> log.Error(sprintf "Entity %A failed with an unknown batch error: %d - %s." entityId httpCode errorCode)

let Upsert (excn : exn option)(logEntry: BonusReqType) : BonusReqType=
    let bonusLog = bonusReq2Log logEntry excn
    log.InsertAsync(
        Table.Partition logEntry.YearMonthSold, 
        Table.Row (logEntry.GmUserId.ToString()), 
        bonusLog, 
        Table.TableInsertMode.Upsert)
    |> Async.RunSynchronously
    |> ignore
    //|> handleResponse
    logEntry

type EmailReceipts() =

    let sendUserReceipt(fromGmailUser: string)(fromGmailPwd : string)(userEmail : string)(firstName : string)(amount: string) : unit =

        use smtpClient = new SmtpClient()
        smtpClient.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect)
        smtpClient.Authenticate(fromGmailUser, fromGmailPwd)

        let msg = MimeMessage()
        msg.From.Add(MailboxAddress ("Greenzorro", "help@greenzorro.com"))
        msg.To.Add(MailboxAddress (firstName, userEmail))
        msg.Subject <- sprintf "Greenzorro fullfilled your Bonus request"
        let body = TextPart ("plain")
        body.Text <- 
            sprintf "Dear %s,\n\
                    \n\
                    Further to your request, we would like to inform you that your Investment bonus of %s has been credited to your account!\n\
                    \n\
                    In order to turn your bonus into cash, all that is required is to place a single minimum bet on one of our games.\n\
                    \n\
                    Thank you for choosing Greenzorro and we hope you enjoy it!\n\
                    \n\
                    Best regards,\n\
                    \n\
                    Greenzorro Support Team" <| firstName <| amount
        msg.Body <- body

        smtpClient.Send(msg)
        smtpClient.Disconnect(true)

    let sendAdminReceipt(fromGmailUser: string)(fromGmailPwd : string)(gmUserId : int)(userEmail : string)(amount: string) : unit =

        use smtpClient = new SmtpClient()
        smtpClient.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect)
        smtpClient.Authenticate(fromGmailUser, fromGmailPwd)

        let msg = MimeMessage()
        msg.From.Add(MailboxAddress ("Admin", "admin@greenzorro.com"))
        msg.To.Add(MailboxAddress ("Antonis", "antonis@greenzorro.com"))
        msg.To.Add(MailboxAddress ("Mario", "mario@greenzorro.com"))
        msg.Subject <- sprintf "Bonus fulfilled for id: %d" <| gmUserId
        let body = TextPart ("plain")
        body.Text <- 
            sprintf "Bonus fulfilled for\n\
                    user id: %d\n\
                    email: %s\n\
                    Amount: %s.\n\
                    \n\
                    Cheers\n\
                    \n\
                    Your friendly admin process."<| gmUserId <| userEmail <| amount

        msg.Body <- body

        smtpClient.Send(msg)
        smtpClient.Disconnect(true)

    let getAmountCur (bonusReq : BonusReqType) : string = 
        String.concat " " [Math.Round(bonusReq.Amount, 2).ToString(); bonusReq.Currency]

    let rec retry(triesLeft : int)(fn : (unit -> unit)) =
        try
            fn()
        with ex ->
            if triesLeft > 0 then
                retry (triesLeft - 1) fn
            else
                raise ex

    member this.SendBonusReqUserReceipt(fromGmailUser: string)(fromGmailPwd : string)(bonusReq : BonusReqType) : BonusReqType =

        // Capitalize first letter of first name
        let firstName = 
            List.ofSeq bonusReq.UserFirstName
            |> function
                | h::t -> 
                    Char.ToUpper(h) :: t
                    |> List.toArray
                    |> String
                | _ -> "Player"

        let toCallFunc() = sendUserReceipt fromGmailUser fromGmailPwd bonusReq.UserEmail firstName (getAmountCur bonusReq)
        try
            retry 3 toCallFunc
        with ex ->
            Upsert (Some ex) bonusReq |> ignore
        bonusReq

    member this.SendBonusReqAdminReceipt (fromGmailUser: string)(fromGmailPwd : string)(bonusReq : BonusReqType) : unit =
        
        let toCallFunc() = sendAdminReceipt fromGmailUser fromGmailPwd bonusReq.GmUserId bonusReq.UserEmail (getAmountCur bonusReq)
        try
            retry 3 toCallFunc
        with ex ->
            Upsert (Some ex) bonusReq |> ignore

let Run(inputMessage: string, log: TraceWriter) =
    log.Info(sprintf "F# Queue trigger function processed: '%s'" inputMessage)

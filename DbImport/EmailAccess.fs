namespace DbImport

open System
open System.IO
open GzCommon
open MailKit
open MimeKit
open MailKit.Net.Imap
open NLog

type MessagesCount = { CurrentIndex : int ; TotalReportMessages : int ; FoundDaysReport : bool }

type EmailAccess(dayToProcess : DateTime, destCustomRptName : string, gmailUser: string, gmailPassword : string) =

    static let logger = LogManager.GetCurrentClassLogger()

    let todaysFileTitle : string =
        let dtStr = dayToProcess.ToString("ddMMMyyyy")
        let yyyyMm = dayToProcess.ToYyyyMm
        let filename = yyyyMm + " " + "(daily" + dtStr + ").xlsx"
        filename

    let mimePart2Disk (mimePart : MimePart)(filename : string) : unit =
        use fs = new FileStream(filename, FileMode.Create)
        mimePart.ContentObject.DecodeTo(fs)
        fs.Flush()

    let msgPart2Disk (messagePart : MessagePart)(filename : string) : unit =
        use fs = new FileStream(filename, FileMode.Create)
        messagePart.Message.WriteTo(fs)
        fs.Flush()

    let findAttachment (isFound : bool)(attachment : MimeEntity): bool =
        if attachment.ContentType.Name = todaysFileTitle then
            match attachment with
            | :? MimePart as f -> mimePart2Disk f destCustomRptName // @"d:\sc\gz\inRpt\" + "Custom Prod 20170616.xlsx"
            | :? MessagePart as f -> msgPart2Disk f destCustomRptName
            | _ -> printfn "Unknown attachment type %s %s"  attachment.ContentType.Name attachment.ContentType.MimeType
            true
        else
            isFound // if found once iterating through it sticks

    let saveUsefulAttachment(message : MimeMessage)(yyyyMm : string) : bool = 
        let from = 
            (
                message.From.Mailboxes 
                |> Seq.head
            ).Address
            
        let subject = message.Subject

        if from.EndsWith("everymatrix.com") && todaysFileTitle.LastIndexOf(subject.Substring(subject.Length - 10, 10)) <> -1 then
            
            Seq.fold findAttachment false message.Attachments
            
        else
            false

    let messageMarkedForDeletion (folder : IMailFolder)(messageIdx : int) : unit = 
        folder.AddFlags(messageIdx, MessageFlags.Deleted, true)

    let messageTasks (inbox : IMailFolder)(yyyyMm : string)(index : int) : bool =
        let message = inbox.GetMessage(index)
        
        let foundUsefulAttachment = saveUsefulAttachment message yyyyMm
        if not foundUsefulAttachment then
            messageMarkedForDeletion inbox index
        foundUsefulAttachment

    let initInbox (client : ImapClient) =
        // Accept all certificates
        client.ServerCertificateValidationCallback <- (fun _ _ _ _ -> true)

        let connRes = client.Connect("imap.gmail.com", 993)
        client.AuthenticationMechanisms.Remove ("XOAUTH2") |> ignore
        let loginRes = client.Authenticate (gmailUser,  gmailPassword)
        let inbox = client.Inbox
        ignore ( inbox.Open(FolderAccess.ReadWrite) )
        inbox

    member this.DownloadCustomReport() : MessagesCount =

        use client = new ImapClient()
        let inbox = initInbox client

        let yyyyMm = dayToProcess.ToYyyyMm

        let rec loop (inbox : IMailFolder)(currentCounts : MessagesCount) : MessagesCount = 
            let index = currentCounts.CurrentIndex
            if index = 0 then
                { currentCounts with CurrentIndex = 0 }
            else 
                let foundReport = messageTasks inbox yyyyMm (index - 1)
                let newMsgCount = { 
                    CurrentIndex = index - 1; 
                    TotalReportMessages = currentCounts.TotalReportMessages + 1;
                    FoundDaysReport = currentCounts.FoundDaysReport || foundReport
                }
                loop inbox newMsgCount

        let msgCount = 
            loop inbox { 
                    CurrentIndex = inbox.Count; 
                    TotalReportMessages = 0; 
                    FoundDaysReport = false 
                }
        msgCount
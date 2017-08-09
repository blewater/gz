namespace GzBatchCommon

open System
open System.IO
open MailKit
open MimeKit
open MailKit.Net.Imap
open NLog
open MailKit.Net.Smtp
open MailKit.Security

type MessagesCount = { CurrentIndex : int ; TotalReportMessages : int ; FoundDaysReport : bool }

type EmailAccess(dayToProcess : DateTime, gmailUser: string, gmailPassword : string) =

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

    let saveUsefulAttachment (destCustomRptName : string)(message : MimeMessage)(yyyyMm : string) : bool = 
        let from = 
            (
                message.From.Mailboxes 
                |> Seq.head
            ).Address
            
        let subject = message.Subject

        if from.EndsWith("everymatrix.com") && todaysFileTitle.LastIndexOf(subject.Substring(subject.Length - 10, 10)) <> -1 then
            
            let findAttachment (isFound : bool)(attachment : MimeEntity): bool =
                if attachment.ContentType.Name = todaysFileTitle then
                    match attachment with
                    | :? MimePart as f -> mimePart2Disk f destCustomRptName // @"d:\sc\gz\inRpt\" + "Custom Prod 20170616.xlsx"
                    | :? MessagePart as f -> msgPart2Disk f destCustomRptName
                    | _ -> printfn "Unknown attachment type %s %s"  attachment.ContentType.Name attachment.ContentType.MimeType
                    true
                else
                    isFound // if found once iterating through it sticks

            Seq.fold findAttachment false message.Attachments
            
        else
            false

    let messageMarkedForDeletion (folder : IMailFolder)(messageIdx : int) : unit = 
        folder.AddFlags(messageIdx, MessageFlags.Deleted, true)

    let messageTasks (inbox : IMailFolder)(destCustomRptName : string)(yyyyMm : string)(index : int) : bool =
        let message = inbox.GetMessage(index)
        
        let foundUsefulAttachment = saveUsefulAttachment destCustomRptName message yyyyMm
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

    member this.DownloadCustomReport(destCustomRptName : string) : MessagesCount =

        use client = new ImapClient()
        let inbox = initInbox client

        let yyyyMm = dayToProcess.ToYyyyMm

        let rec loop (inbox : IMailFolder)(currentCounts : MessagesCount) : MessagesCount = 
            let index = currentCounts.CurrentIndex
            if index = 0 then
                { currentCounts with CurrentIndex = 0 }
            else 
                let foundReport = messageTasks inbox destCustomRptName yyyyMm (index - 1)
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

    member this.SendWithdrawnVintagesCashBonusCsv(csvContent : string)(csvFilenamePath : string)(processedDay : DateTime) =

        use smtpClient = new SmtpClient()
        smtpClient.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect)
        smtpClient.Authenticate(gmailUser, gmailPassword)

        let msg = MimeMessage()
        msg.From.Add(MailboxAddress ("Admin", "admin@greenzorro.com"))
        msg.To.Add(new MailboxAddress ("Antonis", "antonis.voerakos@greenzorro.com"))
        msg.To.Add(MailboxAddress ("Mario", "mario.karagiorgas@greenzorro.com"))
        msg.Subject <- sprintf "Withdrawn Vintages Csv for %s" <| processedDay.ToString("ddd d MMM yyyy")
        let body = TextPart ("plain")
        body.Text <- "Please upload this file in Gammatrix.com Banking...Vendors...System....Manual deposit...Process batch"

        // create an image attachment for the file located at path
        let attachment = MimePart ("text", "csv")
        attachment.ContentObject <- ContentObject (File.OpenRead (csvFilenamePath), ContentEncoding.Default)
        attachment.ContentDisposition <- ContentDisposition (ContentDisposition.Attachment)
        attachment.ContentTransferEncoding <- ContentEncoding.Base64
        attachment.FileName <- Path.GetFileName (csvFilenamePath)

        // now create the multipart/mixed container to hold the message text and the
        // csv attachment
        let multipart = Multipart ("mixed")
        multipart.Add (body)
        multipart.Add (attachment)

        // now set the multipart/mixed as the message body
        msg.Body <- multipart

        smtpClient.Send(msg)
        smtpClient.Disconnect(true)

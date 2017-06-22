open System
open NUnit.Framework
open FsUnit
open System.IO
open FSharp.Configuration
open FsUnitTyped.TopLevelOperators
open MailKit
open MimeKit
open MailKit.Net.Imap

type Settings = AppSettings< "app.config" >

type MessagesCount = { CurrentIndex : int ; TotalReportMessages : int }

[<TestFixture; 
    Description("Test gmail inbox reading")>]
type Test() =

    let getExcelFilenameDateStr(dateToProcess : DateTime) : string =
        let dtStr = dateToProcess.ToString("ddMMMyyyy")
        let yyyyMm = dateToProcess.ToString("yyyyMM")
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

    let todaysFileTitle = getExcelFilenameDateStr <| DateTime.Today.AddDays(-1.0)

    let findAttachment (isFound : bool)(attachment : MimeEntity): bool =
        if attachment.ContentType.Name = todaysFileTitle then
            match attachment with
            | :? MimePart as f -> mimePart2Disk f (@"d:\sc\gz\inRpt\" + f.FileName) // @"d:\sc\gz\inRpt\" + "Custom Prod by Email.xlsx"
            | :? MessagePart as f -> msgPart2Disk f (@"d:\sc\gz\inRpt\" + "Custom Prod by Email.xlsx")
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

    let messageTasks (inbox : IMailFolder)(yyyyMm : string)(index : int) : int =
        let message = inbox.GetMessage(index)
        
        let foundUsefulAttachment = saveUsefulAttachment message yyyyMm
        match foundUsefulAttachment with
        | false -> messageMarkedForDeletion inbox index; 0
        | true -> 1

    let processedMessages(inbox : IMailFolder) : MessagesCount =

        let dateToProcess = DateTime.Today.AddDays(-1.0)
        let yyyyMm = dateToProcess.Year.ToString("0000") + dateToProcess.Month.ToString("00")
        let dayToProcess = yyyyMm + dateToProcess.Day.ToString("00")

        let rec loop (inbox : IMailFolder)(currentCounts : MessagesCount) : MessagesCount = 
            let count = inbox.Count
            let index = currentCounts.CurrentIndex
            if index = 0 then
                { currentCounts with CurrentIndex = 0 }
            else 
                let newMsgCount = { 
                    CurrentIndex = index - 1; 
                    TotalReportMessages = currentCounts.TotalReportMessages + messageTasks inbox yyyyMm (index - 1)
                }
                loop inbox newMsgCount

        let msgCount = loop inbox { CurrentIndex = inbox.Count; TotalReportMessages = 0 }
        msgCount

    [<Test; Description("Read message(s) from hostmaster@greenzorro.com")>]
    member x.ReadImapGmailByMailKit () =
        use client = new ImapClient()
        // Accept all certificates
        client.ServerCertificateValidationCallback <- (fun _ _ _ _ -> true)

        let connRes = client.Connect("imap.gmail.com", 993)
        client.AuthenticationMechanisms.Remove ("XOAUTH2") |> ignore
        let loginRes = client.Authenticate ("hostmaster@greenzorro.com", "c9X6&jSBq%FS!$&roa^8")
        let inbox = client.Inbox
        inbox.Open(FolderAccess.ReadWrite) |> ignore

        let msgIndex = processedMessages inbox

        msgIndex.CurrentIndex |> shouldEqual 0
        inbox.Expunge()
        inbox.Count |> shouldEqual 1

[<EntryPoint>]
let main argv = 
    let t = Test()
    //do t.ReadImapGmail()
    0 // return an integer exit code

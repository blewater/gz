open System
open NUnit.Framework
open FsUnit
open OpenPop.Pop3
open System.IO
open ActiveUp.Net.Mail

[<TestFixture; 
    Description("Test gmail inbox reading")>]
type Test() =
    [<Test; Description("Read message(s) from hostmaster@greenzorro.com")>]
    member x.ReadImapGmail () =
        let gmReader = new Imap4Client()
        let connRes = gmReader.ConnectSsl("imap.gmail.com", 993)
        let loginRes = gmReader.Login("hostmaster@greenzorro.com", "c9X6&jSBq%FS!$&roa^8")
        let mailbox = gmReader.SelectMailbox("Inbox")
        let messages = mailbox.SearchParse("UNSEEN")
        let msgCount = messages.Count
        let mutable totalAttachmentsFound = 0
        for email in messages do
            let from = email.From
            let subject = email.Subject

            if from.Email.EndsWith("everymatrix.com") && subject.StartsWith("Losses") && email.Attachments.Count > 0 then
                for attachment in email.Attachments do
                    printfn "Attachment %s of type %s" attachment.ContentName attachment.ContentType.MimeType
                    if attachment.Filename.StartsWith "Losses" 
                            && not <| File.Exists(@"d:\sc\gz\inRpt\" + attachment.Filename) then
                        printfn "Saving attachment %s" attachment.Filename
                        File.WriteAllBytes("d:/sc/gz/inRpt/"+attachment.Filename, attachment.BinaryContent)
                        totalAttachmentsFound <- totalAttachmentsFound + 1
        totalAttachmentsFound |> should greaterThanOrEqualTo 0

//    member x.ReadPopGmail () =
//        let gmPopReader = new Pop3Client()
//        let connRes = gmPopReader.Connect("pop.gmail.com", 995, true)
//        gmPopReader.Authenticate "hostmaster@greenzorro.com" "c9X6&jSBq%FS!$&roa^8"
//        let msgCount = gmPopReader.GetMessageCount()
//        let mutable found = 0
//        for i in 1..msgCount do
//            let headers = gmPopReader.GetMessageHeaders(i)
//            let from = headers.From
//            let subject = headers.Subject
//
//            if from.HasValidMailAddress 
//            && from.Address.EndsWith("@everymatrix.com") && subject.Equals("Losses") then
//                for attachment in gmPopReader.GetMessage(i).FindAllAttachments() do
//                    if attachment.FileName.StartsWith "Losses" then
//                        printfn "Saving attachment %s" attachment.FileName
//                        File.WriteAllBytes("d:/sc/gz/inRpt/"+attachment.FileName, attachment.Body)
//                        found <- found + 1
//        found |> should greaterThanOrEqualTo 1

[<EntryPoint>]
let main argv = 
    0 // return an integer exit code

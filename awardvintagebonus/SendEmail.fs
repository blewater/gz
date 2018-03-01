#if INTERACTIVE
#r "./packages/MailKit/lib/net45/MailKit.dll"
#r "./packages/MimeKit/lib/net45/MimeKit.dll"
#else
module SendEmail
#endif

open MimeKit
open MailKit.Net.Smtp
open MailKit.Security
open BonusReq
open System

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
            TblLogger.Upsert (Some ex) bonusReq |> ignore
        bonusReq

    member this.SendBonusReqAdminReceipt (fromGmailUser: string)(fromGmailPwd : string)(bonusReq : BonusReqType) : unit =
        
        let toCallFunc() = sendAdminReceipt fromGmailUser fromGmailPwd bonusReq.GmUserId bonusReq.UserEmail (getAmountCur bonusReq)
        try
            retry 3 toCallFunc
        with ex ->
            TblLogger.Upsert (Some ex) bonusReq |> ignore
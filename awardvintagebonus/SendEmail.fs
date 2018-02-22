#if INTERACTIVE
#r "./packages/MailKit/lib/net45/MailKit.dll"
#r "./packages/MimeKit/lib/net45/MimeKit.dll"
#r "./packages/NLog/lib/net45/NLog.dll"
#else
module SendEmail
#endif

open MimeKit
open NLog
open MailKit.Net.Smtp
open MailKit.Security
open BonusReq
open System

type EmailReceipts() =

    static let logger = LogManager.GetCurrentClassLogger()

    let sendUserReceipt(fromGmailUser: string)(fromGmailPwd : string)(userEmail : string)(firstName : string)(amount: string) =

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
                    In order to turn your bonus into cash, all that is required is to place a single minimum bet on one of our games (even 0,10 EUR).\n\
                    \n\
                    Thank you for choosing Greenzorro and we wish you a wonderful day and all the best for the new year!\n\
                    \n\
                    Best regards,\n\
                    \n\
                    Greenzorro Support Team" <| firstName <| amount
        msg.Body <- body

        smtpClient.Send(msg)
        smtpClient.Disconnect(true)

    member this.SendBonusReqAdminReceipt(fromGmailUser: string)(fromGmailPwd : string)(gmUserId : int)(userEmail : string)(amount: string) =

        use smtpClient = new SmtpClient()
        smtpClient.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect)
        smtpClient.Authenticate(fromGmailUser, fromGmailPwd)

        let msg = MimeMessage()
        msg.From.Add(MailboxAddress ("Admin", "admin@greenzorro.com"))
        //msg.To.Add(MailboxAddress ("Antonis", "antonis@greenzorro.com"))
        msg.To.Add(MailboxAddress ("Mario", "mario@greenzorro.com"))
        msg.Subject <- sprintf "Bonus fulfilled for %s" <| userEmail
        let body = TextPart ("plain")
        body.Text <- 
            sprintf "Bonus fulfilled for\n\
                    user id:%d\n\
                    email: %s\n\
                    Amount: %s.\n\
                    \n\
                    Cheers\n\
                    \n\
                    Your friendly admin process."<| gmUserId <| userEmail <| amount

        msg.Body <- body

        smtpClient.Send(msg)
        smtpClient.Disconnect(true)

    member this.SendBonusReqUserReceipt(fromGmailUser: string)(fromGmailPwd : string)(bonusReq : BonusReqType) =
        let firstName = 
            List.ofSeq bonusReq.UserFirstName
            |> function
                | h::t -> 
                    Char.ToUpper(h) :: t
                    |> List.toArray
                    |> String
                | _ -> "Player"
        let amount = bonusReq.Amount.ToString()
            //String.concat ([bonusReq.Amount.ToString(), " ", bonusReq.
        sendUserReceipt fromGmailUser fromGmailPwd bonusReq.UserEmail firstName amount
        bonusReq
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimeKit;
using System;
using System.Configuration;
using System.IO;

namespace gzWeb.Tests.Helpers
{
    [TestClass]
    public class EmailSender
    {
        [TestMethod]
        public void SendUserWithdrawalReq()
        {
            var userEmail = "salem8@gmail.com";
            var everymatrixUserId = 4300962;
            var currency = "EUR";
            var amount = "1";
            var username = "ladderman";
            var comment = string.Format("vintage month 201706 cash for {0}", username);

            using (var depositsInMem = new StringWriter()) {
                var depLine = string.Format($"{everymatrixUserId};CasinoWallet;{currency};{amount};{comment}");
                depositsInMem.WriteLine(depLine);
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Admin Greenzorro", "admin@greenzorro.com"));
            message.To.Add(new MailboxAddress("Mario", "salem8@gmail.com"));
            message.To.Add(new MailboxAddress("Mario", "mario.karagiorgas@greenzorro.com"));
            message.Subject =$@"Withdrawn Vintage Csv for user id {everymatrixUserId} on {DateTime.UtcNow.ToString("ddd d MMM yyyy")}";

            var builder = new BodyBuilder
            {
//Please 
//upload the attached file in 
//Gammatrix Banking...Vendors...System....Manual deposit...Process batch
//the attached file 

//-- Or

TextBody = $@"User with userName {username}, email {userEmail} withdrew vintage(s)
Please go to
Gammatrix Banking...Vendors...System....Manual deposit

Enter

CasinoWallet - Bonus
To userID {everymatrixUserId}
Amount {amount} {currency}

Thanks
Your friendly neighborhood admin"
            };

            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);

                var gmailUser = ConfigurationManager.AppSettings["gmailUser"];
                var gmailPwd = ConfigurationManager.AppSettings["gmailPwd"];
                client.Authenticate(gmailUser, gmailPwd);

                var options = FormatOptions.Default.Clone();

                if (client.Capabilities.HasFlag(SmtpCapabilities.UTF8))
                    options.International = true;

                client.Send(options, message);

                client.Disconnect(true);
            }
        }
    }
}

using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace gzDAL.Repos.Interfaces
{
    public interface IEmailService
    {
        Task SendEmail(string templateCode, MailAddress fromAddress, string toAddress, Dictionary<string, object> data);
    }
}
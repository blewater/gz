using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace gzDAL.Repos
{
    public interface IEmailService
    {
        Task SendEmail(string templateCode, MailAddress fromAddress, string toAddress, Dictionary<object, object> data);
    }
}
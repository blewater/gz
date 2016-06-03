using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using gzDAL.Models;
using gzDAL.Repos;

namespace gzWeb.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        HomeController(IEmailService emailService)
        {
            
        }
        // GET: Admin/Home
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> SendTestEmail()
        {
            var dataObj = new Dictionary<object, object>
                          {
                                  {"FirstName", "Dinos"},
                                  {"LastName", "Chatzopoulos"}
                          };

            await _emailService.SendEmail("testEmail", new MailAddress("xdinos@gmail.com", "from me"), "xdinos@gmail.com", dataObj);

            return RedirectToAction("Index");
        }

        private readonly IEmailService _emailService;
    }
}
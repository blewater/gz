using gzDAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace gzWeb.Areas.Admin.Controllers
{
    public class LogsController : Controller
    {
        // GET: Admin/Logs
        public ActionResult Index()
        {
            List<LogEntry> model = null;

            return View("Index", model);
        }
    }
}
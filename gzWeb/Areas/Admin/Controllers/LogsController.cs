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
        private readonly ApplicationDbContext _dbContext;

        public LogsController()
        {
            // TODO: (xdinos) inject
            _dbContext = new ApplicationDbContext();
        }

        // GET: Admin/Logs
        public ActionResult Index()
        {
            List<LogEntry> model = _dbContext.LogEntries.Take(30).ToList();

            return View("Index", model);
        }
    }
}
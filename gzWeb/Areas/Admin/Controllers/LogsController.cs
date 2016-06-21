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
        public ActionResult Index(int page = 1, int pageSize = 20, string logLevel = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            IQueryable<LogEntry> logEntries = _dbContext.LogEntries;

            if (!String.IsNullOrEmpty(logLevel))
                logEntries = logEntries.Where(x => x.Level == logLevel);
            if (fromDate.HasValue)
                logEntries = logEntries.Where(x => x.Logged >= fromDate.Value);
            if (toDate.HasValue)
                logEntries = logEntries.Where(x => x.Logged <= toDate.Value);

            return View("Index", logEntries.Skip(pageSize*(page - 1)).Take(pageSize));
        }
    }
}
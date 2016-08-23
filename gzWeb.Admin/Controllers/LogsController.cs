using System;
using System.Linq;
using System.Web.Mvc;
using gzDAL.Models;
using gzWeb.Admin.Models;

namespace gzWeb.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
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

            var now = DateTime.Now;
            if (!fromDate.HasValue)
                fromDate = now.AddMonths(-1);

            if (!toDate.HasValue)
                toDate = fromDate.Value.AddMonths(1);

            logEntries = logEntries.Where(x => x.Logged >= fromDate.Value && x.Logged <= toDate.Value);
            var totalPages = (int)Math.Ceiling((float)logEntries.Count() / pageSize);

            return View("Index", new LogViewModel
            {
                CurrentPage = page,
                TotalPages = totalPages,
                LogLevel = logLevel,
                FromDate = fromDate.Value,
                ToDate = toDate.Value,
                LogEntries = logEntries.OrderByDescending(x => x.Logged).Skip(pageSize * (page - 1)).Take(pageSize).ToList()
            });
        }

        public PartialViewResult GetEntry(int id)
        {
            return PartialView("_LogEntryModal", _dbContext.LogEntries.Single(x => x.Id == id));
        }
    }
}
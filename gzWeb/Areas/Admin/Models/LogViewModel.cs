using gzDAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzWeb.Areas.Admin.Models
{
    public class LogViewModel
    {        
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string LogLevel { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<LogEntry> LogEntries { get; set; }
    }
}
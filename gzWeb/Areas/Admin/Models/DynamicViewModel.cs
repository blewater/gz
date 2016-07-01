using gzDAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzWeb.Areas.Admin.Models
{
    public class DynamicViewModel
    {        
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public List<DynamicPage> DynamicEntries { get; set; }
    }
}
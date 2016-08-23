using System.Collections.Generic;
using gzDAL.Models;

namespace gzWeb.Admin.Models
{
    public class EmailsViewModel
    {        
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public List<EmailTemplate> EmailsEntries { get; set; }
    }
}
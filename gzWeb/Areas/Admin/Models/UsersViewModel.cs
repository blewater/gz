using gzDAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace gzWeb.Areas.Admin.Models
{
    public class UsersViewModel
    {        
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; }
        public List<ApplicationUser> UsersEntries { get; set; }
    }

    public class UserViewModel
    {
        public ApplicationUser User { get; set; }
        public List<CustomRole> Roles { get; set; }
        public List<CustomRole> RolesOfUser { get; set; }
    }

    public class RoleViewModel
    {
        public CustomRole Role { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public List<ApplicationUser> UsersOfRole { get; set; }
    }
}
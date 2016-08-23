using System.Collections.Generic;
using gzDAL.Models;

namespace gzWeb.Admin.Models
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

    public class UserViewPostModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string GmCustomerId { get; set; }
        public List<int> RolesOfUser { get; set; }
    }

    public class RoleViewModel
    {
        public CustomRole Role { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public List<ApplicationUser> UsersOfRole { get; set; }
    }
}
using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

//namespace gzWeb.Models
//{
//    public partial class ApplicationUser : IdentityUser
//    {
//        public virtual CustomerProfile CustomerProfile { get; set; }
//    }

//    public class CustomerProfile
//    {
//        public int Id { get; set; }
//        public string FirstName { get; set; }
//        public string LastName { get; set; }
//        public DateTime Birthday { get; set; }
//        public DateTime? DateCreated { get; set; }
//        public DateTime? DateUpdated { get; set; }
//    }

//    public class ApplicationDbObject : IdentityDbContext<ApplicationUser>
//    {
//        public ApplicationDbObject()
//            : base("DefaultConnection")
//        { }

//        public DbSet<CustomerProfile> CustomerProfile { get; set; }
//    }
//}
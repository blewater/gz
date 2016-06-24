using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gzDAL.Conf;
using gzDAL.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using gzWeb.Areas.Admin.Models;

namespace gzWeb.Areas.Admin.Controllers
{
    public class ManageController : Controller
    {
        private ApplicationUserManager _userManager;
        private readonly ApplicationDbContext _dbContext;

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ManageController()
        {
            // TODO: (xdinos) inject
            _dbContext = new ApplicationDbContext();
        }

        // GET: Admin/Users
        public ActionResult Users(int page = 1, int pageSize = 20, string searchTerm = null)
        {
            var query = _dbContext.Users;
            if (!String.IsNullOrEmpty(searchTerm))
                query.Where(x => x.UserName == searchTerm || x.FirstName == searchTerm || x.LastName == searchTerm);

            var totalPages = (int)Math.Ceiling((float)query.Count() / pageSize);

            return View(new UsersViewModel
            {
                CurrentPage=page,
                TotalPages = totalPages,
                SearchTerm = searchTerm,
                UsersEntries = query.OrderBy(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize).ToList()
            });
        }


        //[HttpPost]
        //public ActionResult UserAddToRole(int userId, int roleId)
        //{
        //    var role = _dbContext.Roles.SingleOrDefault(x => x.Id == roleId);

        //    if (role)
        //    UserManager.AddToRole()
        //}

        #region Roles

        // GET: Admin/Users
        public ActionResult Roles()
        {
            return View(_dbContext.Roles.ToList());
        }

        public ActionResult RoleCreate()
        {
            return View(new CustomRole());
        }

        [HttpPost]
        public ActionResult RoleCreate(CustomRole model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _dbContext.Roles.Add(model);
            _dbContext.SaveChanges();

            return RedirectToAction("Roles", "Manage", new { Area = "Admin" });
        }

        public ActionResult RoleEdit(int id)
        {
            return View(_dbContext.Roles.Single(x => x.Id == id));
        }
        
        [HttpPost]
        public ActionResult RoleEdit(CustomRole model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _dbContext.Roles.AddOrUpdate(model);
            _dbContext.SaveChanges();

            return RedirectToAction("Roles", "Manage", new { Area = "Admin" });
        }

        public ActionResult RoleDetails(int id)
        {
            return View(_dbContext.Roles.Single(x => x.Id == id));
        }

        #endregion
    }
}
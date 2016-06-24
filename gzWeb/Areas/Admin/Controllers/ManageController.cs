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
        public ActionResult Users()
        {
            return View(_dbContext.Users.ToList());
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
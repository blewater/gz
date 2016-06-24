using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gzDAL.Models;

namespace gzWeb.Areas.Admin.Controllers
{
    public class ManageController : Controller
    {

        private readonly ApplicationDbContext _dbContext;

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

            return RedirectToAction("Roles", "Manage", new { Area = "Admin" });
        }

        public ActionResult RoleDetails(int id)
        {
            return View(_dbContext.Roles.Single(x => x.Id == id));
        }
    }
}
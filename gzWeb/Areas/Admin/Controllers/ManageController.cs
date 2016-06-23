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
    }
}
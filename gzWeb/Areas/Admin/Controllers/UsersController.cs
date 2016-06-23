using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gzDAL.Models;

namespace gzWeb.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {

        private readonly ApplicationDbContext _dbContext;

        public UsersController()
        {
            // TODO: (xdinos) inject
            _dbContext = new ApplicationDbContext();
        }

        // GET: Admin/Users
        public ActionResult Index()
        {
            return View(_dbContext.Users.ToList());
        }
    }
}
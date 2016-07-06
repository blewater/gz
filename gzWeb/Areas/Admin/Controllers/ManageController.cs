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
    [Authorize(Roles = "Administrator")]
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
            var query = _dbContext.Users.AsQueryable();
            if (!String.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.UserName.Contains(searchTerm) ||
                                         x.FirstName.Contains(searchTerm) ||
                                         x.LastName.Contains(searchTerm) ||
                                         x.Email.Contains(searchTerm));
            }

            var totalPages = (int)Math.Ceiling((float)query.Count() / pageSize);

            return View(new UsersViewModel
            {
                CurrentPage=page,
                TotalPages = totalPages,
                SearchTerm = searchTerm,
                UsersEntries = query.OrderBy(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize).ToList()
            });
        }

        public ActionResult UserEdit(int id)
        {
            var user = _dbContext.Users.Single(x => x.Id == id);
            var roles = _dbContext.Roles.ToList();
            var rolesOfUser = roles.Where(x => x.Users.Any(r => r.UserId == user.Id)).ToList();

            roles.RemoveAll(x => rolesOfUser.Any(r => r.Id == x.Id));

            return View(new UserViewModel
                        {
                                User = user,
                                Roles = roles,
                                RolesOfUser = rolesOfUser
                        });
        }

        [HttpPost]
        public ActionResult UserEdit(UserViewPostModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("UserEdit", "Manage", new {Area = "Admin", id = model.Id});

            var user = _dbContext.Users.Single(x => x.Id == model.Id);
            var rolesOfUser = user.Roles.ToList();
            foreach (var userRole in rolesOfUser)
                user.Roles.Remove(userRole);

            foreach (var roleId in model.RolesOfUser)
                user.Roles.Add(new CustomUserRole {RoleId = roleId, UserId = user.Id});

            _dbContext.Users.AddOrUpdate(user);
            _dbContext.SaveChanges();

            return RedirectToAction("Users", "Manage", new { Area = "Admin" });
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
            var role = _dbContext.Roles.Single(x => x.Id == id);
            var users = _dbContext.Users.ToList();
            var usersOfRole = users.Where(x => x.Roles.Any(r => r.RoleId == role.Id)).ToList();
            return View(new RoleViewModel
            {
                Role = role,
                Users = users,
                UsersOfRole = usersOfRole
            });
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
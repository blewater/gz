using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gzDAL.Models;

namespace gzWeb.Areas.Admin.Controllers
{
    public class EmailTemplatesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public EmailTemplatesController()
        {
            // TODO: (xdinos) inject
            _dbContext = new ApplicationDbContext();
        }

        // GET: Admin/EmailTemplates
        public ActionResult Index()
        {
            return View(_dbContext.EmailTemplates.ToList());
        }

        
        public ActionResult Create()
        {
            return View(new EmailTemplate());
        }

        [HttpPost]
        public ActionResult Create(EmailTemplate model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _dbContext.EmailTemplates.Add(model);
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "EmailTemplates", new {Area = "Admin"});
        }

        public ActionResult Details(int id)
        {
            return View(_dbContext.EmailTemplates.Single(x => x.Id == id));
        }

        public ActionResult Edit(int id)
        {
            return View(_dbContext.EmailTemplates.Single(x => x.Id == id));
        }

        [HttpPost]
        public ActionResult Edit(EmailTemplate model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _dbContext.EmailTemplates.AddOrUpdate(model, _dbContext);
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "EmailTemplates", new { Area = "Admin" });
        }
    }
}
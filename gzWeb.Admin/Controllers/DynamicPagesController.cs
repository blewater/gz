using System;
using System.Linq;
using System.Web.Mvc;
using gzDAL.Models;
using gzWeb.Admin.Models;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace gzWeb.Admin.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class DynamicPagesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public DynamicPagesController()
        {
            // TODO: (xdinos) inject
            _dbContext = new ApplicationDbContext();
        }

        // GET: Admin/EmailTemplates
        public ActionResult Index(int page = 1, int pageSize = 20, string searchTerm = null)
        {
            var query = _dbContext.DynamicPages.AsQueryable();
            if (!String.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(x => x.Code.Contains(searchTerm));
            }

            var totalPages = (int)Math.Ceiling((float)query.Count() / pageSize);
            return View("Index", new DynamicViewModel
                                 {
                                         CurrentPage = page,
                                         TotalPages = totalPages,
                                         SearchTerm = searchTerm,
                                         DynamicEntries =
                                                 query.OrderBy(x => x.Id)
                                                      .Skip((page - 1)*pageSize)
                                                      .Take(pageSize)
                                                      .ToList()
                                 });
        }

        public ActionResult Create()
        {
            return View(new DynamicPage());
        }

        [HttpPost]
        public ActionResult Create(DynamicPage model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _dbContext.DynamicPages.Add(model);
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "DynamicPages");
        }

        public ActionResult Details(int id)
        {
            return View(_dbContext.DynamicPages.Single(x => x.Id == id));
        }

        public ActionResult Edit(int id)
        {
            var model = _dbContext.DynamicPages.Single(x => x.Id == id);
            //if (!String.IsNullOrEmpty(jsonData))
            //{
            //    ViewBag.JsonData = jsonData;
            //    var objData = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonData);
            //    using (var service = RazorEngineService.Create(new TemplateServiceConfiguration()))
            //        ViewBag.EmailTemplate = service.RunCompile(model.Body, "body", null, objData);
            //}
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(DynamicPage model, string jsonData)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _dbContext.DynamicPages.AddOrUpdate(model, _dbContext);
            _dbContext.SaveChanges();

            if (!String.IsNullOrEmpty(jsonData))
            {
                ViewBag.JsonData = jsonData;
                var objData = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonData);
                using (var service = RazorEngineService.Create(new TemplateServiceConfiguration()))
                    ViewBag.DynamicPage = service.RunCompile(model.Code, "code", null, objData);
            }
            return View(model);

            ////return RedirectToAction("Index", "EmailTemplates", new { Area = "Admin" });
            //return RedirectToAction("Edit", "EmailTemplates", new {Area = "Admin", id = model.Id, jsonData = jsonData });
        }
    }
}
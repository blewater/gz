using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gzDAL.Models;
using gzWeb.Areas.Admin.Models;

namespace gzWeb.Areas.Admin.Controllers
{
    public class CarouselEntriesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public CarouselEntriesController()
        {
            // TODO: (xdinos) inject
            _dbContext = new ApplicationDbContext();
        }

        // GET: Admin/CarouselEntries
        public ActionResult Index(int page = 1, int pageSize = 20, string searchTerm = null)
        {
            var query = _dbContext.CarouselEntries.AsQueryable();
            if (!String.IsNullOrEmpty(searchTerm))
                query = query.Where(x => x.Code.Contains(searchTerm));

            var totalPages = (int)Math.Ceiling((float)query.Count() / pageSize);
            return View("Index", new CarouselEntriesViewModel()
                                 {
                                         CurrentPage = page,
                                         TotalPages = totalPages,
                                         SearchTerm = searchTerm,
                                         Entries = query.OrderBy(x => x.Id)
                                                        .Skip((page - 1)*pageSize)
                                                        .Take(pageSize)
                                                        .ToList()
                                 });
        }

        public ActionResult Create()
        {
            return View(new CarouselEntry());
        }

        [HttpPost]
        public ActionResult Create(CarouselEntry model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _dbContext.CarouselEntries.Add(model);
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "CarouselEntries", new { Area = "Admin" });
        }

        public ActionResult Details(int id)
        {
            return View(_dbContext.CarouselEntries.Single(x => x.Id == id));
        }

        public ActionResult Edit(int id)
        {
            return View(_dbContext.CarouselEntries.Single(x => x.Id == id));
        }

        [HttpPost]
        public ActionResult Edit(CarouselEntry model)
        {
            if (!ModelState.IsValid)
                return View(model);

            _dbContext.CarouselEntries.AddOrUpdate(model, _dbContext);
            _dbContext.SaveChanges();

            return RedirectToAction("Index", "CarouselEntries", new { Area = "Admin" });
        }
    }
}
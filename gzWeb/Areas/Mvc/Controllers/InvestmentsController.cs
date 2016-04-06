using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using gzWeb.Areas.Mvc.Models;
using gzDAL.Models;
using gzWeb.Models;
using gzWeb.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using gzDAL.DTO;
using gzDAL.Conf;

namespace gzWeb.Areas.Mvc.Controllers
{
    // [Authorize]
    public class InvestmentsController : Controller
    {
        public ActionResult Summary() { return View(); }
        public ActionResult Portfolio() { return View(); }
        public ActionResult Performance() { return View(); } 
        public ActionResult Activity() { return View(); } 
        



        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Investments
        [Authorize]
        public ActionResult Index()
        {
            db.Database.Log = new DebugTextWriter().Write;

            var manager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var customer = manager.FindById(User.Identity.GetUserId<int>());

            var bal = customer.InvBalance;

            var customerDto = new CustomerDTO();
            Mapper.Map<ApplicationUser, CustomerDTO>(customer, customerDto);
            return View(customerDto);
        }

        // GET: Investments ajax
        public JsonResult GetInvestAmnt() {
            var manager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var customer = manager.FindById(User.Identity.GetUserId<int>());

            var investments = customer
                .GzTransactions
                .Where(t => t.Type.Code == TransferTypeEnum.CreditedPlayingLoss)
                .OrderByDescending(t => t.CreatedOnUTC)
                .Select(t => new { t.Amount, t.CreatedOnUTC })
                .ToList();

            return Json(investments, JsonRequestBehavior.AllowGet);
        }

        // GET: Investments/Create
        //public ActionResult Create()
        //{
        //    ViewBag.Id = new SelectList(db.InvBalances, "CustomerId", "CustomerId");
        //    return View();
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

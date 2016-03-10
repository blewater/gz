using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using gzWeb.Areas.Mvc.Models;
using gzWeb.Models;
using gzWeb.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace gzWeb.Areas.Mvc.Controllers
{
    // [Authorize]
    public class InvestmentsController : Controller
    {
        #region Summary
        public ActionResult Summary()
        {
            var now = DateTime.Now;
            var model = new SummaryViewModel()
            {
                Currency = "€",
                Culture = "en-US",
                InvestmentsBalance = 15000,
                TotalDeposits = 10000,
                TotalWithdrawals = 30000,
                GamingBalance = 4000,
                TotalInvestments = 14000,
                TotalInvestmentsReturns = 1000,
                NextInvestmentOn = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)),
                LastInvestmentAmount = 1000
            };
            return View(model);
        }
        public JsonResult GetVintages(int userId)
        {
            return Json(Utilities.Response.Try(
                () =>
                {
                    var vintagesViewModel = new
                    {
                        Currency = "€",
                        Culture = "en-US",
                        Vintages = new []
                        {
                            new { Date = new DateTime(2014, 7, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2014, 8, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2014, 9, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2014, 10, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2014, 11, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2014, 12, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2015, 1, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2015, 2, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2015, 3, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2015, 4, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2015, 5, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2015, 6, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2015, 7, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2015, 8, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2015, 9, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2015, 10, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2015, 11, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2015, 12, 1), InvestAmount = 100M, ReturnPercent = 10 },
                            new { Date = new DateTime(2016, 1, 1), InvestAmount = 200M, ReturnPercent = -5 },
                            new { Date = new DateTime(2016, 2, 1), InvestAmount = 80M, ReturnPercent = 15 },
                            new { Date = new DateTime(2016, 3, 1), InvestAmount = 150M, ReturnPercent = 10 },
                        }
                    };
                    var result = vintagesViewModel.Vintages
                        .OrderByDescending(x => x.Date.Year)
                        .ThenByDescending(x => x.Date.Month)
                        .Select(x =>
                            new {
                                Year = x.Date.Year,
                                Month = new CultureInfo(vintagesViewModel.Culture).DateTimeFormat.GetMonthName(x.Date.Month),
                                InvestAmount = x.InvestAmount.ToMoneyString(vintagesViewModel.Currency),
                                ReturnPercent = String.Format("{0}{1}%", x.ReturnPercent > 0 ? "+" : (x.ReturnPercent < 0 ? "-" : String.Empty), x.ReturnPercent)
                            });
                    return result;
                }
            ));
        }
        #endregion


        #region Portfolio
        public ActionResult Portfolio()
        {
            return View();
        }
        #endregion

        #region Performance
        public ActionResult Performance()
        {
            return View();
        } 
        #endregion

        #region Activity
        public ActionResult Activity()
        {
            return View();
        } 
        #endregion
        



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

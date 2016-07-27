using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using gzDAL.Models;
using gzDAL.Repos;
using Microsoft.AspNet.Identity;

namespace gzWeb.Areas.Mvc.Controllers
{
    public class InvestmentsController : Controller
    {
        // TODO: Only for performance tests should be removed.
        public async Task<ActionResult> TestSummary() {

            using (var db = new ApplicationDbContext()) {
                var gzTrxRepo = new GzTransactionRepo(db);
                var custPortRepo = new CustPortfolioRepo(db);
                var userRepo = new UserRepo(db, gzTrxRepo,
                    new InvBalanceRepo(db, new CustFundShareRepo(db, custPortRepo), gzTrxRepo, custPortRepo));

                var userId = await db.Users.Where(u => u.Email == "salem8@gmail.com")
                    .Select(u => u.Id)
                    .SingleAsync();
                    
                var summaryRes = await userRepo.GetSummaryDataAsync(userId);
                var summaryDto = summaryRes.Item1;

                return View(summaryDto);
            }
        }
        public PartialViewResult Summary() { return PartialView(); }
        public PartialViewResult Portfolio() { return PartialView(); }
        public PartialViewResult Performance() { return PartialView(); } 
        public PartialViewResult Activity() { return PartialView(); } 
    }
}

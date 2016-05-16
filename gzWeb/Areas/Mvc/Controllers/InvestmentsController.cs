using System.Web.Mvc;

namespace gzWeb.Areas.Mvc.Controllers
{
    // [Authorize]
    public class InvestmentsController : Controller
    {
        public ActionResult Summary() { return View(); }
        public ActionResult Portfolio() { return View(); }
        public ActionResult Performance() { return View(); } 
        public ActionResult Activity() { return View(); } 
    }
}

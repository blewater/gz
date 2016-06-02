using System.Web.Mvc;

namespace gzWeb.Areas.Mvc.Controllers
{
    public class GamesController : Controller
    {
        public ActionResult Games()
        {
            return View();
        }
        public ActionResult Game()
        {
            return View();
        }
    }
}

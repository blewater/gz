using System.Web.Mvc;

namespace gzWeb.Areas.Mvc.Controllers
{
    public class GamesController : Controller
    {
        public PartialViewResult Games()
        {
            return PartialView();
        }
        public PartialViewResult Game()
        {
            return PartialView();
        }
    }
}

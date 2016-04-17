using System.Web.Mvc;

namespace gzWeb.Areas.Mvc.Controllers
{
    public class AuthController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }

        public ActionResult Activate()
        {
            return View();
        }

        public PartialViewResult RegisterForm()
        {
            return PartialView("RegisterForm");
        }

        public PartialViewResult RegisterProfile()
        {
            return PartialView("RegisterProfile");
        }

        public PartialViewResult RegisterDetails()
        {
            return PartialView("RegisterDetails");
        }
    }
}

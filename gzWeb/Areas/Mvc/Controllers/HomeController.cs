using System;
using System.IO;
using System.Web.Mvc;
using gzWeb.Areas.Mvc.Models;

namespace gzWeb.Areas.Mvc.Controllers
{
    public class HomeController : Controller
    {
        #region Actions
        public ActionResult Index()
        {
            return View();
        }

        #region Shared
        public PartialViewResult Header()
        {
            var showFullName = false;
            var first = "Manolis";
            var last = "Marinos";
            var username = "Nikos";
            var avatarsPath = "~/Content/Images";
            var imgName = "nikos.png";

            var model = new UserInfoViewModel()
            {
                IsAuthenticated = true,//HttpContext.User != null && HttpContext.User.Identity != null && HttpContext.User.Identity.IsAuthenticated,
                Name = showFullName ? String.Format("{0}{2}{1}", first, last, String.IsNullOrEmpty(first) || String.IsNullOrEmpty(last) ? String.Empty : " ") : username,
                Initials = showFullName ? String.Format("{0}{1}", first.Substring(0, 1).ToUpper(), last.Substring(0, 1).ToUpper()) : username.Substring(0, 1).ToUpper(),
                Img = String.IsNullOrEmpty(imgName) ? String.Empty : Url.Content(Path.Combine(avatarsPath, imgName)),
                Bg = String.IsNullOrEmpty(imgName) ? GetNewBgColor() : String.Empty
            };
            return PartialView("_Header", model);
        }
        #endregion
        #endregion

        #region Methods
        private static string GetNewBgColor()
        {
            const int colorMin = 40;
            const int colorMax = 180;

            const int minTotalDiff = 80;    // parameter used in new color acceptance criteria
            const int okSingleDiff = 30;    // id.
            int prevR = 0, prevG = 0, prevB = 0, newR = 0, newG = 0, newB = 0;
            var rand = new Random();

            bool found = false;
            while (!found)
            {
                newR = rand.Next(colorMin, colorMax);
                newG = rand.Next(colorMin, colorMax);
                newB = rand.Next(colorMin, colorMax);

                int diffR = Math.Abs(prevR - newR);
                int diffG = Math.Abs(prevG - newG);
                int diffB = Math.Abs(prevB - newB);

                // we only take the new color if...
                //   Collectively the color components are changed by a certain minimum
                //   or if at least one individual colors is changed by "a lot".
                if (diffR + diffG + diffB >= minTotalDiff || diffR >= okSingleDiff || diffG >= okSingleDiff || diffB >= okSingleDiff)
                    found = true;
            }

            prevR = newR;
            prevG = newG;
            prevB = newB;

            return String.Format("#{0:X2}{1:X2}{2:X2}", prevR, prevG, prevB);
        }
        #endregion
    }
}
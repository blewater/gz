using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using gzWeb.Models;

namespace gzWeb.Controllers {
    public class HomeController : Controller
    {
        #region Actions
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        #region Casino

        #endregion

        #region Investment

        #endregion

        #region Shared
        public PartialViewResult Header()
        {
            var first = "Manolis";
            var last = "Marinos";
            var username = "mmarinos";
            var avatarsPath = "~/Content/Images";
            var imgName = "nikos.png";
            var text = String.IsNullOrEmpty(first) || String.IsNullOrEmpty(last)
                       ? String.Format("{0}{2}{1}", first, last, String.IsNullOrEmpty(first) && String.IsNullOrEmpty(last))
                       : username;

            var model = new UserInfoViewModel()
            {
                Text = text,
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
            const int colorMin = 110;
            const int colorMax = 240;

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
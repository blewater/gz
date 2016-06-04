﻿using System.Web.Mvc;
using System.Web.Routing;

namespace gzWeb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new [] { "gzWeb.Areas.Mvc.Controllers" }
            );

            //routes.MapRoute(
            //    name: "MvcRoute",
            //    url: "Mvc/{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);
        }
    }
}

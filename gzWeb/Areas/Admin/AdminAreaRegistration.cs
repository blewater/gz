using System.Web.Mvc;

namespace gzWeb.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            //context.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            context.MapRoute(
                    "AdminRoute",
                    "Admin/{controller}/{action}/{id}",
                    new {action = "Index", id = UrlParameter.Optional},
                    new[] { "gzWeb.Areas.Admin.Controllers" });
        }
    }
}
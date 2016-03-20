using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AutoMapper;
using gzWeb.Models;
using Glimpse.Core.ClientScript;

namespace gzWeb
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Code first Init
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Migrations.Configuration>());
            // Initialize to latest version only if not run before
            new ApplicationDbContext().Database.Initialize(false);

            //Automapper
            Mapper.Initialize(cfg => cfg.CreateMap<ApplicationUser, CustomerDTO>());
        }
    }
}

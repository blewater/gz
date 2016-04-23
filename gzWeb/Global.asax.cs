using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AutoMapper;
using gzDAL.Conf;
using gzDAL.Models;
using gzDAL.DTO;
using gzDAL.Repos;
using gzDAL.Repos.Interfaces;
using gzWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

namespace gzWeb
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Check from web.config or Azure settings if db needs to be init
            bool dbMigrateToLatest = bool.Parse(ConfigurationManager.AppSettings["MigrateDatabaseToLatestVersion"]);

            if (dbMigrateToLatest)
            {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Migrations.Configuration>());
                // Initialize to latest version only if not run before
                new ApplicationDbContext().Database.Initialize(false);
            }

            //Automapper
            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<ApplicationUser, CustomerDTO>();
                cfg.CreateMap<VintageDto, VintageViewModel>();
            });

            InitializeSimpleInjector(mapperConfiguration);
        }

        private void InitializeSimpleInjector(MapperConfiguration automapperConfig)
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();
            container.RegisterSingleton<MapperConfiguration>(automapperConfig);
            container.Register<IMapper>(() => automapperConfig.CreateMapper(container.GetInstance));
            container.Register<ApplicationDbContext, ApplicationDbContext>(Lifestyle.Scoped);
            container.Register<ICustFundShareRepo, CustFundShareRepo>(Lifestyle.Scoped);
            container.Register<ICurrencyRateRepo, CurrencyRateRepo>(Lifestyle.Scoped);
            container.Register<IInvBalanceRepo, InvBalanceRepo>(Lifestyle.Scoped);
            container.Register<IGzTransactionRepo, GzTransactionRepo>(Lifestyle.Scoped);
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);
            container.Verify();
            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
        }
    }
}

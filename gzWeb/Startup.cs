using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AutoMapper;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.Repos;
using gzDAL.Repos.Interfaces;
using gzWeb.Models;
using JSNLog;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.DataProtection;
using NLog.Owin.Logging;
using Owin;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;
using SimpleInjector.Integration.WebApi;
using FluentScheduler;
using gzWeb.Utilities;
using NLog;

[assembly: OwinStartupAttribute(typeof(gzWeb.Startup))]
namespace gzWeb {
    //public interface IOwinContextProvider
    //{
    //    IOwinContext CurrentContext { get; }
    //}

    //public class CallContextOwinContextProvider : IOwinContextProvider
    //{
    //    public IOwinContext CurrentContext
    //    {
    //        get { return (IOwinContext)CallContext.LogicalGetData("IOwinContext"); }
    //    }
    //}

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AreaRegistration.RegisterAllAreas();

            var gzConnStringTupleNameValue = GetDbConnStringValue();
            var gzDbConnStringName = gzConnStringTupleNameValue.Item1.ToLower();
            var gzDbConnStringValue = gzConnStringTupleNameValue.Item2;
#if DEBUG
            // Check from web.config or Azure settings if db needs to be init
            bool dbMigrateToLatest = bool.Parse(ConfigurationManager.AppSettings["MigrateDatabaseToLatestVersion"]);

            // Add 1 more check against prod database in case debug mode is on
            if (dbMigrateToLatest && !gzDbConnStringName.Contains("prod")) {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Migrations.Configuration>());
                // Initialize to latest version only if not run before
                new ApplicationDbContext().Database.Initialize(false);
            }
#endif
            // Method body should be kept empty; please put any initializations in Startup.cs
            NLog.GlobalDiagnosticsContext.Set("gzConnectionString", gzDbConnStringValue);

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Automapper
            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<ApplicationUser, CustomerDTO>();
                cfg.CreateMap<VintageDto, VintageViewModel>().ReverseMap();
            });

            var config = new HttpConfiguration();
            
            var container = InitializeSimpleInjector(app, config, mapperConfiguration);

            //DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));

            ConfigureAuth(app, () => container.GetInstance<ApplicationUserManager>());

            WebApiConfig.Register(config);
            app.UseWebApi(config);

            app.UseJSNLog();
            app.UseNLog();

            // Start the scheduler
            JobManager.Initialize(new GlobalScheduledJobsRegistry());

            //app.CreatePerOwinContext(() => container.GetInstance<ApplicationUserManager>());
        }

        private static Container InitializeSimpleInjector(IAppBuilder app, HttpConfiguration config, MapperConfiguration automapperConfig)
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();

            app.Use(async (context, next) =>
                          {
                              using (container.BeginExecutionContextScope())
                              {
                                  await next();
                              }
                          });

            container.RegisterSingleton<MapperConfiguration>(automapperConfig);
            container.Register<IMapper>(() => automapperConfig.CreateMapper(container.GetInstance));

            if (ConfigurationManager.AppSettings.AllKeys.Any(x => x == "UNIT_TEST"))
                app.SetDataProtectionProvider(new DpapiDataProtectionProvider());

            container.RegisterSingleton(new DataProtectionProviderFactory(app.GetDataProtectionProvider));
            container.Register(()=>new ApplicationDbContext(), Lifestyle.Scoped);
            container.Register<ApplicationUserManager>(Lifestyle.Scoped);
            container.Register<IUserStore<ApplicationUser, int>, CustomUserStore>(Lifestyle.Scoped);
            container.Register<IUserRepo, UserRepo>(Lifestyle.Scoped);
            container.Register<ICustFundShareRepo, CustFundShareRepo>(Lifestyle.Scoped);
            container.Register<ICurrencyRateRepo, CurrencyRateRepo>(Lifestyle.Scoped);
            container.Register<ICustPortfolioRepo, CustPortfolioRepo>(Lifestyle.Scoped);
            container.Register<IInvBalanceRepo, InvBalanceRepo>(Lifestyle.Scoped);
            container.Register<IGzTransactionRepo, GzTransactionRepo>(Lifestyle.Scoped);
            container.Register<IEmailService, SendGridEmailService>(Lifestyle.Scoped);
            container.Register<ICacheUserData, CacheUserData>(Lifestyle.Scoped);
            container.RegisterWebApiControllers(config);
            
            container.Verify();

            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            return container;
        }

        /// <summary>
        /// 
        /// Return the RunTime Configured Db Connection String Name & Value
        /// 
        /// </summary>
        /// <returns>Tuple of DbConn Name & Value</returns>
        private Tuple<string, string> GetDbConnStringValue() {

            var gzDbConnStringName = ApplicationDbContext.GetCompileModeConnString(null);
            var gzDbConnStringValue = ConfigurationManager.ConnectionStrings[gzDbConnStringName].ConnectionString;
            return Tuple.Create(gzDbConnStringName, gzDbConnStringValue);
        }

    }
}

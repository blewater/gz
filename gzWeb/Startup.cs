using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AutoMapper;
using Common.Logging.NLog;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.Repos;
using gzDAL.Repos.Interfaces;
using gzWeb.Models;
using JSNLog;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using NLog.Owin.Logging;
using Owin;
using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;
using SimpleInjector.Integration.WebApi;

[assembly: OwinStartupAttribute(typeof(gzWeb.Startup))]
namespace gzWeb
{
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
            //NLog.GlobalDiagnosticsContext.Set("connectionString", ApplicationDbContext.GetCompileModeConnString(null));
            var databaseTarget = (NLog.Targets.DatabaseTarget)NLog.LogManager.Configuration.FindTargetByName("dbLog");
            databaseTarget.ConnectionStringName = ApplicationDbContext.GetCompileModeConnString(null);
            NLog.LogManager.ReconfigExistingLoggers();

            //AreaRegistration.RegisterAllAreas();
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
            ConfigureAuth(app, () => container.GetInstance<ApplicationUserManager>());

            WebApiConfig.Register(config);
            app.UseWebApi(config);

            app.UseJSNLog();
            app.UseNLog();

            //app.CreatePerOwinContext(() => container.GetInstance<ApplicationUserManager>());
        }

        private static Container InitializeSimpleInjector(IAppBuilder app, HttpConfiguration config, MapperConfiguration automapperConfig)
        {
            var container = new Container();
            //container.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();
            container.Options.DefaultScopedLifestyle = new WebApiRequestLifestyle();

            app.Use(async (context, next) =>
                          {
                              using (container.BeginExecutionContextScope())
                              {
                                  await next();
                              }
                          });

            //app.Use(async (context, next) =>
            //              {
            //                  CallContext.LogicalSetData("IOwinContext", context);
            //                  await next();
            //              });

            //container.RegisterSingleton<IOwinContextProvider>(new CallContextOwinContextProvider());

            container.RegisterSingleton<MapperConfiguration>(automapperConfig);
            container.Register<IMapper>(() => automapperConfig.CreateMapper(container.GetInstance));

            if (ConfigurationManager.AppSettings.AllKeys.Any(x => x == "UNIT_TEST"))
                app.SetDataProtectionProvider(new DpapiDataProtectionProvider());

            container.RegisterSingleton(new DataProtectionProviderFactory(app.GetDataProtectionProvider));
            container.Register(()=>new ApplicationDbContext(), Lifestyle.Scoped);
            container.Register<ApplicationUserManager>(Lifestyle.Scoped);
            container.Register<IUserStore<ApplicationUser, int>, CustomUserStore>(Lifestyle.Scoped);
            container.Register<ICustFundShareRepo, CustFundShareRepo>(Lifestyle.Scoped);
            container.Register<ICurrencyRateRepo, CurrencyRateRepo>(Lifestyle.Scoped);
            container.Register<ICustPortfolioRepo, CustPortfolioRepo>(Lifestyle.Scoped);
            container.Register<IInvBalanceRepo, InvBalanceRepo>(Lifestyle.Scoped);
            container.Register<IGzTransactionRepo, GzTransactionRepo>(Lifestyle.Scoped);
            container.Register<IEmailService, SendGridEmailService>(Lifestyle.Scoped);
            container.RegisterWebApiControllers(config);
            
            container.Verify();

            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            return container;
        }
    }
}

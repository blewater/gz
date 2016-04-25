using System.Configuration;
using System.Data.Entity;
using System.Runtime.Remoting.Messaging;
using System.Web;
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
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
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
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Automapper
            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<ApplicationUser, CustomerDTO>();
                cfg.CreateMap<VintageDto, VintageViewModel>();
            });
            
            var container = InitializeSimpleInjector(app, mapperConfiguration);
            
            // Check from web.config or Azure settings if db needs to be init
            bool dbMigrateToLatest = bool.Parse(ConfigurationManager.AppSettings["MigrateDatabaseToLatestVersion"]);

            if (dbMigrateToLatest)
            {
                Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Migrations.Configuration>());
                // Initialize to latest version only if not run before
                new ApplicationDbContext().Database.Initialize(false);
            }
            
            ConfigureAuth(app, container);
        }

        private Container InitializeSimpleInjector(IAppBuilder app, MapperConfiguration automapperConfig)
        {
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();

            app.Use(async (context, next) =>
            {
                using (var scope = container.BeginExecutionContextScope())
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
            

            container.Register<ApplicationDbContext>(Lifestyle.Scoped);
            container.Register<ApplicationSignInManager>(Lifestyle.Scoped);
            container.Register<ApplicationUserManager>(Lifestyle.Scoped);
            container.Register<IDataProtectionProvider>(app.GetDataProtectionProvider, Lifestyle.Scoped);
            
            //container.Register<IAuthenticationManager>(() => DependencyResolver.Current.GetService<IOwinContextProvider>().CurrentContext.Authentication, Lifestyle.Scoped);
            container.Register(() =>
                               {
                                   if (HttpContext.Current != null &&
                                       HttpContext.Current.Items["owin.Environment"] == null && 
                                       container.IsVerifying)
                                   {
                                       return new OwinContext().Authentication;
                                   }
                                   return HttpContext.Current.GetOwinContext().Authentication;
                               }, Lifestyle.Scoped);
            container.Register<IUserStore<ApplicationUser, int>, CustomUserStore>(Lifestyle.Scoped);
            container.Register<ICustFundShareRepo, CustFundShareRepo>(Lifestyle.Scoped);
            container.Register<ICurrencyRateRepo, CurrencyRateRepo>(Lifestyle.Scoped);
            container.Register<IInvBalanceRepo, InvBalanceRepo>(Lifestyle.Scoped);
            container.Register<IGzTransactionRepo, GzTransactionRepo>(Lifestyle.Scoped);
            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);
            container.Verify();

            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            GlobalConfiguration.Configure(WebApiConfig.Register);

            return container;
        }
    }
}

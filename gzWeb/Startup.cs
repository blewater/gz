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
            //AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Automapper
            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<ApplicationUser, CustomerDTO>();
                cfg.CreateMap<VintageDto, VintageViewModel>();
            });


            var config = new HttpConfiguration();
            ConfigureAuth(app, null);
            var container = InitializeSimpleInjector(app, config, mapperConfiguration);

            WebApiConfig.Register(config);
            app.UseWebApi(config);

            app.CreatePerOwinContext(() => container.GetInstance<ApplicationUserManager>());
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
            if ((string) app.Properties["host.AppMode"] == "development")
                app.SetDataProtectionProvider(new DpapiDataProtectionProvider());

            //container.RegisterSingleton(app);
            container.Register(() => app.GetDataProtectionProvider(), Lifestyle.Scoped);
            container.Register<ApplicationDbContext>(Lifestyle.Scoped);
            //container.Register<ApplicationSignInManager>(Lifestyle.Scoped);
            container.Register<ApplicationUserManager>(Lifestyle.Scoped);
            
            //container.Register<IAuthenticationManager>(() => DependencyResolver.Current.GetService<IOwinContextProvider>().CurrentContext.Authentication, Lifestyle.Scoped);
            //container.Register(() =>
            //                   {
            //                       return container.GetInstance<IOwinContextProvider>().CurrentContext.Authentication;
            //                       //if (HttpContext.Current != null &&
            //                       //    HttpContext.Current.Items["owin.Environment"] == null && 
            //                       //    container.IsVerifying)
            //                       //{
            //                       //    return new OwinContext().Authentication;
            //                       //}
            //                       //return HttpContext.Current.GetOwinContext().Authentication;
            //                   }, Lifestyle.Scoped);
            container.Register<IUserStore<ApplicationUser, int>, CustomUserStore>(Lifestyle.Scoped);
            container.Register<ICustFundShareRepo, CustFundShareRepo>(Lifestyle.Scoped);
            container.Register<ICurrencyRateRepo, CurrencyRateRepo>(Lifestyle.Scoped);
            container.Register<IInvBalanceRepo, InvBalanceRepo>(Lifestyle.Scoped);
            container.Register<IGzTransactionRepo, GzTransactionRepo>(Lifestyle.Scoped);
            container.RegisterWebApiControllers(config);
            
            container.Verify();

            config.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            //GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);

            //GlobalConfiguration.Configure(WebApiConfig.Register);

            return container;
        }
    }
}

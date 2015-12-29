using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(gzWeb.Startup))]
namespace gzWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

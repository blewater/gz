using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(gzWeb.Admin.Startup))]
namespace gzWeb.Admin
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}

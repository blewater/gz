using System;
using System.Net.Http;
using System.Web.Http;
using gzDAL.Conf;
using gzWeb.Utilities;
using Microsoft.AspNet.Identity.Owin;

namespace gzWeb.Controllers
{
    public class BaseApiController : ApiController
    {
        private ApplicationUserManager _userManager;
        protected ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        protected internal OkWithMessageResult OkMsg(object obj, string message = "", bool showStackTrace = true)
        {
            return new OkWithMessageResult(Request, Response.Try(() => obj, message, showStackTrace));
        }
        protected internal OkWithMessageResult OkMsg(Func<object> func, string message = "", bool showStackTrace = true)
        {
            return new OkWithMessageResult(Request, Response.Try(func, message, showStackTrace));
        }
        protected internal OkWithMessageResult OkMsg(Action action, string message = "", bool showStackTrace = true)
        {
            return new OkWithMessageResult(Request, Response.Try(action));
        }
    }
}

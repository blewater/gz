using System;
using System.Web.Http;
using gzWeb.Utilities;

namespace gzWeb.Controllers
{
    public class BaseApiController : ApiController
    {
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

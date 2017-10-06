using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace gzWeb.Controllers
{
    /// <summary>
    /// https://stackoverflow.com/questions/39327231/caching-of-asp-net-mvc-web-api-results
    /// </summary>
    public class ClientCachingInApi : ActionFilterAttribute

    {
        /// <summary>
        /// Caching duration in seconds.
        /// </summary>
        public int Duration { get; set; }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext) {

            actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue {
                MaxAge = TimeSpan.FromSeconds(Duration),
                MustRevalidate = true,
                Public = true
            };

        }
    }
}
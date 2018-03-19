﻿using System;
using System.Web.Http;
using gzDAL.Conf;
using gzWeb.Utilities;
using Microsoft.ApplicationInsights;

namespace gzWeb.Controllers
{
    public class BaseApiController : ApiController
    {
        protected TelemetryClient telemetry;

        public BaseApiController(ApplicationUserManager userManager)
        {
            telemetry = new TelemetryClient();
            UserManager = userManager;
        }
        
        protected ApplicationUserManager UserManager { get; }

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

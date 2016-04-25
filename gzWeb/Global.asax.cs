using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AutoMapper;
using gzDAL.Conf;
using gzDAL.Models;
using gzDAL.DTO;
using gzDAL.Repos;
using gzDAL.Repos.Interfaces;
using gzWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

namespace gzWeb
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Method body should be kept empty; please put any initializations in Startup.cs
        }
    }
}

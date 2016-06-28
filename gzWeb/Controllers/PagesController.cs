using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using gzDAL.Conf;
using gzDAL.Models;

namespace gzWeb.Controllers
{
    [Authorize]
    [RoutePrefix("api/Pages")]
    public class PagesController : BaseApiController
    {
        public PagesController(ApplicationUserManager userManager, ApplicationDbContext dbContext)
                : base(userManager)
        {
            _dbContext = dbContext;
        }

        [Route("Carousel")]
        public IHttpActionResult Carousel()
        {
            var now = DateTime.UtcNow;
            return OkMsg(() => _dbContext.DynamicPages.Where(x => x.Live && x.Category == "carousel" && x.LiveFrom >= now && x.LiveTo <= now));
        }

        [Route("Page")]
        public IHttpActionResult Page(string code)
        {
            return OkMsg(() => _dbContext.DynamicPages.SingleOrDefault(x => x.Code == code));
        }

        private readonly ApplicationDbContext _dbContext;
    }
}

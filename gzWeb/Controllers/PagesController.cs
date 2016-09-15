using System;
using System.Linq;
using System.Web.Http;
using gzDAL.Conf;
using gzDAL.Models;

namespace gzWeb.Controllers
{
    //[RoutePrefix("api/Pages")]
    public class PagesController : BaseApiController
    {
        public PagesController(ApplicationUserManager userManager, ApplicationDbContext dbContext)
                : base(userManager)
        {
            _dbContext = dbContext;
        }

        //[Route("Carousel")]
        [HttpGet]
        public IHttpActionResult Carousel()
        {
            var now = DateTime.UtcNow;
            return OkMsg(() => _dbContext.CarouselEntries.Where(x => x.Live && now >= x.LiveFrom && now <= x.LiveTo));
        }

        //[Route("Page")]
        [HttpGet]
        public IHttpActionResult Page(string code)
        {
            return OkMsg(() => _dbContext.DynamicPages.SingleOrDefault(x => x.Code == code));
        }

        private readonly ApplicationDbContext _dbContext;
    }
}

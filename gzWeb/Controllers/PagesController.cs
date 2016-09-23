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
        public IHttpActionResult Thumbnails()
        {
            var now = DateTime.UtcNow;
            return OkMsg(() =>
                _dbContext.DynamicPages
                          .Where(x => x.Live && now >= x.LiveFrom && now <= x.LiveTo)
                          .Select(x => new
                          {
                              Code = x.Code,
                              Img = x.ThumbImageUrl,
                              Title = x.ThumbTitle,
                              Description = x.ThumbText,
                          })
                          .ToList());
        }
        [HttpGet]
        public IHttpActionResult Page(string code)
        {
            return OkMsg(() => _dbContext.DynamicPages.SingleOrDefault(x => x.Code == code).Html);
        }

        [HttpGet]
        public IHttpActionResult Categories()
        {
            var dbCategories = _dbContext.GameCategories.ToList();
            var customCategories = dbCategories.Select(x => new
            {
                x.Code,
                x.Title,
                GameSlugs = x.GameSlugs.Split(';')
            }).ToList();

            return OkMsg(() => customCategories);
        }

        private readonly ApplicationDbContext _dbContext;
    }
}

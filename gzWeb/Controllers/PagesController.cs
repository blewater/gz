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
        public IHttpActionResult Carousel(bool isMobile)
        {
            var now = DateTime.UtcNow;
            return OkMsg(() => _dbContext.CarouselEntries.Where(x => x.Live && now >= x.LiveFrom && now <= x.LiveTo && !x.Deleted && x.IsMobile == isMobile));
        }

        //[Route("Page")]
        [HttpGet]
        public IHttpActionResult Thumbnails(bool isMobile)
        {
            var now = DateTime.UtcNow;
            //return OkMsg(() =>
            //    _dbContext.DynamicPages
            //              .Where(x => x.Live && now >= x.LiveFrom && now <= x.LiveTo && !x.Deleted && x.UseInPromoList && x.IsMobile==isMobile)
            //              .Select(x => new
            //              {
            //                  Code = x.Code,
            //                  Img = x.ThumbImageUrl,
            //                  Title = x.ThumbTitle,
            //                  Description = x.ThumbText,
            //              })
            //              .ToList());
            return OkMsg(() => {
                var pagesQuerable = _dbContext.DynamicPages.Where(x => x.Live && now >= x.LiveFrom && now <= x.LiveTo && !x.Deleted && x.UseInPromoList);
                if (isMobile)
                    pagesQuerable = pagesQuerable.Where(x => x.IsMobile);
                var pages = pagesQuerable.Select(x => new
                {
                    Code = x.Code,
                    Img = x.ThumbImageUrl,
                    Title = x.ThumbTitle,
                    Description = x.ThumbText,
                }).ToList();
                return pages;
            });
        }
        [HttpGet]
        public IHttpActionResult Page(string code)
        {
            return OkMsg(() => _dbContext.DynamicPages.Single(x => x.Code == code).Html);
        }

        [HttpGet]
        public IHttpActionResult Categories(bool isMobile)
        {
            var dbCategories = _dbContext.GameCategories.Where(x=>x.IsMobile == isMobile).ToList();
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

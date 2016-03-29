using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using gzDAL.Models;
using gzWeb.Models;

namespace gzWeb.Controllers
{
    public class InvestmentsApiController : ApiController
    {
        public InvestmentsApiController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IHttpActionResult GetPortfolios()
        {
            return TryExecute(() => _dbContext.Portfolios.Where(x => x.IsActive)
                                              .Select(x => new
                                                           {
                                                                   x.Id,
                                                                   x.RiskTolerance,
                                                                   Funds =
                                                                   x.PortFunds.Select(f => new {f.Fund.HoldingName, f.Weight})
                                                           })
                                              .ToList());
        }

        protected Response<T> TryExecute<T>(Func<T> func) where T : class
        {
            try
            {
                return new Response<T>(Request, func());
            }
            catch (Exception exception)
            {
                return new Response<T>(Request, null) {Error = new ErrorViewModel {Message = exception.Message}};
            }
        }

        private readonly ApplicationDbContext _dbContext;
    }
}

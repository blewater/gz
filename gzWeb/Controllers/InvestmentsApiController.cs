using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using gzDAL.Conf;
using gzDAL.Models;
using gzDAL.ModelUtil;
using gzWeb.Configuration;
using gzWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NLog.LayoutRenderers;

namespace gzWeb.Controllers
{
    [Authorize]
    // TODO: [RoutePrefix("api/Investments")]
    public class InvestmentsApiController : BaseApiController
    {
        #region Constructor
        public InvestmentsApiController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        #endregion
        
        #region Actions
        #region Summary
        [HttpGet]
        public IHttpActionResult GetSummaryData()
        {
            var now = DateTime.Now;
            var vintages = _dummyVintages;
            var model = new SummaryDataViewModel()
            {
                Currency = "€",
                Culture = "en-US",
                InvestmentsBalance = 15000,
                TotalDeposits = 10000,
                TotalWithdrawals = 30000,
                GamingBalance = 4000,
                TotalInvestments = 14000,
                TotalInvestmentsReturns = 1000,
                NextInvestmentOn = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)),
                LastInvestmentAmount = 1000,
                StatusAsOf = DateTime.Today,
                Vintages = vintages.OrderByDescending(x => x.Date.Year).ThenByDescending(x => x.Date.Month).ToList()
            };
            return OkMsg(model);
        }

        [HttpPost]
        public IHttpActionResult TransferCashToGames()
        {
            return OkMsg(() =>
            {
                
            });
        }
        #endregion

        #region Portfolio
        [HttpGet]
        public IHttpActionResult GetPortfolioData()
        {
            var user = UserManager.FindById(User.Identity.GetUserId<int>());
            if (user == null) 
                return OkMsg(new object(), "User not found!");
            
            var now = DateTime.Now;
            var model = new PortfolioDataViewModel
                        {
                                Currency = user.Currency, //"€", // TODO: get from user data
                                NextInvestmentOn = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)),
                                NextExpectedInvestment = 15000,
                                ROI = new ReturnOnInvestmentViewModel {Title = "% Current ROI", Percent = 59},
                                Plans = GetCustomerPlans(user)
                        };
            return OkMsg(model);
        }

        [HttpPost]
        public IHttpActionResult SetPlanSelection()
        {
            return OkMsg(() =>
            {

            });
        }
        #endregion

        #region Performance
        [HttpGet]
        public IHttpActionResult GetPerformanceData()
        {
            var user = UserManager.FindById(User.Identity.GetUserId<int>());
            if (user == null)
                return OkMsg(new object(), "User not found!");

            var model = new PerformanceDataViewModel()
                        {
                                Currency = "€",
                                Plans = GetCustomerPlans(user)
                        };
            return OkMsg(model);
        }
        #endregion

        #region Activity
        #endregion
        
        [HttpGet]
        public IHttpActionResult GetPortfolios()
        {
            return OkMsg(() =>
            {
                var portfolios =
                    _dbContext.Portfolios
                              .Where(x => x.IsActive)
                              .Select(x => new
                                      {
                                          x.Id,
                                          x.RiskTolerance,
                                          Funds = x.PortFunds.Select(f => new {f.Fund.HoldingName, f.Weight})
                                      })
                              .ToList();
                return portfolios;
            });
        }
        #endregion

        private IEnumerable<PlanViewModel> GetCustomerPlans(ApplicationUser user)
        {
            var yearMonth = DbExpressions.GetStrYearMonth(DateTime.Now.Year, DateTime.Now.Month);
            var customerPortfolio = _dbContext.CustPortfolios
                                             .SingleOrDefault(x => x.CustomerId == user.Id &&
                                                                   x.YearMonth == yearMonth);
            var portfolios = _dbContext.Portfolios
                             .Where(x => x.IsActive);

            foreach (var portfolio in portfolios)
            {
                yield return
                        CreateFromPrototype(portfolio,
                                            customerPortfolio,
                                            customerPortfolio != null && portfolio.Id == customerPortfolio.PortfolioId);
            }
        }

        private PlanViewModel CreateFromPrototype(Portfolio x, CustPortfolio custPortfolio, bool active)
        {
            var portfolioPrototype = PortfolioConfiguration.GetPortfolioPrototype(x.RiskTolerance);
            return new PlanViewModel
                   {
                           Id = x.Id,
                           Title = portfolioPrototype.Title,
                           Balance = 0, // TODO: get from customer data
                           Color = portfolioPrototype.Color,
                           Percent = active ? custPortfolio.Weight : 0,
                           ReturnRate = 0, // TODO: ???
                           Selected = active, // TODO: get from customer data
                           Holdings = x.PortFunds.Select(f => new HoldingViewModel
                                                              {
                                                                      Name = f.Fund.HoldingName,
                                                                      Weight = f.Weight
                                                              })
                   };
        }
        
        #region Fields
        private readonly ApplicationDbContext _dbContext;
        #endregion

        #region Dummy Data
        
        private static IList<VintageViewModel> _dummyVintages = new[]
        {
            new VintageViewModel {Date = new DateTime(2014, 7, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2014, 8, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2014, 9, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2014, 10, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2014, 11, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2014, 12, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2015, 1, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2015, 2, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2015, 3, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2015, 4, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2015, 5, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2015, 6, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2015, 7, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2015, 8, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2015, 9, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2015, 10, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2015, 11, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2015, 12, 1), InvestAmount = 100M, ReturnPercent = 10},
            new VintageViewModel {Date = new DateTime(2016, 1, 1), InvestAmount = 200M, ReturnPercent = -5},
            new VintageViewModel {Date = new DateTime(2016, 2, 1), InvestAmount = 80M, ReturnPercent = 15},
            new VintageViewModel {Date = new DateTime(2016, 3, 1), InvestAmount = 150M, ReturnPercent = 10},
        };
        
        #endregion
    }
}

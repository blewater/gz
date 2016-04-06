using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using gzDAL.Models;
using gzWeb.Models;

namespace gzWeb.Controllers
{
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
            var now = DateTime.Now;
            var model = new PortfolioDataViewModel()
            {
                Currency = "€",
                NextInvestmentOn = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)),
                NextExpectedInvestment = 15000,
                ROI = new ReturnOnInvestmentViewModel() { Title = "% Current ROI", Percent = 59 },
                Plans = _dummyPlans
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
            var model = new PerformanceDataViewModel()
            {
                Currency = "€",                
                Plans = _dummyPlans
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

        #region Fields
        private readonly ApplicationDbContext _dbContext;
        #endregion

        #region Dummy Data
        private static IList<HoldingViewModel> _dummyHoldings = new[]
        {
            new HoldingViewModel() {Name = "Vanguard VTI", Weight = 8},
            new HoldingViewModel() {Name = "Vanguard VEA", Weight = 5},
            new HoldingViewModel() {Name = "Vanguard VWO", Weight = 5},
            new HoldingViewModel() {Name = "Vanguard VIG", Weight = 15},
            new HoldingViewModel() {Name = "State Street XLE", Weight = 7},
            new HoldingViewModel() {Name = "Schwab SCHP", Weight = 25},
            new HoldingViewModel() {Name = "State Street XLE", Weight = 35}
        };
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
        private static PlanViewModel _dummyAggressive = new PlanViewModel()
        {
            Title = "Aggressive",
            Selected = false,
            Percent = 34,
            Balance = 1500,
            ReturnRate = 0.1,
            Color = "#227B46",
            Holdings = _dummyHoldings
        };
        private static PlanViewModel _dummyModerate = new PlanViewModel()
        {
            Title = "Moderate",
            Selected = true,
            Percent = 23,
            Balance = 1500,
            ReturnRate = 0.07,
            Color = "#64BF89",
            Holdings = _dummyHoldings
        };
        private static PlanViewModel _dummyConservative = new PlanViewModel()
        {
            Title = "Conservative",
            Selected = false,
            Percent = 43,
            Balance = 1500,
            ReturnRate = 0.04,
            Color = "#B4DCC4",
            Holdings = _dummyHoldings
        };
        private static IList<PlanViewModel> _dummyPlans = new [] { _dummyConservative, _dummyModerate, _dummyAggressive };
        #endregion
    }
}

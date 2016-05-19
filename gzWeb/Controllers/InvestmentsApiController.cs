using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Http;
using AutoMapper;
using gzDAL;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.ModelUtil;
using gzDAL.Repos;
using gzDAL.Repos.Interfaces;
using gzWeb.Configuration;
using gzWeb.Contracts;
using gzWeb.Models;
using gzWeb.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NLog.LayoutRenderers;

namespace gzWeb.Controllers
{
    [Authorize]
    // TODO: [RoutePrefix("api/Investments")]
    public class InvestmentsApiController : BaseApiController, IInvestmentsApi
    {
        #region Constructor
        public InvestmentsApiController(
            ApplicationDbContext dbContext,
            IInvBalanceRepo invBalanceRepo,
            IGzTransactionRepo gzTransactionRepo,
            ICustFundShareRepo custFundShareRepo,
            ICurrencyRateRepo currencyRateRepo,
            IMapper mapper,
            ApplicationUserManager userManager):base(userManager)
        {
            _dbContext = dbContext;
            _invBalanceRepo = invBalanceRepo;
            _gzTransactionRepo = gzTransactionRepo;
            _custFundShareRepo = custFundShareRepo;
            _currencyRateRepo = currencyRateRepo;
            _mapper = mapper;
        }
        #endregion
        
        #region Actions

        #region Summary
        [HttpGet]
        public IHttpActionResult GetSummaryData() {

            var user = UserManager.FindById(User.Identity.GetUserId<int>());
            if (user == null)
                return OkMsg(new object(), "User not found!");
            
            return OkMsg(((IInvestmentsApi)this).GetSummaryData(user));
        }

        /// <summary>
        /// 
        /// Get the summary user data converted to the user currency.
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        SummaryDataViewModel IInvestmentsApi.GetSummaryData(ApplicationUser user)
        {
            CurrencyInfo userCurrency;
            decimal usdToUserRate = GetUserCurrencyRate(user, out userCurrency);
            var withdrawalEligibility = _gzTransactionRepo.GetWithdrawEligibilityData(user.Id);

            var customerVintages = _gzTransactionRepo
                .GetCustomerVintages(user.Id)

                // Convert to User currency
                .Select(v => new VintageDto() {
                    InvestAmount = DbExpressions.RoundCustomerBalanceAmount(v.InvestAmount * usdToUserRate),
                    YearMonthStr = v.YearMonthStr,
                    Locked = v.Locked,
                    Sold = v.Sold
                }).ToList();

            var vintagesVMs = customerVintages.Select(t => _mapper.Map<VintageDto, VintageViewModel>(t)).ToList();

            var summaryDvm = new SummaryDataViewModel
            {
                //Currency = userCurrency.Symbol,
                //Culture = "en-GB",
                InvestmentsBalance = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * user.InvBalance),
                TotalDeposits = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * user.TotalDeposits),
                TotalWithdrawals = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * user.TotalWithdrawals),

                //TODO: from the EveryMatrix Web API
                //GamingBalance = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate),

                TotalInvestments = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * user.TotalInvestments),

                // TODO (Mario): Check if it's more accurate to report this as [InvestmentsBalance - TotalInvestments]
                TotalInvestmentsReturns = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * user.TotalInvestmentReturns),

                NextInvestmentOn = DbExpressions.GetNextMonthsFirstWeekday(),
                LastInvestmentAmount = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * user.LastInvestmentAmount),
                StatusAsOf = _invBalanceRepo.GetLastUpdatedDateTime(user.Id),
                Vintages = vintagesVMs,

                // Withdrawal eligibility
                LockInDays = withdrawalEligibility.LockInDays,
                EligibleWithdrawDate = withdrawalEligibility.EligibleWithdrawDate,
                OkToWithdraw = withdrawalEligibility.OkToWithdraw,
                Prompt = withdrawalEligibility.Prompt
            };

            return summaryDvm;
        }

        /// <summary>
        /// 
        /// Get the user currency and exchange rate from dollar to user.
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="userCurrency"></param>
        /// <returns></returns>
        private decimal GetUserCurrencyRate(ApplicationUser user, out CurrencyInfo userCurrency) {

            userCurrency = CurrencyHelper.GetSymbol(user.Currency);
            var usdToUserRate = _currencyRateRepo.GetLastCurrencyRateFromUSD(userCurrency.ISOSymbol);
            return usdToUserRate;
        }

        /// <summary>
        /// 
        /// HttpGet the Vintages with their Selling Values calculated
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet] public IHttpActionResult GetVintagesWithSellingValues()
        {
            var user = UserManager.FindById(User.Identity.GetUserId<int>());
            if (user == null)
                return OkMsg(new object(), "User not found!");

            var userVintages = GetVintagesSellingValuesByUser(user);

            return OkMsg(() => userVintages);
        }

        /// <summary>
        /// 
        /// Get the customer vintages with their selling value converted to the user currency.
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IEnumerable<VintageViewModel> GetVintagesSellingValuesByUser(ApplicationUser user) {

            CurrencyInfo userCurrency;
            decimal usdToUserRate = GetUserCurrencyRate(user, out userCurrency);

            var customerVintages = _invBalanceRepo
            .GetCustomerVintagesSellingValue(user.Id)

            // Convert to User currency
            .Select(v => new VintageDto() {
                YearMonthStr = v.YearMonthStr,
                InvestAmount = DbExpressions.RoundCustomerBalanceAmount(v.InvestAmount * usdToUserRate),
                SellingValue = DbExpressions.RoundCustomerBalanceAmount(v.SellingValue * usdToUserRate),
                Locked = v.Locked,
                Sold = v.Sold
            }).ToList();

            var vintages = customerVintages.Select(t => _mapper.Map<VintageDto, VintageViewModel>(t)).ToList();
            return vintages;
        }


        [HttpPost]
        public IHttpActionResult WithdrawVintages(IList<VintageViewModel> vintages)
        {
            var vintagesDtos = vintages.Select(v => 
                _mapper.Map<VintageViewModel, VintageDto>(v))
                .ToList();

            var updatedVintages = _invBalanceRepo.SaveDbSellVintages(
                    User.Identity.GetUserId<int>(), vintagesDtos)
                .Select(v => _mapper.Map<VintageDto, VintageViewModel>(v))
                .ToList();

            // TODO Actual withdraw and return remaining vintages
            return OkMsg(() => updatedVintages);
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
                            //Currency = CurrencyHelper.GetSymbol(user.Currency).Symbol,
                            NextInvestmentOn = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)),
                            NextExpectedInvestment = 15000,
                            //ROI = new ReturnOnInvestmentViewModel { Title = "% Current ROI", Percent = 59 },
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

            var model = new PerformanceDataViewModel
                        {
                                Currency = CurrencyHelper.GetSymbol(user.Currency).Symbol,
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
            var customerPortfolio = _custFundShareRepo.GetCurrentCustomerPortfolio(user.Id);

            var portfolios = _dbContext.Portfolios
                             .Where(x => x.IsActive);

            foreach (var portfolio in portfolios)
            {
                yield return CreateFromPrototype(portfolio, customerPortfolio != null && portfolio.Id == customerPortfolio.Id);
            }
        }

        private PlanViewModel CreateFromPrototype(Portfolio x, bool active)
        {
            var portfolioPrototype = PortfolioConfiguration.GetPortfolioPrototype(x.RiskTolerance);
            return new PlanViewModel
                   {
                           Id = x.Id,
                           Title = portfolioPrototype.Title,
                           UserBalance = 0, // TODO: get from customer data
                           Color = portfolioPrototype.Color,
                           AllocationPercent = active ? 100 : 0,
                           ROI = 0, // TODO: ???
                           Selected = active, // TODO: get from customer data
                           Holdings = x.PortFunds.Select(f => new HoldingViewModel
                                                              {
                                                                      Name = f.Fund.HoldingName,
                                                                      Weight = f.Weight
                                                              })
                   };
        }
        
        #region Fields

        private readonly IMapper _mapper;
        private readonly IInvBalanceRepo _invBalanceRepo;
        private readonly IGzTransactionRepo _gzTransactionRepo;
        private readonly ICurrencyRateRepo _currencyRateRepo;
        private readonly ICustFundShareRepo _custFundShareRepo;
        private readonly ApplicationDbContext _dbContext;

        #endregion

        #region Dummy Data
        
        private static IList<VintageViewModel> _dummyVintages = new[]
        {
            new VintageViewModel {YearMonthStr = "201407", InvestAmount = 100M, SellingValue = 102M, Locked = true },
            new VintageViewModel {YearMonthStr = "201408", InvestAmount = 100M, SellingValue = 109M, Locked = true },
            new VintageViewModel {YearMonthStr = "201409", InvestAmount = 100M, SellingValue = 107M, Locked = true },
            new VintageViewModel {YearMonthStr = "201410", InvestAmount = 100M, SellingValue = 112M, Locked = true },
            new VintageViewModel {YearMonthStr = "201411", InvestAmount = 100M, SellingValue = 111M, Locked = true },
            new VintageViewModel {YearMonthStr = "201412", InvestAmount = 100M, SellingValue = 106M },
            new VintageViewModel {YearMonthStr = "201501", InvestAmount = 100M, SellingValue = 107M },
            new VintageViewModel {YearMonthStr = "201502", InvestAmount = 100M, SellingValue = 103M },
            new VintageViewModel {YearMonthStr = "201503", InvestAmount = 100M, SellingValue = 105M, Sold = true},
            new VintageViewModel {YearMonthStr = "201504", InvestAmount = 100M, SellingValue = 105M },
            new VintageViewModel {YearMonthStr = "201505", InvestAmount = 100M, SellingValue = 102M },
            new VintageViewModel {YearMonthStr = "201506", InvestAmount = 100M, SellingValue = 106M },
            new VintageViewModel {YearMonthStr = "201507", InvestAmount = 100M, SellingValue = 107M, Sold = true },
            new VintageViewModel {YearMonthStr = "201508", InvestAmount = 100M, SellingValue = 109M },
            new VintageViewModel {YearMonthStr = "201509", InvestAmount = 100M, SellingValue = 100M },
            new VintageViewModel {YearMonthStr = "201510", InvestAmount = 100M, SellingValue = 106M },
            new VintageViewModel {YearMonthStr = "201511", InvestAmount = 100M, SellingValue = 108M, Sold = true },
            new VintageViewModel {YearMonthStr = "201512", InvestAmount = 100M, SellingValue = 105M },
            new VintageViewModel {YearMonthStr = "201601", InvestAmount = 200M, SellingValue = 204M },
            new VintageViewModel {YearMonthStr = "201602", InvestAmount = 80M, SellingValue = 93M },
            new VintageViewModel {YearMonthStr = "201603", InvestAmount = 150M, SellingValue = 158M },
        };
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

        private static PlanViewModel _dummyAggressive = new PlanViewModel()
        {
            Title = "Aggressive",
            Selected = false,
            AllocationPercent = 34,
            UserBalance = 1500,
            ROI = 0.1,
            Color = "#227B46",
            Holdings = _dummyHoldings
        };
        private static PlanViewModel _dummyModerate = new PlanViewModel()
        {
            Title = "Moderate",
            Selected = true,
            AllocationPercent = 23,
            UserBalance = 1500,
            ROI = 0.07,
            Color = "#64BF89",
            Holdings = _dummyHoldings
        };
        private static PlanViewModel _dummyConservative = new PlanViewModel()
        {
            Title = "Conservative",
            Selected = false,
            AllocationPercent = 43,
            UserBalance = 1500,
            ROI = 0.04,
            Color = "#B4DCC4",
            Holdings = _dummyHoldings
        };
        private static IList<PlanViewModel> _dummyPlans = new[] { _dummyConservative, _dummyModerate, _dummyAggressive };
        #endregion
    }
}

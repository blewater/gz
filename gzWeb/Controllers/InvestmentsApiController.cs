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
using gzWeb.Contracts;
using gzWeb.Models;
using gzWeb.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NLog.LayoutRenderers;
using Z.EntityFramework.Plus;

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
            ICustPortfolioRepo custPortfolioRepo,
            IUserRepo userRepo,
            IMapper mapper,
            ApplicationUserManager userManager):base(userManager)
        {
            _dbContext = dbContext;
            _invBalanceRepo = invBalanceRepo;
            _gzTransactionRepo = gzTransactionRepo;
            _custFundShareRepo = custFundShareRepo;
            _currencyRateRepo = currencyRateRepo;
            _custPortfolioRepo = custPortfolioRepo;
            _userRepo = userRepo;
            _mapper = mapper;
        }
        #endregion
        
        #region Actions

        #region Summary
        [HttpGet]
        public IHttpActionResult GetSummaryData() {

            var userId = User.Identity.GetUserId<int>();
            ApplicationUser user;
            var summaryDto = _userRepo.GetSummaryData(userId, out user);

            if (user == null)
                return OkMsg(new object(), "User not found!");

            return OkMsg(((IInvestmentsApi)this).GetSummaryData(user, summaryDto));
        }

        /// <summary>
        /// 
        /// Get the summary user data converted to the user currency.
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="summaryDto"></param>
        /// <returns></returns>
        SummaryDataViewModel IInvestmentsApi.GetSummaryData(ApplicationUser user, UserSummaryDTO summaryDto)
        {
            CurrencyInfo userCurrency;
            decimal usdToUserRate = GetUserCurrencyRate(user, out userCurrency);
            var withdrawalEligibility = _gzTransactionRepo.GetWithdrawEligibilityData(user.Id);

            var summaryDvm = new SummaryDataViewModel
            {
                //Currency = userCurrency.Symbol,
                //Culture = "en-GB",
                InvestmentsBalance = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * summaryDto.InvestmentsBalance),
                TotalDeposits = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * summaryDto.TotalDeposits),
                TotalWithdrawals = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * summaryDto.TotalWithdrawals),

                //TODO: from the EveryMatrix Web API
                //GamingBalance = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate),

                TotalInvestments = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * summaryDto.TotalInvestments),

                // TODO (Mario): Check if it's more accurate to report this as [InvestmentsBalance - TotalInvestments]
                TotalInvestmentsReturns = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * summaryDto.TotalInvestmentsReturns),

                NextInvestmentOn = DbExpressions.GetNextMonthsFirstWeekday(),
                LastInvestmentAmount = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * summaryDto.LastInvestmentAmount),
                StatusAsOf = summaryDto.StatusAsOf,
                //Vintages = vintagesVMs,

                // Withdrawal eligibility
                LockInDays = summaryDto.LockInDays,
                EligibleWithdrawDate = summaryDto.EligibleWithdrawDate,
                OkToWithdraw = summaryDto.OkToWithdraw,
                Prompt = summaryDto.Prompt
            };

            summaryDvm.Vintages = summaryDto.Vintages.Select(t => _mapper.Map<VintageDto, VintageViewModel>(t)).ToList();
            foreach (var dto in summaryDvm.Vintages) {
                dto.InvestmentAmount = DbExpressions.RoundCustomerBalanceAmount(dto.InvestmentAmount * usdToUserRate);
            }

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
                .GetCustomerVintagesSellingValue(user.Id);

            // Convert to User currency
            foreach (var dto in customerVintages) {
                dto.InvestmentAmount = DbExpressions.RoundCustomerBalanceAmount(dto.InvestmentAmount*usdToUserRate);
                dto.SellingValue = DbExpressions.RoundCustomerBalanceAmount(dto.SellingValue*usdToUserRate);
            }

            var vintages = customerVintages.Select(t => _mapper.Map<VintageDto, VintageViewModel>(t)).ToList();
            return vintages;
        }

        /// <summary>
        /// 
        /// Post and sell the selected vintages.
        /// 
        /// </summary>
        /// <param name="vintages"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult WithdrawVintages(IList<VintageViewModel> vintages) {
            var vintagesDtos = vintages.Select(v =>
                _mapper.Map<VintageViewModel, VintageDto>(v))
                .ToList();

            var user = UserManager.FindById(User.Identity.GetUserId<int>());
            var userId = user.Id;

            // Get user currency rate
            CurrencyInfo userCurrency;
            decimal usdToUserRate = GetUserCurrencyRate(user, out userCurrency);

            // Sell Vintages
            var updatedVintages = SaveDbSellVintages(userId, vintagesDtos);

            var inUserRateVintages =
            updatedVintages.Select(v => new VintageViewModel() {
                 YearMonthStr = v.YearMonthStr,
                 InvestmentAmount = DbExpressions.RoundCustomerBalanceAmount(v.InvestmentAmount * usdToUserRate),
                 SellingValue = DbExpressions.RoundCustomerBalanceAmount(v.SellingValue * usdToUserRate),
                 Locked = v.Locked,
                 Sold = v.Sold
             }).ToList();

            return OkMsg(inUserRateVintages);
        }

        /// <summary>
        /// 
        /// Public interface unit test friendly method.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <returns></returns>
        public ICollection<VintageDto> SaveDbSellVintages(int customerId, ICollection<VintageDto> vintages) {

            var updatedVintages = _invBalanceRepo.SaveDbSellVintages(customerId, vintages);

            return updatedVintages;
        }

        #endregion

        #region Portfolio
        [HttpGet]
        public IHttpActionResult GetPortfolioData()
        {
            var user = UserManager.FindById(User.Identity.GetUserId<int>());
            if (user == null)
                return OkMsg(new object(), "User not found!");

            CurrencyInfo userCurrency;
            decimal usdToUserRate = GetUserCurrencyRate(user, out userCurrency);
            var investmentAmount = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate*user.LastInvestmentAmount);

            var now = DateTime.Now;
            var model = new PortfolioDataViewModel
                        {
                            NextInvestmentOn = DbExpressions.GetNextMonthsFirstWeekday(),
                            NextExpectedInvestment = investmentAmount,
                            Plans = GetCustomerPlans(user.Id, investmentAmount)
                        };
            return OkMsg(model);
        }

        [HttpPost]
        public IHttpActionResult SetPlanSelection(PlanViewModel plan)
        {
            return OkMsg(() =>
            {
                var user = UserManager.FindById(User.Identity.GetUserId<int>());
                if (user == null)
                    return OkMsg(new object(), "User not found!");
                return OkMsg(() => _custPortfolioRepo.SaveDbCustomerSelectNextMonthsPortfolio(user.Id, plan.Risk));
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

            var userCurrency = CurrencyHelper.GetSymbol(user.Currency);
            var usdToUserRate = _currencyRateRepo.GetLastCurrencyRateFromUSD(userCurrency.ISOSymbol);

            var model = new PerformanceDataViewModel
            {
                InvestmentsBalance = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * user.InvBalance),
                NextExpectedInvestment = DbExpressions.RoundCustomerBalanceAmount(usdToUserRate * user.LastInvestmentAmount),
                Plans = GetCustomerPlans(user.Id)
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

        #region Methods
        public IEnumerable<PlanViewModel> GetCustomerPlans(int customerId, decimal nextInvestAmount = 0) {

            var portfolioDtos = _custPortfolioRepo.GetCustomerPlans(customerId);
            var portfolios = portfolioDtos
                .Select(p => new PlanViewModel() {
                    Id = p.Id,
                    Title = p.Title,
                    Color = p.Color,
                    AllocatedPercent = p.AllocatedPercent,
                    AllocatedAmount = p.AllocatedAmount,
                    ROI = p.ROI,
                    Risk = p.Risk,
                    Selected = p.Selected,
                    Holdings = p.Holdings
                        .Select(h=> new HoldingViewModel() {
                            Name = h.Name,
                            Weight = h.Weight
                        })
                })
                .ToList();

            return portfolios;
        }
        #endregion
        
        #region Fields
        private readonly IMapper _mapper;
        private readonly IInvBalanceRepo _invBalanceRepo;
        private readonly IGzTransactionRepo _gzTransactionRepo;
        private readonly ICurrencyRateRepo _currencyRateRepo;
        private readonly ICustFundShareRepo _custFundShareRepo;
        private readonly ICustPortfolioRepo _custPortfolioRepo;
        private readonly IUserRepo _userRepo;
        private readonly ApplicationDbContext _dbContext;
        #endregion

        #region Dummy Data
        
        private static IList<VintageViewModel> _dummyVintages = new[]
        {
            new VintageViewModel {YearMonthStr = "201407", InvestmentAmount = 100M, SellingValue = 102M, Locked = true },
            new VintageViewModel {YearMonthStr = "201408", InvestmentAmount = 100M, SellingValue = 109M, Locked = true },
            new VintageViewModel {YearMonthStr = "201409", InvestmentAmount = 100M, SellingValue = 107M, Locked = true },
            new VintageViewModel {YearMonthStr = "201410", InvestmentAmount = 100M, SellingValue = 112M, Locked = true },
            new VintageViewModel {YearMonthStr = "201411", InvestmentAmount = 100M, SellingValue = 111M, Locked = true },
            new VintageViewModel {YearMonthStr = "201412", InvestmentAmount = 100M, SellingValue = 106M },
            new VintageViewModel {YearMonthStr = "201501", InvestmentAmount = 100M, SellingValue = 107M },
            new VintageViewModel {YearMonthStr = "201502", InvestmentAmount = 100M, SellingValue = 103M },
            new VintageViewModel {YearMonthStr = "201503", InvestmentAmount = 100M, SellingValue = 105M, Sold = true},
            new VintageViewModel {YearMonthStr = "201504", InvestmentAmount = 100M, SellingValue = 105M },
            new VintageViewModel {YearMonthStr = "201505", InvestmentAmount = 100M, SellingValue = 102M },
            new VintageViewModel {YearMonthStr = "201506", InvestmentAmount = 100M, SellingValue = 106M },
            new VintageViewModel {YearMonthStr = "201507", InvestmentAmount = 100M, SellingValue = 107M, Sold = true },
            new VintageViewModel {YearMonthStr = "201508", InvestmentAmount = 100M, SellingValue = 109M },
            new VintageViewModel {YearMonthStr = "201509", InvestmentAmount = 100M, SellingValue = 100M },
            new VintageViewModel {YearMonthStr = "201510", InvestmentAmount = 100M, SellingValue = 106M },
            new VintageViewModel {YearMonthStr = "201511", InvestmentAmount = 100M, SellingValue = 108M, Sold = true },
            new VintageViewModel {YearMonthStr = "201512", InvestmentAmount = 100M, SellingValue = 105M },
            new VintageViewModel {YearMonthStr = "201601", InvestmentAmount = 200M, SellingValue = 204M },
            new VintageViewModel {YearMonthStr = "201602", InvestmentAmount = 80M, SellingValue = 93M },
            new VintageViewModel {YearMonthStr = "201603", InvestmentAmount = 150M, SellingValue = 158M },
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
            ROI = 5,
            Color = "#227B46",
            Holdings = _dummyHoldings,
            Risk = RiskToleranceEnum.High
        };
        private static PlanViewModel _dummyModerate = new PlanViewModel()
        {
            Title = "Moderate",
            Selected = true,
            ROI = 3.5,
            Color = "#64BF89",
            Holdings = _dummyHoldings,
            Risk = RiskToleranceEnum.Medium
        };
        private static PlanViewModel _dummyConservative = new PlanViewModel()
        {
            Title = "Conservative",
            Selected = false,
            ROI = 2,
            Color = "#B4DCC4",
            Holdings = _dummyHoldings,
            Risk = RiskToleranceEnum.Low
        };
        private static IList<PlanViewModel> _dummyPlans = new[] { _dummyConservative, _dummyModerate, _dummyAggressive };
        #endregion
    }
}

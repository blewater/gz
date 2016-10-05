using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using AutoMapper;
using gzDAL;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using gzWeb.Contracts;
using gzWeb.Models;
using Microsoft.AspNet.Identity;
using Z.EntityFramework.Plus;

namespace gzWeb.Controllers {
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
                ApplicationUserManager userManager) : base(userManager)
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
        public async Task<IHttpActionResult> GetSummaryData()
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("GetSummaryData requested for [User#{0}]", userId);
            
            var summaryRes = await _userRepo.GetSummaryDataAsync(userId);
            var user = summaryRes.Item2;
            var summaryDto = summaryRes.Item1;

            if (user == null)
            {
                Logger.Error("User not found [User#{0}]", userId);
                return OkMsg(new object(), "User not found!");
            }

            return OkMsg(((IInvestmentsApi) this).GetSummaryData(user, summaryDto));
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
            var usdToUserRate = GetUserCurrencyRate(user);

            var summaryDvm = new SummaryDataViewModel
                             {
                                     InvestmentsBalance =
                                             DbExpressions.RoundCustomerBalanceAmount(usdToUserRate*
                                                                                      summaryDto.InvestmentsBalance),
                                     TotalDeposits =
                                             DbExpressions.RoundCustomerBalanceAmount(usdToUserRate*
                                                                                      summaryDto.TotalDeposits),
                                     TotalWithdrawals =
                                             DbExpressions.RoundCustomerBalanceAmount(usdToUserRate*
                                                                                      summaryDto.TotalWithdrawals),
                                     TotalInvestments =
                                             DbExpressions.RoundCustomerBalanceAmount(usdToUserRate*
                                                                                      summaryDto.TotalInvestments),
                                     TotalInvestmentsReturns =
                                             DbExpressions.RoundCustomerBalanceAmount(usdToUserRate*
                                                                                      summaryDto.TotalInvestmentsReturns),
                                     NextInvestmentOn = DbExpressions.GetNextMonthsFirstWeekday(),
                                     LastInvestmentAmount =
                                             DbExpressions.RoundCustomerBalanceAmount(usdToUserRate*
                                                                                      summaryDto.LastInvestmentAmount),
                                     StatusAsOf = summaryDto.StatusAsOf,
                                     LockInDays = summaryDto.LockInDays,
                                     EligibleWithdrawDate = summaryDto.EligibleWithdrawDate,
                                     OkToWithdraw = summaryDto.OkToWithdraw,
                                     Prompt = summaryDto.Prompt,
                                     Vintages = summaryDto.Vintages.Select(t =>
                                                                           _mapper
                                                                                   .Map<VintageDto, VintageViewModel>(t))
                                                          .ToList()
                             };

            foreach (var vm in summaryDvm.Vintages)
            {
                vm.InvestmentAmount = DbExpressions.RoundCustomerBalanceAmount(vm.InvestmentAmount*usdToUserRate);
                vm.SellingValue = DbExpressions.RoundCustomerBalanceAmount(vm.SellingValue*usdToUserRate);
            }

            return summaryDvm;
        }

        /// <summary>
        /// 
        /// Get the user currency and exchange rate from dollar to user.
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private decimal GetUserCurrencyRate(ApplicationUser user)
        {
            var userCurrency = CurrencyHelper.GetSymbol(user.Currency);
            var usdToUserRate = _currencyRateRepo.GetLastCurrencyRateFromUSD(userCurrency.ISOSymbol);

            return usdToUserRate;
        }

        /// <summary>
        /// 
        /// HttpPost the Vintages to have their Selling Values calculated.
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> GetVintagesWithSellingValues(IList<VintageViewModel> vintages)
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("GetVintagesWithSellingValues requested for [User#{0}]", userId);

            var user = await _userRepo.GetCachedUserAsync(userId);
            if (user == null)
            {
                Logger.Error("User not found [User#{0}].", userId);
                return OkMsg(new object(), "User not found!");
            }

            var userVintages = GetVintagesSellingValuesByUser(user, vintages);

            return OkMsg(() => userVintages);
        }

        /// <summary>
        /// 
        /// Get the customer vintages with their selling value converted to the user currency.
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Obsolete]
        public IEnumerable<VintageViewModel> GetVintagesSellingValuesByUser(ApplicationUser user)
        {
            Logger.Trace("GetVintagesSellingValuesByUser requested for [User#{0}]", user.Id);
            var usdToUserRate = GetUserCurrencyRate(user);

            var customerVintages = _invBalanceRepo
                    .GetCustomerVintagesSellingValue(user.Id);

            // Convert to User currency
            foreach (var dto in customerVintages)
            {
                dto.InvestmentAmount = DbExpressions.RoundCustomerBalanceAmount(dto.InvestmentAmount*usdToUserRate);
                dto.SellingValue = DbExpressions.RoundCustomerBalanceAmount(dto.SellingValue*usdToUserRate);
            }

            var vintages = customerVintages
                    .Select(t => _mapper.Map<VintageDto, VintageViewModel>(t)).ToList();
            return vintages;
        }

        /// <summary>
        /// 
        /// Get the customer vintages with their selling value converted to the user currency.
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IEnumerable<VintageViewModel> GetVintagesSellingValuesByUser(ApplicationUser user,
                                                                            IList<VintageViewModel> vintagesVM)
        {
            var usdToUserRate = GetUserCurrencyRate(user);

            var vintageDtos = vintagesVM
                    .Select(t => _mapper.Map<VintageViewModel, VintageDto>(t)).ToList();

            var customerVintages = _invBalanceRepo
                    .GetCustomerVintagesSellingValue(user.Id, vintageDtos);

            // Convert to User currency
            foreach (var dto in customerVintages)
            {
                dto.InvestmentAmount = DbExpressions.RoundCustomerBalanceAmount(dto.InvestmentAmount*usdToUserRate);
                dto.SellingValue = DbExpressions.RoundCustomerBalanceAmount(dto.SellingValue*usdToUserRate);
            }

            var vintagesVmRet = vintageDtos
                    .Select(t => _mapper.Map<VintageDto, VintageViewModel>(t)).ToList();

            return vintagesVmRet;
        }

        /// <summary>
        /// 
        /// Post and sell the selected vintages.
        /// 
        /// </summary>
        /// <param name="vintages"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult> WithdrawVintages(IList<VintageViewModel> vintages)
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("GetVintagesSellingValuesByUser requested for [User#{0}]", userId);

            var vintagesDtos = vintages
                    .Select(v => _mapper.Map<VintageViewModel, VintageDto>(v))
                    .ToList();
            
            // Sell Vintages
            var updatedVintages = SaveDbSellVintages(userId, vintagesDtos);

            // Handle Response
            var user = await _userRepo.GetCachedUserAsync(userId);

            // Get user currency rate
            var usdToUserRate = GetUserCurrencyRate(user);

            var inUserRateVintages =
                    updatedVintages.AsParallel().Select(v => new VintageViewModel()
                                                             {
                                                                     YearMonthStr = v.YearMonthStr,
                                                                     InvestmentAmount =
                                                                             DbExpressions.RoundCustomerBalanceAmount(
                                                                                     v.InvestmentAmount*usdToUserRate),
                                                                     SellingValue =
                                                                             DbExpressions.RoundCustomerBalanceAmount(
                                                                                     v.SellingValue*usdToUserRate),
                                                                     Locked = v.Locked,
                                                                     Sold = v.Sold
                                                             }).ToList();

            return OkMsg(inUserRateVintages);
        }

        /// <summary>
        /// 
        /// Interface call fro selling vintage
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <param name="bypassQueue">True for unit tests</param>
        /// <returns></returns>
        public ICollection<VintageDto> SaveDbSellVintages(
                int customerId,
                ICollection<VintageDto> vintages,
                bool bypassQueue = false)
        {

            if (!bypassQueue)
            {
                // Compatible only with IIS hosting
                HostingEnvironment.QueueBackgroundWorkItem(
                        ct =>
                        _invBalanceRepo.SaveDbSellVintages(customerId, vintages));
            }
            else
            {
                _invBalanceRepo.SaveDbSellVintages(customerId, vintages);
            }

            // Presume the intended vintages were sold
            foreach (var dto in vintages.Where(v => v.Selected))
            {
                dto.Sold = true;
            }

            return vintages;
        }

        #endregion

        #region Portfolio

        [HttpGet]
        public async Task<IHttpActionResult> GetPortfolioData()
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("GetPortfolioData requested for [User#{0}]", userId);
            var user = await _userRepo.GetCachedUserAsync(userId);
            if (user == null)
            {
                Logger.Error("User not found [User#{0}]", userId);
                return OkMsg(new object(), "User not found!");
            }

            var usdToUserRate = GetUserCurrencyRate(user);
            var investmentAmount =
                    DbExpressions.RoundCustomerBalanceAmount(usdToUserRate*
                                                             _gzTransactionRepo.LastInvestmentAmount(user.Id,
                                                                                                     DateTime.UtcNow
                                                                                                             .ToStringYearMonth
                                                                                                             ()));

            var model = new PortfolioDataViewModel
                        {
                                NextInvestmentOn = DbExpressions.GetNextMonthsFirstWeekday(),
                                NextExpectedInvestment = investmentAmount,
                                Plans = await GetCustomerPlansAsync(user.Id, investmentAmount)
                        };
            return OkMsg(model);
        }

        [HttpPost]
        public async Task<IHttpActionResult> SetPlanSelection(PlanViewModel plan)
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("SetPlanSelection requested for [User#{0}]", userId);
            var user = await _userRepo.GetCachedUserAsync(userId);
            if (user == null)
            {
                Logger.Error("User not found [User#{0}]", userId);
                return OkMsg(new object(), "User not found!");
            }

            return OkMsg(() =>
                         {
                             return
                                     OkMsg(
                                             () =>
                                             _custPortfolioRepo.SaveDbCustomerSelectNextMonthsPortfolio(user.Id,
                                                                                                        plan.Risk));
                         });
        }

        #endregion

        #region Performance

        [HttpGet]
        public async Task<IHttpActionResult> GetPerformanceData()
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("SetPlanSelection requested for [User#{0}]", userId);
            var user = await _userRepo.GetCachedUserAsync(userId);
            if (user == null)
            {
                Logger.Error("User not found [User#{0}]", userId);
                return OkMsg(new object(), "User not found!");
            }

            var invBalanceRes =
                    await
                    _invBalanceRepo.GetCachedLatestBalanceTimestampAsync(_invBalanceRepo.CacheLatestBalanceAsync(user.Id));


            var userCurrency = CurrencyHelper.GetSymbol(user.Currency);
            var usdToUserRate = _currencyRateRepo.GetLastCurrencyRateFromUSD(userCurrency.ISOSymbol);

            var model = new PerformanceDataViewModel
                        {
                                InvestmentsBalance =
                                        DbExpressions.RoundCustomerBalanceAmount(usdToUserRate*invBalanceRes.Item1),
                                NextExpectedInvestment =
                                        DbExpressions.RoundCustomerBalanceAmount(usdToUserRate*
                                                                                 _gzTransactionRepo.LastInvestmentAmount
                                                                                         (user.Id,
                                                                                          DateTime.UtcNow
                                                                                                  .ToStringYearMonth())),
                                Plans = await GetCustomerPlansAsync(user.Id)
                        };
            return OkMsg(model);
        }

        #endregion

        #region Activity

        #endregion

        [HttpGet]
        public IHttpActionResult GetPortfolios()
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("GetPortfolios requested for [User#{0}]", userId);

            return OkMsg(() =>
                         {
                             var portfolios =
                                     _dbContext.Portfolios
                                               .Where(x => x.IsActive)
                                               .Select(x => new
                                                            {
                                                                    x.Id,
                                                                    x.RiskTolerance,
                                                                    Funds =
                                                                    x.PortFunds.Select(
                                                                            f => new {f.Fund.HoldingName, f.Weight})
                                                            })
                                               .FromCacheAsync(DateTime.UtcNow.AddDays(1))
                                               .Result;
                             return portfolios;
                         });
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<PlanViewModel>> GetCustomerPlansAsync(int customerId, decimal nextInvestAmount = 0)
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("GetCustomerPlansAsync requested for [User#{0}]", userId);

            var portfolioDtos = await _custPortfolioRepo.GetCustomerPlansAsync(customerId);
            var portfolios = portfolioDtos
                    .Select(p => new PlanViewModel()
                                 {
                                         Id = p.Id,
                                         Title = p.Title,
                                         Color = p.Color,
                                         AllocatedPercent = p.AllocatedPercent,
                                         AllocatedAmount = p.AllocatedAmount,
                                         ROI = p.ROI,
                                         Risk = p.Risk,
                                         Selected = p.Selected,
                                         Holdings = p.Holdings
                                                     .Select(h => new HoldingViewModel()
                                                                  {
                                                                          Name = h.Name,
                                                                          Weight = h.Weight
                                                                  })
                                 })
                    .OrderBy(x => x.Risk)
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
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion
    }
}


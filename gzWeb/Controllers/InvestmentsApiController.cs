using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Threading.Tasks;
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
using Microsoft.AspNet.Identity;
using Z.EntityFramework.Plus;
using System.Configuration;
using System.Web.Hosting;

namespace gzWeb.Controllers {
    [Authorize]
    // TODO: [RoutePrefix("api/Investments")]
    public class InvestmentsApiController : BaseApiController, IInvestmentsApi
    {
        #region Constructor

        public InvestmentsApiController(
                ApplicationDbContext inDbContext,
                IInvBalanceRepo inInvBalanceRepo,
                IGzTransactionRepo inGzTransactionRepo,
                IUserPortfolioRepo inUserPortfolioRepo,
                IUserRepo inUserRepo,
                IMapper inMapper,
                ApplicationUserManager inUserManager) : base(inUserManager)
        {
            this.dbContext = inDbContext;
            this.invBalanceRepo = inInvBalanceRepo;
            this.gzTransactionRepo = inGzTransactionRepo;
            this.userPortfolioRepo = inUserPortfolioRepo;
            this.userRepo = inUserRepo;
            this.mapper = inMapper;
        }

        #endregion

        #region Actions

        #region Summary

        [HttpGet]
        public async Task<IHttpActionResult> GetSummaryData()
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("GetSummaryData requested for [User#{0}]", userId);
            
            var summaryRes = await invBalanceRepo.GetSummaryDataAsync(userId);
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
            var summaryDvm = new SummaryDataViewModel
            {
                    InvestmentsBalance =
                            DbExpressions.RoundCustomerBalanceAmount(summaryDto.InvestmentsBalance),
                    TotalInvestments =
                            DbExpressions.RoundCustomerBalanceAmount(summaryDto.TotalInvestments),
                    TotalInvestmentsReturns =
                            DbExpressions.RoundCustomerBalanceAmount(summaryDto.TotalInvestmentsReturns),
                    NextInvestmentOn = DbExpressions.GetNextMonthsFirstWeekday(),
                    LastInvestmentAmount =
                            DbExpressions.RoundCustomerBalanceAmount(summaryDto.LastInvestmentAmount),
                    StatusAsOf = summaryDto.StatusAsOf,
                    LockInDays = summaryDto.LockInDays,
                    EligibleWithdrawDate = summaryDto.EligibleWithdrawDate,
                    OkToWithdraw = summaryDto.OkToWithdraw,
                    Prompt = summaryDto.Prompt,
                    Vintages = 
                        summaryDto
                        .Vintages
                        .Select(
                            t =>
                                mapper
                                .Map<VintageDto, VintageViewModel>(t))
                                .ToList(),
                    BegGmBalance = Math.Round(summaryDto.BegGmBalance, 1),
                    EndGmBalance = Math.Round(summaryDto.EndGmBalance, 1),
                    Deposits = Math.Round(summaryDto.Deposits, 1),
                    GmGainLoss = Math.Round(summaryDto.GmGainLoss, 1),
                    Withdrawals = Math.Round(summaryDto.Withdrawals, 1),
                    CashBonus = Math.Round(summaryDto.CashBonus, 1)
            };

            foreach (var vm in summaryDvm.Vintages)
            {
                vm.InvestmentAmount = DbExpressions.RoundCustomerBalanceAmount(vm.InvestmentAmount);
                vm.SellingValue = DbExpressions.RoundCustomerBalanceAmount(vm.SellingValue);
            }

            return summaryDvm;
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

            var user = await userRepo.GetCachedUserAsync(userId);
            if (user == null)
            {
                Logger.Error("User not found [User#{0}].", userId);
                return OkMsg(new object(), "User not found!");
            }

            var vintagesWithSoldAmounts = GetVintagesSellingValuesByUser(user, vintages);

            return OkMsg( () => vintagesWithSoldAmounts );
        }

        /// <summary>
        /// 
        /// Get vintages ready for UI
        /// 
        /// </summary>
        /// <param name="vintageDtos"></param>
        /// <returns></returns>
        private List<VintageViewModel> GetVintagesDto2Vm(List<VintageDto> vintageDtos)
        {
            // Round amounts according to business rule
            foreach (var dto in vintageDtos)
            {
                dto.InvestmentAmount = DbExpressions.RoundCustomerBalanceAmount(dto.InvestmentAmount);
                dto.SellingValue = DbExpressions.RoundCustomerBalanceAmount(dto.SellingValue);
            }

            var vintagesVmRet = vintageDtos
                .Select(t => mapper.Map<VintageDto, VintageViewModel>(t)).ToList();
            return vintagesVmRet;
        }

        /// <summary>
        /// 
        /// ** Unit Test Helper **
        /// 
        /// Get the customer vintages with their selling value converted to the user currency.
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<List<VintageViewModel>> GetVintagesSellingValuesByUserTestHelper(ApplicationUser user)
        {
            Logger.Trace("GetVintagesSellingValuesByUser requested for [User#{0}]", user.Id);

            var vintagesValuedAtPresentValue = 
                await invBalanceRepo
                    .GetCustomerVintagesSellingValueUnitTestHelper(user.Id);

            var vintagesVmRet = GetVintagesDto2Vm(vintagesValuedAtPresentValue);

            return vintagesVmRet;
        }

        /// <summary>
        /// 
        /// Get the customer vintages with their selling value converted to the user currency.
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="vintagesVm"></param>
        /// <returns></returns>
        private VintagesWithSellingValuesViewModel GetVintagesSellingValuesByUser(ApplicationUser user, IList<VintageViewModel> vintagesVm)
        {
            var vintageDtos = vintagesVm
                    .Select(t => mapper.Map<VintageViewModel, VintageDto>(t))
                    .ToList();

            var vintagesWithPastSoldAmounts = 
                invBalanceRepo
                .GetCustomerVintagesSellingValueNow(user.Id, vintageDtos);
            
            var vintagesValuedAtPresentValueDto = vintagesWithPastSoldAmounts.VintagesDtos;

            var vintagesVmRet = GetVintagesDto2Vm(vintagesValuedAtPresentValueDto);

            return new VintagesWithSellingValuesViewModel() {
                Vintages = vintagesVmRet,
                WithdrawnAmount = vintagesWithPastSoldAmounts.SoldVintagesAmounts.TotalSoldVintagesNetProceeds,
                WithdrawnFeesAmount = vintagesWithPastSoldAmounts.SoldVintagesAmounts.TotalSoldVintagesFeesAmount
            };
        }

        /// <summary>
        /// 
        /// Post and sell the selected vintages.
        /// 
        /// </summary>
        /// <param name="vintages"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult WithdrawVintages(IList<VintageViewModel> vintages)
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("GetVintagesSellingValuesByUser requested for [User#{0}]", userId);

            var vintagesDtos = vintages
                .Select(v => mapper.Map<VintageViewModel, VintageDto>(v))
                .ToList();

            var queueAzureConnString = ConfigurationManager.AppSettings["QueueAzureConnString"];
            var queueName = ConfigurationManager.AppSettings["QueueName"];
            var emailAdmins = ConfigurationManager.AppSettings["WithdrawalBonunAdmins"];
            var gmailUser = ConfigurationManager.AppSettings["gmailHostMaster"];
            var gmailPwd = ConfigurationManager.AppSettings["gmailHostPwd"];

            // Sell Vintages
            var updatedVintages = 
                    invBalanceRepo
                        .SaveDbSellAllSelectedVintagesInTransRetry(
                            userId,
                            vintagesDtos,
                            true,
                            queueAzureConnString,
                            queueName,
                            emailAdmins,
                            gmailUser,
                            gmailPwd
                        );

            var vmVintagesAfterWithdrawals =
                updatedVintages
                    .Select(v =>
                        mapper
                        .Map<VintageDto, VintageViewModel>(v)
                    );
            foreach (var v in vmVintagesAfterWithdrawals) {
                v.InvestmentAmount = DbExpressions.RoundCustomerBalanceAmount(v.InvestmentAmount);
                v.SellingValue = DbExpressions.RoundCustomerBalanceAmount(v.SellingValue);
            }

            return OkMsg(vmVintagesAfterWithdrawals);
        }

        #endregion

        #region Portfolio

        [HttpGet]
        public async Task<IHttpActionResult> GetPortfolioData()
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("GetPortfolioData requested for [User#{0}]", userId);
            var user = await userRepo.GetCachedUserAsync(userId);
            if (user == null)
            {
                Logger.Error("User not found [User#{0}]", userId);
                return OkMsg(new object(), "User not found!");
            }

            var investmentAmount =
                    DbExpressions.RoundCustomerBalanceAmount(gzTransactionRepo.LastInvestmentAmount(user.Id,
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
            var user = await userRepo.GetCachedUserAsync(userId);
            if (user == null)
            {
                Logger.Error("User not found [User#{0}]", userId);
                return OkMsg(new object(), "User not found!");
            }

            return 
                OkMsg(() =>
                {
                    return
                        OkMsg(
                            () =>
                            userPortfolioRepo.SetDbDefaultPortfolio(user.Id, plan.Risk));
                });
        }

        #endregion

        #region Performance

        [HttpGet]
        public async Task<IHttpActionResult> GetPerformanceData()
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("SetPlanSelection requested for [User#{0}]", userId);
            var user = await userRepo.GetCachedUserAsync(userId);
            if (user == null)
            {
                Logger.Error("User not found [User#{0}]", userId);
                return OkMsg(new object(), "User not found!");
            }

            var invBalance =
                invBalanceRepo
                    .GetLatestBalanceDto(
                        await
                            invBalanceRepo.GetCachedLatestBalanceAsync(userId)
                    ).Balance;

            var lastInvestmentAmount =
                gzTransactionRepo.LastInvestmentAmount
                (user.Id,
                    DateTime.UtcNow
                        .ToStringYearMonth());

            var model = new PerformanceDataViewModel
            {
                    InvestmentsBalance = // Don't include the month's loss amount in the current inv balance
                            DbExpressions.RoundCustomerBalanceAmount(invBalance - lastInvestmentAmount),
                    NextExpectedInvestment =
                            DbExpressions.RoundCustomerBalanceAmount(lastInvestmentAmount),
                    Plans = await GetCustomerPlansAsync(user.Id)
            };
            return OkMsg(model);
        }

        #endregion

        #region Activity

        #endregion

        [HttpGet]
        public async Task<IHttpActionResult> GetPortfolios()
        {
            var userId = User.Identity.GetUserId<int>();
            Logger.Trace("GetPortfolios requested for [User#{0}]", userId);

            var portfolios =
                await dbContext.Portfolios
                    .Where(x => x.IsActive)
                    .Select(x => new {
                        x.Id,
                        x.RiskTolerance,
                        Funds =
                            x.PortFunds.Select(
                                f => new {f.Fund.HoldingName, f.Weight})
                    })
                    .FromCacheAsync(DateTime.UtcNow.AddDays(1));

            return OkMsg(portfolios);
        }

        #endregion

        #region Methods

        public async Task<List<PlanViewModel>> GetCustomerPlansAsync(int userId, decimal nextInvestAmount = 0)
        {
            var portfolioDtos = await userPortfolioRepo.GetUserPlansAsync(userId);
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
                                                                    .ToList()
                                 })
                    .OrderBy(x => x.Risk)
                    .ToList();

            return portfolios;
        }

        #endregion

        #region Fields

        private readonly IMapper mapper;
        private readonly IInvBalanceRepo invBalanceRepo;
        private readonly IGzTransactionRepo gzTransactionRepo;
        private readonly IUserPortfolioRepo userPortfolioRepo;
        private readonly IUserRepo userRepo;
        private readonly ApplicationDbContext dbContext;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        #endregion
    }
}


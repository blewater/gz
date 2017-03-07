using System;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using Z.EntityFramework.Plus;
using NLog;
using static System.Diagnostics.Debug;

namespace gzDAL.Repos
{
    public class UserRepo : IUserRepo
    {
        private readonly ApplicationDbContext _db;
        private readonly IGzTransactionRepo _gzTransactionRepo;
        private readonly IInvBalanceRepo _invBalanceRepo;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// Constructor
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="gzTransactionRepo"></param>
        /// <param name="invBalanceRepo"></param>
        public UserRepo(ApplicationDbContext db, 
            IGzTransactionRepo gzTransactionRepo, 
            IInvBalanceRepo invBalanceRepo)
        {
            this._db = db;
            this._gzTransactionRepo = gzTransactionRepo;
            this._invBalanceRepo = invBalanceRepo;
        }

        /// <summary>
        ///  
        /// Cache & query user asynchronously
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private Task<ApplicationUser> CacheUser(int userId) {

                var userQtask = _db.Users
                    .Where(u => u.Id == userId)
                    .DeferredSingleOrDefault()
                    .FromCacheAsync(DateTime.UtcNow.AddDays(1));

                return userQtask;
        }

        /// <summary>
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="userTask"></param>
        /// <returns></returns>
        private async Task<ApplicationUser> GetCachedUserAsync(Task<ApplicationUser> userTask) {

            var userRow = await userTask;
            return userRow;
        }

        public async Task<ApplicationUser> GetCachedUserAsync(int userId) {
            var task = GetCachedUserAsync(CacheUser(userId));

            return await task;
        }

        /// <summary>
        /// 
        /// Perform all the Summary SQL Queries in a optimized fashion to fill up the summary DTO object.
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Tuple<UserSummaryDTO, ApplicationUser>> GetSummaryDataAsync(int userId) {

            ApplicationUser userRet = null;
            UserSummaryDTO summaryDtoRet = null;

            try {

                //--------------- Start async queries
                var userQtask = CacheUser(userId);
                var latestBalanceTask = _invBalanceRepo.CacheLatestBalanceAsync(userId);

                //---------------- Execute SQL Functions
                var totalPlayerLossesAmount = await _gzTransactionRepo.GetTotalPlayerLossesAmountAsync(userId);

                var vintages = _invBalanceRepo.GetCustomerVintages(userId);

                var lastInvestmentAmount = await _gzTransactionRepo.GetLastInvestmentAmountAsync(userId);

                var withdrawalEligibility = await _gzTransactionRepo.GetWithdrawEligibilityDataAsync(userId);
                //-------------- Retrieve previously executed async query results

                // user
                userRet = await userQtask;
                if (userRet == null) {
                    _logger.Error("User with id {0} is null in GetSummaryData()", userId);
                }
                Assert(userRet != null);

                // balance, last update
                var invBalanceRes = await _invBalanceRepo.GetCachedLatestBalanceTimestampAsync(latestBalanceTask);

                // Package all the results
                summaryDtoRet = new UserSummaryDTO() {

                    Currency = CurrencyHelper.GetSymbol(userRet.Currency),
                    InvestmentsBalance = invBalanceRes.Balance, // balance

                    // Monthly gaming amounts
                    BegMonthlyGmBalance = invBalanceRes.BegGmBalance,
                    MonthlyDeposits = invBalanceRes.Deposits,
                    MonthlyWithdrawals = invBalanceRes.Withdrawals,
                    MonthlyGamingGainLoss = invBalanceRes.GmGainLoss,
                    EndMonthlyGmBalance = invBalanceRes.EndGmBalance,

                    // 
                    TotalInvestments = totalPlayerLossesAmount,

                    TotalInvestmentsReturns = invBalanceRes.Balance - totalPlayerLossesAmount,

                    NextInvestmentOn = DbExpressions.GetNextMonthsFirstWeekday(),
                    LastInvestmentAmount = lastInvestmentAmount,

                    //latestBalanceUpdateDatetime
                    StatusAsOf = invBalanceRes.UpdatedOnUtc ?? DateTime.UtcNow.AddDays(-1), 
                    Vintages = vintages,

                    // Withdrawal eligibility
                    LockInDays = withdrawalEligibility.LockInDays,
                    EligibleWithdrawDate = withdrawalEligibility.EligibleWithdrawDate,
                    OkToWithdraw = withdrawalEligibility.OkToWithdraw,
                    Prompt = withdrawalEligibility.Prompt
                };
            } catch (Exception ex) {
                _logger.Error(ex, "Exception in GetSummaryData()");
            }

            return Tuple.Create(summaryDtoRet, userRet);
        }
    }
}

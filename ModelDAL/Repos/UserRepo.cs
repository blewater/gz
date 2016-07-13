using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using gzDAL.ModelsUtil;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using Z.EntityFramework.Plus;
using NLog;

namespace gzDAL.Repos
{
    public class UserRepo : IUserRepo
    {
        private readonly ApplicationDbContext _db;
        private readonly IGzTransactionRepo _gzTransactionRepo;
        private readonly IInvBalanceRepo _invBalanceRepo;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public UserRepo(ApplicationDbContext db, 
            IGzTransactionRepo gzTransactionRepo, 
            IInvBalanceRepo invBalanceRepo)
        {
            this._db = db;
            this._gzTransactionRepo = gzTransactionRepo;
            this._invBalanceRepo = invBalanceRepo;
        }

        private Task<ApplicationUser> CacheUser(int userId) {

            var userQtask = _db.Users
                .Where(u => u.Id == userId)
                .DeferredSingleOrDefault()
                .FromCacheAsync(DateTime.UtcNow.AddDays(1));

            return userQtask;
        }
        private ApplicationUser GetCachedUser(Task<ApplicationUser> userTask) {

            var userRow = userTask.Result;
            return userRow;
        }

        public ApplicationUser GetCachedUser(int userId) {
            return GetCachedUser(CacheUser(userId));
        }

        /// <summary>
        /// 
        /// Perform all the Summary SQL Queries in a optimized fashion to fill up the summary DTO object.
        /// 
        /// </summary>
        /// <returns></returns>
        public UserSummaryDTO GetSummaryData(int userId, out ApplicationUser userRet) {

            userRet = null;
            UserSummaryDTO summaryDtoRet = null;

            try {

                //--------------- Start async queries
                var userQtask = CacheUser(userId);
                var latestBalanceTask = _invBalanceRepo.CacheLatestBalance(userId);
                var invGainLossTask = _invBalanceRepo.CacheInvestmentReturns(userId);

                //---------------- Execute SQL Functions
                decimal totalWithdrawalsAmount = _db.Database
                    .SqlQuery<decimal>("Select dbo.GetTotalTrxAmount(@customerId, @TrxType)",
                        new SqlParameter("@CustomerId", userId),
                        new SqlParameter("@TrxType", (int) GzTransactionTypeEnum.TransferToGaming))
                    .Single<decimal>();

                decimal totalInvestmentsAmount = _db.Database
                    .SqlQuery<decimal>("Select dbo.GetTotalTrxAmount(@CustomerId, @TrxType)",
                        new SqlParameter("@CustomerId", userId),
                        new SqlParameter("@TrxType", (int) GzTransactionTypeEnum.CreditedPlayingLoss))
                    .Single<decimal>();

                var vintages = _invBalanceRepo.GetCustomerVintages(userId);

                var thisYearMonthStr = DateTime.UtcNow.ToStringYearMonth();
                var lastInvestmentAmount = _db.Database
                    .SqlQuery<decimal>("Select Amount From dbo.GetMonthsTrxAmount(@CustomerId, @YearMonth, @TrxType)",
                        new SqlParameter("@CustomerId", userId),
                        new SqlParameter("@YearMonth", thisYearMonthStr),
                        new SqlParameter("@TrxType", (int) GzTransactionTypeEnum.CreditedPlayingLoss))
                    .SingleOrDefault();

                var withdrawalEligibility = _gzTransactionRepo.GetWithdrawEligibilityData(userId);

                var totalDeposits = _gzTransactionRepo.GetTotalDeposit(userId);

                //-------------- Retrieve previously executed async query results

                // user
                userRet = userQtask.Result;
                if (userRet == null) {
                    _logger.Error("User with id {0} is null in GetSummaryData()", userId);
                }

                // balance, last update
                DateTime? latestBalanceUpdateDatetime;
                var balance = _invBalanceRepo.GetCachedLatestBalanceTimestamp(latestBalanceTask,
                    out latestBalanceUpdateDatetime);

                // investment gain or loss
                var invGainLossSum = _invBalanceRepo.GetCachedInvestmentReturns(invGainLossTask);

                summaryDtoRet = new UserSummaryDTO() {

                    Currency = CurrencyHelper.GetSymbol(userRet.Currency),
                    InvestmentsBalance = balance,
                    TotalDeposits = totalDeposits,
                    TotalWithdrawals = totalWithdrawalsAmount,

                    TotalInvestments = totalInvestmentsAmount,

                    // TODO (Mario): Check if it's more accurate to report this as [InvestmentsBalance - TotalInvestments]
                    TotalInvestmentsReturns = invGainLossSum,

                    NextInvestmentOn = DbExpressions.GetNextMonthsFirstWeekday(),
                    LastInvestmentAmount = lastInvestmentAmount,
                    StatusAsOf = latestBalanceUpdateDatetime ?? DateTime.UtcNow.AddDays(-1),
                    Vintages = vintages,

                    // Withdrawal eligibility
                    LockInDays = withdrawalEligibility.LockInDays,
                    EligibleWithdrawDate = withdrawalEligibility.EligibleWithdrawDate,
                    OkToWithdraw = withdrawalEligibility.OkToWithdraw,
                    Prompt = withdrawalEligibility.Prompt
                };
            }
            catch (Exception ex) {
                _logger.Error(ex, "Exception in GetSummaryData()");
            }

            return summaryDtoRet;
        }
    }
}

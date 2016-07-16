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
                var latestBalanceTask = _invBalanceRepo.CacheLatestBalance(userId);
                var invGainLossTask = _invBalanceRepo.CacheInvestmentReturns(userId);

                //---------------- Execute SQL Functions
                decimal totalWithdrawalsAmount = await _db.Database
                    .SqlQuery<decimal>("Select dbo.GetTotalTrxAmount(@customerId, @TrxType)",
                        new SqlParameter("@CustomerId", userId),
                        new SqlParameter("@TrxType", (int)GzTransactionTypeEnum.TransferToGaming))
                    .SingleAsync();

                decimal totalInvestmentsAmount = await _db.Database
                    .SqlQuery<decimal>("Select dbo.GetTotalTrxAmount(@CustomerId, @TrxType)",
                        new SqlParameter("@CustomerId", userId),
                        new SqlParameter("@TrxType", (int)GzTransactionTypeEnum.CreditedPlayingLoss))
                    .SingleAsync();

                var vintages = _invBalanceRepo.GetCustomerVintages(userId);

                var thisYearMonthStr = DateTime.UtcNow.ToStringYearMonth();
                var lastInvestmentAmount = await _db.Database
                    .SqlQuery<decimal>("Select Amount From dbo.GetMonthsTrxAmount(@CustomerId, @YearMonth, @TrxType)",
                        new SqlParameter("@CustomerId", userId),
                        new SqlParameter("@YearMonth", thisYearMonthStr),
                        new SqlParameter("@TrxType", (int)GzTransactionTypeEnum.CreditedPlayingLoss))
                    .SingleOrDefaultAsync();

                var withdrawalEligibility = await _gzTransactionRepo.GetWithdrawEligibilityDataAsync(userId);


                var totalDeposits = _gzTransactionRepo.GetTotalDeposit(userId);

                //-------------- Retrieve previously executed async query results

                // user
                userRet = await userQtask;
                if (userRet == null) {
                    _logger.Error("User with id {0} is null in GetSummaryData()", userId);
                }

                // balance, last update
                var invBalanceRes = await _invBalanceRepo.GetCachedLatestBalanceTimestampAsync(latestBalanceTask);

                // investment gain or loss
                decimal invGainLossSum = await _invBalanceRepo.GetCachedInvestmentReturnsAsync(invGainLossTask);

                summaryDtoRet = new UserSummaryDTO() {

                    Currency = CurrencyHelper.GetSymbol(userRet.Currency),
                    InvestmentsBalance = invBalanceRes.Item1, // balance
                    TotalDeposits = totalDeposits,
                    TotalWithdrawals = totalWithdrawalsAmount,

                    TotalInvestments = totalInvestmentsAmount,

                    // TODO (Mario): Check if it's more accurate to report this as [InvestmentsBalance - TotalInvestments]
                    TotalInvestmentsReturns = invGainLossSum,

                    NextInvestmentOn = DbExpressions.GetNextMonthsFirstWeekday(),
                    LastInvestmentAmount = lastInvestmentAmount,

                    //latestBalanceUpdateDatetime
                    StatusAsOf = invBalanceRes.Item2 ?? DateTime.UtcNow.AddDays(-1), 
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

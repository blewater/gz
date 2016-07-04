using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using gzDAL.ModelsUtil;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using Z.EntityFramework.Plus;

namespace gzDAL.Repos
{
    public class UserRepo : IUserRepo
    {
        private readonly ApplicationDbContext _db;
        private readonly GzTransactionRepo _gzTransactionRepo;
        private readonly InvBalanceRepo _invBalanceRepo;
        private readonly ICurrencyRateRepo _currencyRateRepo;

        public UserRepo(ApplicationDbContext db, 
            GzTransactionRepo gzTransactionRepo, 
            InvBalanceRepo invBalanceRepo)
        {
            this._db = db;
            this._gzTransactionRepo = gzTransactionRepo;
            this._invBalanceRepo = invBalanceRepo;
        }

        /// <summary>
        /// 
        /// Perform all the Summary SQL Queries in a optimized fashion to fill up the summary DTO object.
        /// 
        /// </summary>
        /// <returns></returns>
        public UserSummaryDTO GetSummaryData(int userId, out ApplicationUser user) {

            var futureUser = _db.Users
                .Where(u => u.Id == userId)
                .DeferredSingleOrDefault()
                .FutureValue();

            var totalInvestmentReturns = _db.InvBalances
                .Where(b => b.CustomerId == userId)
                .Select(b => b.InvGainLoss)
                .DeferredSum()
                .FutureValue();

            var lastBalanceRow = _db.InvBalances
                .Where(i => i.CustomerId == userId)
                .OrderByDescending(i => i.YearMonth)
                .Select(b => new { b.UpdatedOnUTC, b.Balance })
                .DeferredFirstOrDefault()
                .FutureValue();

            decimal totalWithdrawalsAmount = _db.Database
                .SqlQuery<decimal>("Select dbo.GetTotalTrxAmount(@customerId, @TrxType)",
                    new SqlParameter("@CustomerId", userId),
                    new SqlParameter("@TrxType", (int)GzTransactionTypeEnum.TransferToGaming))
                .Single<decimal>();

            decimal totalInvestmentsAmount = _db.Database
                .SqlQuery<decimal>("Select dbo.GetTotalTrxAmount(@CustomerId, @TrxType)",
                    new SqlParameter("@CustomerId", userId),
                    new SqlParameter("@TrxType", (int)GzTransactionTypeEnum.CreditedPlayingLoss))
                .Single<decimal>();

            var vintages = _invBalanceRepo.GetCustomerVintages(userId);

            var thisYearMonthStr = DateTime.UtcNow.ToStringYearMonth();
            var lastInvestmentAmount = _db.Database
                .SqlQuery<decimal>("Select Amount From dbo.GetMonthsTrxAmount()",
                    new SqlParameter("@CustomerId", userId),
                    new SqlParameter("@YearMonth", thisYearMonthStr),
                    new SqlParameter("@TrxType", (int) GzTransactionTypeEnum.CreditedPlayingLoss))
                .SingleOrDefault();

            var withdrawalEligibility = _gzTransactionRepo.GetWithdrawEligibilityData(userId);

            var lastBalanceRowValue = lastBalanceRow.Value;

            user = futureUser.Value;

            var userSummaryDto = new UserSummaryDTO() {
                
                Currency = CurrencyHelper.GetSymbol(user.Currency),
                InvestmentsBalance = lastBalanceRowValue.Balance,
                TotalDeposits = _gzTransactionRepo.GetTotalDeposit(userId),
                TotalWithdrawals = totalWithdrawalsAmount,

                TotalInvestments = totalInvestmentsAmount,

                // TODO (Mario): Check if it's more accurate to report this as [InvestmentsBalance - TotalInvestments]
                TotalInvestmentsReturns = totalInvestmentReturns.Value,

                NextInvestmentOn = DbExpressions.GetNextMonthsFirstWeekday(),
                LastInvestmentAmount = lastInvestmentAmount,
                StatusAsOf = lastBalanceRowValue.UpdatedOnUTC,
                Vintages = vintages,

                // Withdrawal eligibility
                LockInDays = withdrawalEligibility.LockInDays,
                EligibleWithdrawDate = withdrawalEligibility.EligibleWithdrawDate,
                OkToWithdraw = withdrawalEligibility.OkToWithdraw,
                Prompt = withdrawalEligibility.Prompt
            };

            return userSummaryDto;
        }
    }
}

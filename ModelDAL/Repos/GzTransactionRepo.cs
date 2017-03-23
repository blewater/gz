using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq.Expressions;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using gzDAL.Models;
using Z.EntityFramework.Plus;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web.UI.WebControls.Expressions;
using NLog;

namespace gzDAL.Repos {

    /// <summary>
    /// 
    /// For any greenzorro Transaction creation/update
    /// Currency conversions are encapsulated here alone.
    /// 
    /// </summary>
    public class GzTransactionRepo : IGzTransactionRepo {
        private readonly ApplicationDbContext _db;
        private readonly IConfRepo confRepo;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public GzTransactionRepo(ApplicationDbContext db, IConfRepo confRepo) {
            this._db = db;
            this.confRepo = confRepo;
        }

        /// <summary>
        /// 
        /// Last pending loss to be invested
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearMonthStr"></param>
        /// <returns></returns>
        public decimal LastInvestmentAmount(int customerId, string yearMonthStr) {

            string key = "lastinvestmentamount" + customerId + yearMonthStr;
            var lastInvestmentAmount = (decimal?)MemoryCache.Default.Get(key);

            if (!lastInvestmentAmount.HasValue) {
                lastInvestmentAmount = _db.Database
                    .SqlQuery<decimal>("Select Amount From dbo.GetMonthsTrxAmount(@CustomerId, @YearMonth, @TrxType)",
                        new SqlParameter("@CustomerId", customerId),
                        new SqlParameter("@YearMonth", yearMonthStr),
                        new SqlParameter("@TrxType", (int) GzTransactionTypeEnum.CreditedPlayingLoss))
                    .SingleOrDefault();

                // 1 day cache
                MemoryCache
                    .Default
                    .Set(key, lastInvestmentAmount.Value, DateTimeOffset.UtcNow.AddDays(1));
            }
            return lastInvestmentAmount.Value;
        }

        /// <summary>
        /// 
        /// Overloaded (Function): Calculate the greenzorro & Fund fees on any amount that greenzorro offered an investment service.
        /// 
        /// </summary>
        /// <param name="liquidationAmount"></param>
        /// <returns>Total greenzorro + Fund fees on a investment amount.</returns>
        public decimal GetWithdrawnFees(decimal liquidationAmount) {

            decimal gzFeesAmount, fundsFeesAmount;
            return GetWithdrawnFees(liquidationAmount, out gzFeesAmount, out fundsFeesAmount);
        }

        /// <summary>
        /// 
        /// Get the time-stamp of the only (query asks the last in case of duplicate test data)
        /// liquidation transaction for the customer month's activity.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <returns></returns>
        public DateTime GetSoldPortfolioTimestamp(int customerId, int yearCurrent, int monthCurrent) {

            var yearMonthCurrentStr = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent);

            var soldPortfolioTimestamp =
                _db.GzTrxs
                    .Where(t => t.YearMonthCtd == yearMonthCurrentStr
                                && t.CustomerId == customerId &&
                                t.Type.Code == GzTransactionTypeEnum.FullCustomerFundsLiquidation)
                    .Select(t => new { t.Id, t.CreatedOnUtc })
                    .OrderByDescending(t => t.Id)
                    .Select(t => t.CreatedOnUtc)
                    .Single();

            return soldPortfolioTimestamp;
        }

        /// <summary>
        /// 
        /// Create any type of transaction from those allowed.
        /// Used along with peer API methods for specialized transactions
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="gzTransactionType"></param>
        /// <param name="amount"></param>
        /// <param name="trxYearMonth"></param>
        /// <param name="createdOnUtc"></param>
        /// <returns></returns>
        public void SaveDbGzTransaction(int customerId, GzTransactionTypeEnum gzTransactionType, decimal amount, string trxYearMonth, DateTime createdOnUtc) {

            if (
                       gzTransactionType == GzTransactionTypeEnum.GzFees
                    || gzTransactionType == GzTransactionTypeEnum.FundFee
                    || gzTransactionType == GzTransactionTypeEnum.InvWithdrawal
                    || gzTransactionType == GzTransactionTypeEnum.CreditedPlayingLoss) {

                throw new Exception("This type of transaction can be created/updated only by the specialized api of this class" + amount);
            }

            if (amount < 0) {

                throw new Exception("Amount must be greater than 0: " + amount);

            }

            SaveDbGzTransaction(customerId, gzTransactionType, amount, trxYearMonth, createdOnUtc, null);

            _db.SaveChanges();
        }

        /// <summary>
        /// 
        /// Transfer to the Gaming account by selling investment shares and save the calculated commission and fund fees transactions
        /// 
        /// Note investment amount is the full amount before any fees deduction!
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="investmentAmount">Full Investment amount before deducting any fees.</param>
        /// <param name="trxYearMonth"></param>
        /// <param name="createdOnUtc"></param>
        /// <returns></returns>
        public void SaveDbTransferToGamingAmount(int customerId, decimal investmentAmount, string trxYearMonth, DateTime createdOnUtc) {

            if (investmentAmount <= 0) {

                throw new Exception("Invalid amount to transfer to gaming account. Amount must be greater than 0: " + investmentAmount);

            }

            decimal gzFeesAmount, fundsFeesAmount;
            decimal netAmountToCustomer = investmentAmount - GetWithdrawnFees(investmentAmount, out gzFeesAmount, out fundsFeesAmount);

            // Save Fees Transactions
            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.GzFees, gzFeesAmount, trxYearMonth, createdOnUtc, null);

            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.FundFee, fundsFeesAmount, trxYearMonth, createdOnUtc, null);

            // Save the reason for those fees
            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.TransferToGaming, netAmountToCustomer, trxYearMonth, createdOnUtc, null);

            _db.SaveChanges();
        }

        /// <summary>
        /// 
        /// Overloaded: Calculate the greenzorro & Fund fees on any amount that greenzorro offered an investment service.
        /// Returns the individual fees as out parameters.
        /// 
        /// </summary>
        /// <param name="liquidationAmount"></param>
        /// <param name="gzFeesAmount">Out parameter to return the greenzorro fee.</param>
        /// <param name="fundsFeesAmount">Out parameter to return the Fund fee.</param>
        /// <returns>Total greenzorro + Fund fees on a investment amount.</returns>
        private decimal GetWithdrawnFees(decimal liquidationAmount, out decimal gzFeesAmount, out decimal fundsFeesAmount) {

            var confTask = confRepo.GetConfRow();

            var confRow = confTask.Result;

            gzFeesAmount = liquidationAmount *
                // COMMISSION_PCNT: Database Configuration Value
                (decimal)confRow.COMMISSION_PCNT / 100;

            fundsFeesAmount = liquidationAmount *
                // FUND_FEE_PCNT: Database Configuration Value
                (decimal)confRow.FUND_FEE_PCNT / 100;

            return gzFeesAmount + fundsFeesAmount;
        }

        /// <summary>
        /// 
        /// Save to DB the calculated Fund greenzorro fees.
        /// 
        /// Note this is not enclosed within a user transaction. It's the responsibility of the caller.
        /// 
        /// Uses table configuration values.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="liquidationAmount"></param>
        /// <param name="sellingJournalTypeReason"></param>
        /// <param name="trxYearMonth"></param>
        /// <param name="createdOnUtc"></param>
        /// <param name="lastInvestmentCredit">This months casino credit from player losses that will be used for buying funds because of liquidation</param>
        /// <returns></returns>
        public decimal SaveDbLiquidatedPortfolioWithFees(
            int customerId, 
            decimal liquidationAmount, 
            GzTransactionTypeEnum sellingJournalTypeReason, 
            string trxYearMonth, 
            DateTime createdOnUtc, 
            out decimal lastInvestmentCredit)
        {

            if (liquidationAmount <= 0) {

                throw new Exception("Invalid investment amount to liquidate. Amount must be greater than 0: " + liquidationAmount);

            }

            decimal gzFeesAmount, fundsFeesAmount;
            decimal reducedAmountToReturn = liquidationAmount - GetWithdrawnFees(liquidationAmount, out gzFeesAmount, out fundsFeesAmount);

            // Save Fees Transactions
            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.GzFees, gzFeesAmount, trxYearMonth, createdOnUtc, null);

            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.FundFee, fundsFeesAmount, trxYearMonth, createdOnUtc, null);

            // Save the liquidation transaction
            SaveDbGzTransaction(customerId, sellingJournalTypeReason, reducedAmountToReturn, trxYearMonth, createdOnUtc, null);

            // Save the transfer to Everymatrix amount out of the shares selling
            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.TransferToGaming, reducedAmountToReturn, trxYearMonth, 
                createdOnUtc, null);

            // Update any credited player losses back to transfer to Everymatrix amounts without any fees deductions.
            lastInvestmentCredit = SaveDbCashInvestmentsToTrnsfToEverymatrix(customerId, trxYearMonth, createdOnUtc);

            _db.SaveChanges();

            return reducedAmountToReturn;
        }

        /// <summary>
        /// 
        /// Update any cash investment transactions to transactions indicating cash to be returned
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="trxYearMonth"></param>
        /// <param name="createdOnUtc"></param>
        private decimal SaveDbCashInvestmentsToTrnsfToEverymatrix(int customerId, string trxYearMonth, DateTime createdOnUtc) {

            var currentYearMonthStr = createdOnUtc.ToStringYearMonth();

            var cplayingLossId = _db.GzTrxTypes
                .Where(tt => tt.Code == GzTransactionTypeEnum.CreditedPlayingLoss)
                .Select(tt => tt.Id)
                .Single();

            var lastInvestmentCredit = _db.GzTrxs
                .Where(t => t.CustomerId == customerId
                            && t.YearMonthCtd == currentYearMonthStr
                            && t.TypeId == cplayingLossId)
                .Select(t => t.Amount)
                .SingleOrDefault();

            SaveDbGzTransaction(
                customerId, 
                GzTransactionTypeEnum.TransferToGaming, 
                lastInvestmentCredit, trxYearMonth,
                createdOnUtc);

            return lastInvestmentCredit;
        }

        /// <summary>
        /// 
        /// Create or update a playing loss in GzTrx with the credited amount.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="totPlayinLossAmount">Total amount that was lost</param>
        /// <param name="trxYearMonth"></param>
        /// <param name="createdOnUtc">Date of the transaction in UTC</param>
        /// <param name="begGmBalance"></param>
        /// <param name="deposits"></param>
        /// <param name="withdrawals"></param>
        /// <param name="gainLoss"></param>
        /// <param name="endGmbalance"></param>
        /// <returns></returns>
        public void SaveDbPlayingLoss(int customerId, decimal totPlayinLossAmount, string trxYearMonth, DateTime createdOnUtc, decimal begGmBalance, decimal deposits, decimal withdrawals, decimal gainLoss, decimal endGmbalance) {

            var creditPcnt = confRepo.GetConfRow().Result.CREDIT_LOSS_PCNT;

            SaveDbGzTransaction(
                customerId, 
                GzTransactionTypeEnum.CreditedPlayingLoss,
                totPlayinLossAmount * (decimal) creditPcnt / 100, 
                trxYearMonth,
                createdOnUtc,
                creditPcnt, 
                begGmBalance: begGmBalance, 
                deposits: deposits, 
                withdrawals: withdrawals, 
                gainLoss: gainLoss, 
                endGmbalance: endGmbalance);

            _db.SaveChanges();
        }

        /// <summary>
        /// 
        /// Save to database a general type of transaction using an existing DbContext (to support transactions)
        /// Save any transaction type to the database
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="gzTransactionType"></param>
        /// <param name="amount"></param>
        /// <param name="trxYearMonth"></param>
        /// <param name="createdOnUtc">Applicable only for TransferTypeEnum.CreditedPlayingLoss type of transactions</param>
        /// <param name="creditPcntApplied"></param>
        /// <param name="begGmBalance"></param>
        /// <param name="deposits"></param>
        /// <param name="withdrawals"></param>
        /// <param name="gainLoss"></param>
        /// <param name="endGmbalance"></param>
        /// <returns></returns>
        private void SaveDbGzTransaction(
            int customerId, GzTransactionTypeEnum 
            gzTransactionType, 
            decimal amount, 
            string trxYearMonth, 
            DateTime createdOnUtc, 
            float? creditPcntApplied, 
            decimal begGmBalance = 0, 
            decimal deposits = 0, 
            decimal withdrawals = 0, 
            decimal gainLoss = 0, 
            decimal endGmbalance = 0)
        {
            /* Used only for playerLoss type of transactions with different uniqueness than other types of transactions:
                Only 1 playerloss per player/month is allowed */
            if (gzTransactionType == GzTransactionTypeEnum.CreditedPlayingLoss) {
                _db.GzTrxs.AddOrUpdate(

                    t => new {t.CustomerId, t.TypeId, t.YearMonthCtd},
                    new GzTrx {
                        CustomerId = customerId,
                        TypeId =
                            _db.GzTrxTypes.Where(t => t.Code == gzTransactionType).Select(t => t.Id).FirstOrDefault(),
                        YearMonthCtd = trxYearMonth,
                        Amount = amount,
                        BegGmBalance = begGmBalance,
                        Deposits = deposits,
                        Withdrawals = withdrawals,
                        GmGainLoss = gainLoss,
                        EndGmBalance = endGmbalance,
                        // Applicable only for TransferTypeEnum.CreditedPlayingLoss type of transactions
                        CreditPcntApplied = creditPcntApplied,
                        // Truncate Millis to avoid mismatch between .net dt <--> mssql dt
                        CreatedOnUtc = createdOnUtc.Truncate(TimeSpan.FromSeconds(1))
                    }
                    );
            }
            else {
                // Plain insert for non credit loss gztrx
                var newGzTrx =
                    new GzTrx() {
                        CustomerId = customerId,
                        TypeId =
                            _db.GzTrxTypes.Where(t => t.Code == gzTransactionType).Select(t => t.Id).FirstOrDefault(),
                        YearMonthCtd = trxYearMonth,
                        Amount = amount,
                        // Applicable only for TransferTypeEnum.CreditedPlayingLoss type of transactions
                        CreditPcntApplied = creditPcntApplied,
                        // Truncate Millis to avoid mismatch between .net dt <--> mssql dt
                        CreatedOnUtc = createdOnUtc.Truncate(TimeSpan.FromSeconds(1))
                    };
                _db.GzTrxs.Add(newGzTrx);
                _db.SaveChanges();
            }
        }

        /// <summary>
        /// 
        /// Expression lambda for increased readability on transactions.YearMonthCtd being before (past) 
        ///     or same than the incoming string month parameter
        /// 
        /// </summary>
        /// <param name="futureYearMonthStr"></param>
        /// <returns></returns>
        private static Expression<Func<GzTrx, bool>> BeforeEq(string futureYearMonthStr) {
            return t => string.Compare(t.YearMonthCtd, futureYearMonthStr, StringComparison.Ordinal) <= 0;
        }

        /// <summary>
        /// 
        /// Expression lambda for increased readability on transaction.YearMonthCtd being later (future) 
        ///     or same than the incoming string month parameter
        /// 
        /// </summary>
        /// <param name="pastYearMonthStr"></param>
        /// <returns></returns>
        private static Expression<Func<GzTrx, bool>> LaterEq(string pastYearMonthStr) {
            return t => string.Compare(t.YearMonthCtd, pastYearMonthStr, StringComparison.Ordinal) >= 0;
        }
    }
}
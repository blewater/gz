using System;
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
        /// Overloaded: Calculate the greenzorro & Fund fees on any amount that greenzorro offered an investment service.
        /// Returns the individual fees as out parameters.
        /// 
        /// </summary>
        /// <param name="vintageYearMonthStr"></param>
        /// <param name="liquidationAmount"></param>
        /// <param name="vintageCashInvestment"></param>
        /// <returns>Total greenzorro + Fund fees on a investment amount.</returns>
        public FeesDto GetWithdrawnFees(decimal vintageCashInvestment, string vintageYearMonthStr, decimal liquidationAmount) {

            var confRow = confRepo.GetConfRow();

            var isAnEarlyWithdrawal = IsAnEarlyWithdrawal(vintageYearMonthStr, confRow);

            // beyond the unleash day? (early withdrawal fee)
            decimal earlyCashoutFee = GetEarlyCashoutFee(liquidationAmount, isAnEarlyWithdrawal, (decimal)confRow.EARLY_WITHDRAWAL_FEE_PCNT);

            var gzFeesAmount = GetGzFeesAmount(liquidationAmount, isAnEarlyWithdrawal, (decimal)confRow.COMMISSION_PCNT);

            var fundsFeesAmount = GetFundsFeesAmount(liquidationAmount, isAnEarlyWithdrawal, (decimal)confRow.FUND_FEE_PCNT);

            // hurdle fee
            var hurdleFee = GetHurdleFee(
                vintageCashInvestment, 
                liquidationAmount,
                isAnEarlyWithdrawal,
                (decimal) confRow.HURDLE_TRIGGER_GAIN_PCNT,
                (decimal) confRow.EARLY_WITHDRAWAL_FEE_PCNT);

            // Return all fees
            var fees = new FeesDto() {
                EarlyCashoutFee = earlyCashoutFee,
                FundFee = fundsFeesAmount,
                GzFee = gzFeesAmount,
                HurdleFee = hurdleFee,
                Total = gzFeesAmount + fundsFeesAmount + earlyCashoutFee + hurdleFee
            };

            return fees;
        }

        private static bool IsAnEarlyWithdrawal(string vintageYearMonthStr, GzConfiguration confRow)
        {
            const int nominalWithdrawalMonths = 3;

            var vintageMonthDate = DbExpressions.GetDtYearMonthStrTo1StOfMonth(vintageYearMonthStr);

            var futureUnleashDay = vintageMonthDate.AddMonths(nominalWithdrawalMonths); // locking months + 1

            var retValue = futureUnleashDay > DateTime.Today;
            return retValue;
        }

        private static decimal GetEarlyCashoutFee(decimal liquidationAmount, bool isAnEarlyWithdrawal, decimal earlyCashoutFeePcnt)
        {

            decimal earlyCashoutFee = 0m;

            if (isAnEarlyWithdrawal)
            {
                earlyCashoutFee = liquidationAmount
                                  *
                                  (earlyCashoutFeePcnt / 100m);
            }
            return earlyCashoutFee;
        }

        private static decimal GetFundsFeesAmount(decimal liquidationAmount, bool isAnEarlyWithdrawal,
            decimal fundFeePcnt) {

            var fundsFeesAmount =
                !isAnEarlyWithdrawal
                    ? liquidationAmount
                      *
                      (fundFeePcnt / 100m)
                    : 0;
            return fundsFeesAmount;
        }

        private static decimal GetGzFeesAmount(decimal liquidationAmount, bool isAnEarlyWithdrawal, decimal gzCommissionPcnt) {

            var gzFeesAmount =
                !isAnEarlyWithdrawal
                    ? liquidationAmount
                      *
                      (gzCommissionPcnt / 100m)
                    : 0;
            return gzFeesAmount;
        }

        private static decimal GetHurdleFee(
            decimal vintageCashInvestment,
            decimal liquidationAmount,
            bool isEarlyWithdrawal,
            decimal hurdleFeeTriggerPcnt,
            decimal hurdleFeePcnt)
        {

            decimal hurdleFee = 0m;

            // Early withdrawals have no other applicable fees
            if (!isEarlyWithdrawal)
            {

                if (liquidationAmount
                    >=
                    (vintageCashInvestment
                     * // investment gained more than 10%
                     (1 + (hurdleFeeTriggerPcnt / 100m))))
                {

                    hurdleFee = (liquidationAmount - vintageCashInvestment)
                                *
                                (hurdleFeePcnt / 100m);
                }
            }
            return hurdleFee;
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

            var creditPcnt = confRepo.GetConfRow().CREDIT_LOSS_PCNT;

            SaveDbGzTransaction(
                customerId, 
                GzTransactionTypeEnum.CreditedPlayingLoss,
                totPlayinLossAmount * ((decimal) creditPcnt / 100m), 
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
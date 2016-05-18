using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Linq.Expressions;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using gzDAL.Models;

namespace gzDAL.Repos {

    /// <summary>
    /// 
    /// For any Greenzorro Transaction creation/update
    /// Currency conversions are encapsulated here alone.
    /// 
    /// </summary>
    public class GzTransactionRepo : IGzTransactionRepo {
        private readonly ApplicationDbContext _db;

        public GzTransactionRepo(ApplicationDbContext db) {
            this._db = db;
        }

        /// <summary>
        /// 
        /// Get a customer's vintages.
        /// 
        /// Note viewModels have been moved to gzWeb so we return a DTO.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public IEnumerable<VintageDto> GetCustomerVintages(int customerId) {

            var lockInDays = _db.GzConfigurations
                .Select(c => c.LOCK_IN_NUM_DAYS)
                .Single();

            var vintagesList = _db.GzTransactions
                .Where(t => t.Type.Code == GzTransactionJournalTypeEnum.CreditedPlayingLoss 
                    && t.CustomerId == customerId)
                .GroupBy(t => t.YearMonthCtd)
                .OrderByDescending(t => t.Key)
                .Select(g => new {
                    YearMonthStr = g.Key,
                    InvestAmount = g.Sum(t => t.Amount),
                    VintageDate = g.Max(t => t.CreatedOnUTC)
                })
                /** 
                 * The 2 staged select approach with AsEnumerable 
                 *  generates a single select statement vs 
                 *  a one select approach 
                 */
                .AsEnumerable()
                .Select(t => new VintageDto() {
                    YearMonthStr = t.YearMonthStr,
                    InvestAmount = t.InvestAmount,
                    SellThisMonth = (DateTime.UtcNow - t.VintageDate).TotalDays - lockInDays >= 0
                })
                .ToList();

            return vintagesList;
        }

        /// <summary>
        /// 
        /// Get the Customer ids whose transaction activity has been initiated already within a given month
        /// 
        /// </summary>
        /// <param name="thisYearMonth"></param>
        /// <returns></returns>
        public IEnumerable<int> GetActiveCustomers(string thisYearMonth) {

            return _db.GzTransactions
                .Where(BeforeEq(thisYearMonth))
                .OrderBy(t => t.CustomerId)
                .Select(t => t.CustomerId)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 
        /// Data method to enforce the 6? month lock-in period before allowed withdrawals.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public WithdrawEligibilityDTO GetWithdrawEligibilityData(int customerId) {

            string prompt = "First available withdrawal on: ";

            DateTime eligibleWithdrawDate;
            int lockInDays;
            bool okToWithdraw = IsWithdrawalEligible(customerId, out eligibleWithdrawDate, out lockInDays);

            var retValues = new WithdrawEligibilityDTO() {
                LockInDays = lockInDays,
                EligibleWithdrawDate = eligibleWithdrawDate,
                OkToWithdraw = okToWithdraw,
                Prompt = prompt
            };

            return retValues;
        }

        /// <summary>
        /// 
        /// Biz logic for withdrawal eligibility
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="eligibleWithdrawDate"></param>
        /// <param name="lockInDays"></param>
        /// <returns></returns>
        private bool IsWithdrawalEligible(int customerId, out DateTime eligibleWithdrawDate, out int lockInDays) {

            lockInDays = _db.GzConfigurations.Select(c => c.LOCK_IN_NUM_DAYS).Single();

            DateTime earliestLoss = _db.GzTransactions.Where(
                t => t.CustomerId == customerId && t.Type.Code == GzTransactionJournalTypeEnum.CreditedPlayingLoss)
                .OrderByDescending(t => t.Id)
                .Select(t => t.CreatedOnUTC)
                .FirstOrDefault();

            if (earliestLoss.Year == 1) {
                earliestLoss = DateTime.UtcNow;
            }

            eligibleWithdrawDate = earliestLoss.AddDays(lockInDays);

            bool okToWithdraw = eligibleWithdrawDate < DateTime.UtcNow;
            return okToWithdraw;
        }

        /// <summary>
        /// 
        /// Enable or disable the withdrawal button
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public bool GetEnabledWithdraw(int customerId) {

            DateTime eligibleWithdrawDate;
            int lockInDays;
            bool okToWithdraw = IsWithdrawalEligible(customerId, out eligibleWithdrawDate, out lockInDays);

            return okToWithdraw;
        }

        /// <summary>
        /// 
        /// Return whether a customer has a liquidation transaction in a month
        /// 
        /// </summary>
        /// 
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// 
        /// <returns>True: The customer has sold their portfolio. False: if not.</returns>
        public bool GetLiquidationTrxCount(int customerId, int yearCurrent, int monthCurrent) {

            var currentYearMonthStr = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent);

            return _db.GzTransactions
                .Count(t => t.YearMonthCtd == currentYearMonthStr
                            && t.Type.Code == GzTransactionJournalTypeEnum.FullCustomerFundsLiquidation
                            && t.CustomerId == customerId)
                > 0;
        }

        /// <summary>
        /// 
        /// Overloaded (Function): Calculate the Greenzorro & Fund fees on any amount that Greenzorro offered an investment service.
        /// 
        /// </summary>
        /// <param name="liquidationAmount"></param>
        /// <returns>Total Greenzorro + Fund fees on a investment amount.</returns>
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
                _db.GzTransactions
                    .Where(t => t.YearMonthCtd == yearMonthCurrentStr
                                && t.CustomerId == customerId &&
                                t.Type.Code == GzTransactionJournalTypeEnum.FullCustomerFundsLiquidation)
                    .Select(t => new { t.Id, t.CreatedOnUTC })
                    .OrderByDescending(t => t.Id)
                    .Select(t => t.CreatedOnUTC)
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
        /// <param name="createdOnUtc"></param>
        /// <returns></returns>
        public void SaveDbGzTransaction(int customerId, GzTransactionJournalTypeEnum gzTransactionType, decimal amount, DateTime createdOnUtc) {

            if (
                       gzTransactionType == GzTransactionJournalTypeEnum.GzFees
                    || gzTransactionType == GzTransactionJournalTypeEnum.FundFee
                    || gzTransactionType == GzTransactionJournalTypeEnum.InvWithdrawal

                    || gzTransactionType == GzTransactionJournalTypeEnum.PlayingLoss
                    || gzTransactionType == GzTransactionJournalTypeEnum.CreditedPlayingLoss) {

                throw new Exception("This type of transaction can be created/updated only by the specialized api of this class" + amount);
            }

            if (amount < 0) {

                throw new Exception("Amount must be greater than 0: " + amount);

            }

            SaveDbGzTransaction(customerId, gzTransactionType, amount, createdOnUtc, null);

        }

        /// <summary>
        /// 
        /// Transfer to the Gaming account by selling investment shares and save the calculated commission and fund fees transactions
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="withdrawnAmount"></param>
        /// <param name="createdOnUtc"></param>
        /// <returns></returns>
        public void SaveDbTransferToGamingAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUtc) {

            if (withdrawnAmount <= 0) {

                throw new Exception("Invalid amount to transfer to gaming account. Amount must be greater than 0: " + withdrawnAmount);

            }

            ConnRetryConf.TransactWithRetryStrategy(_db,

                () => {

                    SaveDbLiquidatedPortfolioWithFees(customerId, withdrawnAmount, GzTransactionJournalTypeEnum.TransferToGaming, createdOnUtc);

                });
        }

        /// <summary>
        /// 
        /// Save an investment withdrawal transaction and save the calculated commission and fund fees transactions
        /// Note identical method to <see cref="SaveDbTransferToGamingAmount"/> though in practice it may not be
        /// able to instruct the casino platform to use it.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="withdrawnAmount"></param>
        /// <param name="createdOnUtc"></param>
        /// <returns></returns>
        public void SaveDbInvWithdrawalAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUtc) {

            if (withdrawnAmount <= 0) {

                throw new Exception("Invalid withdrawal. Amount must be greater than 0: " + withdrawnAmount);

            }


            ConnRetryConf.TransactWithRetryStrategy(_db,

                () => {

                    SaveDbLiquidatedPortfolioWithFees(customerId, withdrawnAmount, GzTransactionJournalTypeEnum.InvWithdrawal, createdOnUtc);

                });
        }

        /// <summary>
        /// 
        /// Overloaded: Calculate the Greenzorro & Fund fees on any amount that Greenzorro offered an investment service.
        /// Returns the individual fees as out parameters.
        /// 
        /// </summary>
        /// <param name="liquidationAmount"></param>
        /// <param name="gzFeesAmount">Out parameter to return the Greenzorro fee.</param>
        /// <param name="fundsFeesAmount">Out parameter to return the Fund fee.</param>
        /// <returns>Total Greenzorro + Fund fees on a investment amount.</returns>
        private decimal GetWithdrawnFees(decimal liquidationAmount, out decimal gzFeesAmount, out decimal fundsFeesAmount) {

            gzFeesAmount = liquidationAmount *
                // COMMISSION_PCNT: Database Configuration Value
                (decimal)_db.GzConfigurations.Select(c => c.COMMISSION_PCNT).Single() / 100;

            fundsFeesAmount = liquidationAmount *
                // FUND_FEE_PCNT: Database Configuration Value
                (decimal)_db.GzConfigurations.Select(c => c.FUND_FEE_PCNT).Single() / 100;

            return gzFeesAmount + fundsFeesAmount;
        }

        /// <summary>
        /// 
        /// Save to DB the calculated Fund Greenzorro fees.
        /// 
        /// Note this is not enclosed within a user transaction. It's the responsibility of the caller.
        /// 
        /// Uses table configuration values.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="liquidationAmount"></param>
        /// <param name="createdOnUtc"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public decimal SaveDbLiquidatedPortfolioWithFees(int customerId, decimal liquidationAmount, GzTransactionJournalTypeEnum sellingJournalTypeReason, DateTime createdOnUtc) {

            if (liquidationAmount <= 0) {

                throw new Exception("Invalid investment amount to liquidate. Amount must be greater than 0: " + liquidationAmount);

            }

            decimal gzFeesAmount, fundsFeesAmount;
            decimal reducedAmountToReturn = liquidationAmount - GetWithdrawnFees(liquidationAmount, out gzFeesAmount, out fundsFeesAmount);

            // Save Fees Transactions
            SaveDbGzTransaction(customerId, GzTransactionJournalTypeEnum.GzFees, gzFeesAmount, createdOnUtc, null);

            SaveDbGzTransaction(customerId, GzTransactionJournalTypeEnum.FundFee, fundsFeesAmount, createdOnUtc, null);

            // Save the reason for those fees
            SaveDbGzTransaction(customerId, sellingJournalTypeReason, liquidationAmount, createdOnUtc, null);

            return reducedAmountToReturn;
        }

        /// <summary>
        /// 
        /// Create or update a playing loss. Resulting in atomic 2 row being created. A type of PlayingLoss, CreditedPlayingLoss.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="totPlayinLossAmount">Total amount that was lost</param>
        /// <param name="creditPcnt">Percentage number (0-100) to credit balance. For example 50 for half the amount to be credited</param>
        /// <param name="createdOnUtc">Date of the transaction in UTC</param>
        /// <returns></returns>
        public void SaveDbPlayingLoss(int customerId, decimal totPlayinLossAmount, decimal creditPcnt, DateTime createdOnUtc) {

            if (creditPcnt < 0 || creditPcnt > 100) {

                throw new Exception("Invalid percentage not within range 0..100: " + creditPcnt);

            } else {

                ConnRetryConf.TransactWithRetryStrategy(_db,

                    () => {

                        SaveDbGzTransaction(customerId, GzTransactionJournalTypeEnum.PlayingLoss, totPlayinLossAmount, createdOnUtc, null);

                        SaveDbGzTransaction(customerId, GzTransactionJournalTypeEnum.CreditedPlayingLoss,
                            totPlayinLossAmount * creditPcnt / 100,
                            createdOnUtc,
                            // Db Configuration value
                            _db.GzConfigurations.Select(c => c.CREDIT_LOSS_PCNT).Single());

                    });
            }
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
        /// <param name="createdOnUtc"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private void SaveDbGzTransaction(int customerId, GzTransactionJournalTypeEnum gzTransactionType, decimal amount, DateTime createdOnUtc, float? creditPcntApplied) {

            //Not thread safe but ok...within a single request context
            _db.GzTransactions.AddOrUpdate(

                // Assume CreatedOnUtc remains constant for same transaction
                // to support idempotent transactions

                t => new { t.CustomerId, t.TypeId, t.CreatedOnUTC },
                    new GzTransaction {
                        CustomerId = customerId,
                        TypeId = _db.GzTransationTypes.Where(t => t.Code == gzTransactionType).Select(t => t.Id).FirstOrDefault(),
                        YearMonthCtd = createdOnUtc.Year.ToString("0000") + createdOnUtc.Month.ToString("00"),
                        Amount = amount,
                        // Applicable only for TransferTypeEnum.CreditedPlayingLoss type of transactions
                        CreditPcntApplied = creditPcntApplied,
                        // Truncate Millis to avoid mismatch between .net dt <--> mssql dt
                        CreatedOnUTC = DbExpressions.Truncate(createdOnUtc, TimeSpan.FromSeconds(1))
                    }
                );
            _db.SaveChanges();
        }

        /// <summary>
        /// 
        /// Expression lambda for increased readability on transactions.YearMonthCtd being before (past) 
        ///     or same than the incoming string month parameter
        /// 
        /// </summary>
        /// <param name="futureYearMonthStr"></param>
        /// <returns></returns>
        private static Expression<Func<GzTransaction, bool>> BeforeEq(string futureYearMonthStr) {
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
        private static Expression<Func<GzTransaction, bool>> LaterEq(string pastYearMonthStr) {
            return t => string.Compare(t.YearMonthCtd, pastYearMonthStr, StringComparison.Ordinal) >= 0;
        }
    }
}
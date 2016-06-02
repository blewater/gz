using System;
using System.Collections.Generic;
using System.Data.Entity;
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
        /// Get the customer total deposits.
        /// 
        /// This has to query using the Everymatrix customer id.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public decimal GetTotalDeposit(int customerId) {

            decimal totalDeposits = 0;

            var customerIds =
                _db.Users.Where(u => u.Id == customerId)
                    .Select(u => new {
                        u.GmCustomerId,
                        u.Email
                    })
                    .Single();

            if (customerIds.GmCustomerId.HasValue) {

                totalDeposits =
                    _db.GmTrxs
                        .Where(t => t.CustomerId == customerIds.GmCustomerId &&
                                    t.Type.Code == GmTransactionTypeEnum.Deposit)
                        .Select(t => t)
                        .Sum(t => (decimal?)t.Amount) ?? 0;
            } else {

                totalDeposits =
                    _db.GmTrxs
                        .Where(t => t.CustomerEmail == customerIds.Email &&
                                    t.Type.Code == GmTransactionTypeEnum.Deposit)
                        .Select(t => t)
                        .Sum(t => (decimal?) t.Amount) ?? 0;
            }

            return totalDeposits;
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

            var vintagesList = _db.GzTrxs
                .Where(t => t.Type.Code == GzTransactionTypeEnum.CreditedPlayingLoss 
                    && t.CustomerId == customerId)
                .GroupBy(t => t.YearMonthCtd)
                .OrderByDescending(t => t.Key)
                .Select(g => new {
                    YearMonthStr = g.Key,
                    InvestAmount = g.Sum(t => t.Amount),
                    VintageDate = g.Max(t => t.CreatedOnUtc),
                    Sold = g.Any(t => t.Type.Code == GzTransactionTypeEnum.TransferToGaming),
                    MaxInvestmentId = g.Max(t=>t.Id)
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
                    Locked = lockInDays - (DateTime.UtcNow - t.VintageDate).TotalDays > 0,
                    Sold = t.Sold,
                    LastInvestmentId = t.MaxInvestmentId
                })
                .ToList();

            return vintagesList;
        }

        /// <summary>
        /// 
        /// Get the Customer ids whose transaction activity has been initiated already 
        /// within a range of months
        /// 
        /// </summary>
        /// <param name="startYearMonthStr"></param>
        /// <param name="endYearMonthStr"></param>
        /// <returns></returns>
        public IEnumerable<int> GetActiveCustomers(string startYearMonthStr, string endYearMonthStr) {

            if (string.IsNullOrEmpty(startYearMonthStr) && string.IsNullOrEmpty(endYearMonthStr)) {
                throw new Exception("startYearMonth and endYearMonth cannot be empty or null");
            }

            var customerIds = _db.GzTrxs
                .Where(LaterEq(startYearMonthStr))
                .Where(BeforeEq(endYearMonthStr))
                .OrderBy(t => t.CustomerId)
                .Select(t => t.CustomerId)
                .Distinct()
                .ToList();

            return customerIds;
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

            DateTime earliestLoss = _db.GzTrxs.Where(
                t => t.CustomerId == customerId && t.Type.Code == GzTransactionTypeEnum.CreditedPlayingLoss)
                .OrderBy(t => t.Id)
                .Select(t => t.CreatedOnUtc)
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

            return _db.GzTrxs
                .Count(t => t.YearMonthCtd == currentYearMonthStr
                            && t.Type.Code == GzTransactionTypeEnum.FullCustomerFundsLiquidation
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
        /// Perform all database operations for selling a vintage.
        /// 
        /// These operations may or may not call SaveChanges.
        /// 
        /// Assuming will be called within a transaction.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintage"></param>
        /// <param name="soldOnUtc"></param>
        private void SaveDbSellVintage(int customerId, VintageDto vintage, DateTime soldOnUtc) {

            SaveDbLiquidatedPortfolioWithFees(
                customerId, 
                vintage.MarketPrice, 
                GzTransactionTypeEnum.TransferToGaming, 
                soldOnUtc);

            SaveDbSoldVintage(customerId, vintage, soldOnUtc);

            UpdDbVintageInvestmentTrxToCash(customerId, vintage);

            UpdDbSellCustomerFundShares(customerId, vintage);
        }

        /// <summary>
        /// 
        /// Update CreditedPlayingLoss transactions to cash converted of a vintage month.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintage"></param>
        private void UpdDbVintageInvestmentTrxToCash(int customerId, VintageDto vintage) {

            var trxs =
                _db.GzTrxs.Where(t => t.CustomerId == customerId
                                      && t.YearMonthCtd == vintage.YearMonthStr
                                      && t.Type.Code == GzTransactionTypeEnum.CreditedPlayingLoss);

            var liquidatedId = _db.GzTrxTypes.Where(tt => tt.Code == GzTransactionTypeEnum.LiquidatedInvestment)
                    .Select(tt => tt.Id)
                    .First();

            foreach (var gzTrx in trxs) {
                gzTrx.TypeId = liquidatedId;
                //_db.GzTrxs.Attach(gzTrx);
                //var gzTrxEntry = _db.Entry(gzTrx);
                //gzTrxEntry.Property(t => t.TypeId).IsModified = true;
            }
        }

        /// <summary>
        /// 
        /// Update the Customers shares for that month to reduced by the vintage amounts
        /// CustFundShare database entity to 0 any New shares bought for that month
        /// by zeroing the month's shares balance and "archiving" their value.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintage"></param>
        private void UpdDbSellCustomerFundShares(int customerId, VintageDto vintage) {

            var custFundShares =
                _db.CustFundShares
                    .Where(s => s.CustomerId == customerId
                        && s.YearMonth == vintage.YearMonthStr
                        && s.NewSharesNum > 0);

            foreach (var fundShare in custFundShares) {
                fundShare.SharesNum -= fundShare.NewSharesNum ?? 0;
                fundShare.SharesValue -= fundShare.NewSharesValue ?? 0;
                fundShare.SoldNewSharesNum = fundShare.NewSharesNum;
                fundShare.NewSharesNum = 0;
                fundShare.CashedNewSharesValue = fundShare.NewSharesValue;
                fundShare.NewSharesValue = 0;
                //var fundShareEntry = _db.Entry(fundShare);
                //fundShareEntry.State = EntityState.Modified;
            }
        }

        /// <summary>
        /// 
        /// Upsert a sold vintage.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintage"></param>
        /// <param name="soldOnUtc"></param>
        private void SaveDbSoldVintage(int customerId, VintageDto vintage, DateTime soldOnUtc) {

            _db.SoldVintages.AddOrUpdate(
                v => new {v.CustomerId, v.VintageYearMonth},
                new SoldVintage() {
                    CustomerId = customerId,
                    VintageYearMonth = vintage.YearMonthStr,
                    MarketAmount = vintage.MarketPrice,
                    Fees = vintage.Fees,
                    // Truncate Millis to avoid mismatch between .net dt <--> mssql dt
                    SoldOnUtc = DbExpressions.Truncate(soldOnUtc, TimeSpan.FromSeconds(1))
                }
                );
        }

        /// <summary>
        /// 
        /// PreCondition Requirements:
        ///     *** This method assumes it's called within transaction.
        ///     *** Saves only the transactions part of the selling operation excluding balance updating.
        ///     *** This method does not necessarily call SaveChanges.
        ///     *** This method assumes that vintage marketsPrices are up to date.
        /// 
        /// Sell all vintages marked for selling them.
        /// 
        /// Assuming it's called within a transaction.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        public void SaveDbSellVintages(int customerId, IEnumerable<VintageDto> vintages) {

            var soldOnUtc = DateTime.UtcNow;

            foreach (var vintage in vintages) {

                if (vintage.Selected) {
                    SaveDbSellVintage(customerId, vintage, soldOnUtc);
                }
            }
            _db.SaveChanges();
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
        public void SaveDbGzTransaction(int customerId, GzTransactionTypeEnum gzTransactionType, decimal amount, DateTime createdOnUtc) {

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

                    SaveDbLiquidatedPortfolioWithFees(customerId, withdrawnAmount, GzTransactionTypeEnum.TransferToGaming, createdOnUtc);

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

                    SaveDbLiquidatedPortfolioWithFees(customerId, withdrawnAmount, GzTransactionTypeEnum.InvWithdrawal, createdOnUtc);

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
        /// <param name="sellingJournalTypeReason"></param>
        /// <param name="createdOnUtc"></param>
        /// <returns></returns>
        public decimal SaveDbLiquidatedPortfolioWithFees(
            int customerId, 
            decimal liquidationAmount, 
            GzTransactionTypeEnum sellingJournalTypeReason, 
            DateTime createdOnUtc) 
        {

            if (liquidationAmount <= 0) {

                throw new Exception("Invalid investment amount to liquidate. Amount must be greater than 0: " + liquidationAmount);

            }

            decimal gzFeesAmount, fundsFeesAmount;
            decimal reducedAmountToReturn = liquidationAmount - GetWithdrawnFees(liquidationAmount, out gzFeesAmount, out fundsFeesAmount);

            // Save Fees Transactions
            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.GzFees, gzFeesAmount, createdOnUtc, null);

            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.FundFee, fundsFeesAmount, createdOnUtc, null);

            // Save the reason for those fees
            SaveDbGzTransaction(customerId, sellingJournalTypeReason, liquidationAmount, createdOnUtc, null);

            return reducedAmountToReturn;
        }

        /// <summary>
        /// 
        /// Create or update a playing loss. Resulting in atomic 2 rows being created. 
        /// A type of PlayingLoss, CreditedPlayingLoss.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="totPlayinLossAmount">Total amount that was lost</param>
        /// <param name="createdOnUtc">Date of the transaction in UTC</param>
        /// 
        /// <returns></returns>
        public void SaveDbPlayingLoss(int customerId, decimal totPlayinLossAmount, DateTime createdOnUtc) {

            var creditPcnt = _db.GzConfigurations.Select(c => c.CREDIT_LOSS_PCNT).Single();

            SaveDbGzTransaction(
                customerId, 
                GzTransactionTypeEnum.CreditedPlayingLoss,
                totPlayinLossAmount * (decimal) creditPcnt / 100,
                createdOnUtc,
                creditPcnt);

            _db.SaveChanges();
        }

        /// <summary>
        /// 
        /// Save to database a general Gaming type of transaction using an existing DbContext (to support transactions)
        /// Normally we never write to GmTrx table except for testing purposes.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="gzTransactionType"></param>
        /// <param name="amount"></param>
        /// <param name="createdOnUtc"></param>
        /// <returns></returns>
        public void SaveDbGmTransaction(int customerId, GmTransactionTypeEnum gzTransactionType, decimal amount, DateTime createdOnUtc) {

            var customerIds = _db.Users
                .Where(u => u.Id == customerId)
                .Select(u => new {u.GmCustomerId, u.Email})
                .Single();

            GmTrx newGmTrx = new GmTrx {
                CustomerId = customerId,
                GmCustomerId = customerIds.GmCustomerId,
                CustomerEmail = customerIds.Email,
                TypeId = _db.GmTrxTypes.Where(t => t.Code == gzTransactionType).Select(t => t.Id).FirstOrDefault(),
                YearMonthCtd = createdOnUtc.Year.ToString("0000") + createdOnUtc.Month.ToString("00"),
                Amount = amount,
                // Truncate Milliseconds to avoid mismatch between .net dt <--> MSSQl dt
                CreatedOnUtc = DbExpressions.Truncate(createdOnUtc, TimeSpan.FromSeconds(1))
            };

            if (customerIds.GmCustomerId.HasValue) {
                _db.GmTrxs.AddOrUpdate(
                    t => new {t.GmCustomerId, t.CustomerEmail, t.YearMonthCtd, t.TypeId, t.CreatedOnUtc, t.Amount},
                    newGmTrx
                    );
                _db.SaveChanges();
            }
            else {
                _db.GmTrxs.AddOrUpdate(
                    t => new { t.CustomerEmail, t.YearMonthCtd, t.TypeId, t.CreatedOnUtc, t.Amount },
                    newGmTrx
                    );
                _db.SaveChanges();
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
        /// <param name="createdOnUtc">Applicable only for TransferTypeEnum.CreditedPlayingLoss type of transactions</param>
        /// <param name="creditPcntApplied"></param>
        /// <returns></returns>
        private void SaveDbGzTransaction(int customerId, GzTransactionTypeEnum gzTransactionType, decimal amount, DateTime createdOnUtc, float? creditPcntApplied) {

            //Not thread safe but ok...within a single request context
            _db.GzTrxs.AddOrUpdate(

                // Assume CreatedOnUtc remains constant for same transaction
                // to support idempotent transactions

                t => new { t.CustomerId, t.TypeId, t.CreatedOnUtc },
                    new GzTrx {
                        CustomerId = customerId,
                        TypeId = _db.GzTrxTypes.Where(t => t.Code == gzTransactionType).Select(t => t.Id).FirstOrDefault(),
                        YearMonthCtd = createdOnUtc.Year.ToString("0000") + createdOnUtc.Month.ToString("00"),
                        Amount = amount,
                        // Applicable only for TransferTypeEnum.CreditedPlayingLoss type of transactions
                        CreditPcntApplied = creditPcntApplied,
                        // Truncate Millis to avoid mismatch between .net dt <--> mssql dt
                        CreatedOnUtc = DbExpressions.Truncate(createdOnUtc, TimeSpan.FromSeconds(1))
                    }
                );
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
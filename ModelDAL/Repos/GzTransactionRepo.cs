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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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

            decimal totalDeposits = _db.Database

                .SqlQuery<decimal>("Select * From dbo.GetTotalDeposits(@customerId)",
                    new SqlParameter("@CustomerId", customerId))
                .SingleOrDefault();

            return totalDeposits;
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
                MemoryCache.Default.Set(key, lastInvestmentAmount.Value, DateTimeOffset.UtcNow.AddDays(1));
            }
            return lastInvestmentAmount.Value;
        }

        /// <summary>
        /// 
        /// Get month's playling loss
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<decimal> GetLastInvestmentAmountAsync(int userId) {

            using (var db = new ApplicationDbContext()) {

                var thisYearMonthStr = DateTime.UtcNow.ToStringYearMonth();

                var lastInvestmentAmount = await db.Database
                    .SqlQuery<decimal>("Select Amount From dbo.GetMonthsTrxAmount(@CustomerId, @YearMonth, @TrxType)",
                        new SqlParameter("@CustomerId", userId),
                        new SqlParameter("@YearMonth", thisYearMonthStr),
                        new SqlParameter("@TrxType", (int)GzTransactionTypeEnum.CreditedPlayingLoss))
                    .SingleOrDefaultAsync();

                return lastInvestmentAmount;
            }
        }

        /// <summary>
        /// 
        /// Get Total playing loss
        ///   
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<decimal> GetTotalInvestmentsAmountAsync(int userId) {

            using (var db = new ApplicationDbContext()) {

                decimal totalInvestmentsAmount = await db.Database
                    .SqlQuery<decimal>("Select dbo.GetTotalTrxAmount(@CustomerId, @TrxType)",
                        new SqlParameter("@CustomerId", userId),
                        new SqlParameter("@TrxType", (int)GzTransactionTypeEnum.CreditedPlayingLoss))
                    .SingleAsync();

                return totalInvestmentsAmount;
            }
        }

        /// <summary>
        /// 
        /// Get Total Liquidation or investment sales.
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<decimal> GetTotalWithdrawalsAmountAsync(int userId) {

            using (var db = new ApplicationDbContext()) {

                decimal totalWithdrawalsAmount = await db.Database
                    .SqlQuery<decimal>("Select dbo.GetTotalTrxAmount(@customerId, @TrxType)",
                        new SqlParameter("@CustomerId", userId),
                        new SqlParameter("@TrxType", (int)GzTransactionTypeEnum.TransferToGaming))
                    .SingleAsync();

                return totalWithdrawalsAmount;
            }
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

            var customerIds =

                from t in _db.GzTrxs
                join c in _db.Users on t.CustomerId equals c.Id
                where !c.DisabledGzCustomer && !c.ClosedGzAccount 
                    && string.Compare(t.YearMonthCtd, startYearMonthStr, StringComparison.Ordinal) >= 0
                    && string.Compare(t.YearMonthCtd, endYearMonthStr, StringComparison.Ordinal) <= 0
                group c by c.Id
                into g
                select g.Key;

            return customerIds;
        }

        /// <summary>
        /// 
        /// Data method to enforce the 6? month lock-in period before allowed withdrawals.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<WithdrawEligibilityDTO> GetWithdrawEligibilityDataAsync(int customerId) {

            string prompt = "First available withdrawal on: ";

            var task = IsWithdrawalEligible(customerId);
            var tuple = await task;
            var eligibleWithdrawDate = tuple.Item2;
            var lockInDays = tuple.Item3;
            bool okToWithdraw = tuple.Item1;

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
        private async Task<Tuple<bool, DateTime, int>> IsWithdrawalEligible(int customerId) {

            var task = _db.GzConfigurations
                .FromCacheAsync(DateTime.UtcNow.AddDays(1));
            var confRow = task.Result;

            var lockInDays = confRow
                .Select(c => c.LOCK_IN_NUM_DAYS)
                .Single();

            DateTime earliestLoss = await _db.Database
                .SqlQuery<DateTime>("Select dbo.GetMinDateTrx(@CustomerId, @TrxType)",
                    new SqlParameter("@CustomerId", customerId),
                    new SqlParameter("@TrxType", (int)GzTransactionTypeEnum.CreditedPlayingLoss))
                .SingleOrDefaultAsync();

            if (earliestLoss.Year == 1900) {
                earliestLoss = DateTime.UtcNow;
            }

            var eligibleWithdrawDate = earliestLoss.AddDays(lockInDays);

            bool okToWithdraw = eligibleWithdrawDate < DateTime.UtcNow;
            return Tuple.Create(okToWithdraw, eligibleWithdrawDate, lockInDays);
        }

        /// <summary>
        /// 
        /// Enable or disable the withdrawal button
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<bool> GetEnabledWithdraw(int customerId) {

            var task = IsWithdrawalEligible(customerId);
            var tuple = await task;
            bool okToWithdraw = tuple.Item1;

            // Not needed
            //var eligibleWithdrawDate = tuple.Item2;
            //var lockInDays = tuple.Item3;

            return okToWithdraw;
        }

        /// <summary>
        /// 
        /// Return whether a customer has a liquidation transaction in a month
        /// 
        /// </summary>
        /// <param name="customerId"></param>
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

            SaveDbTransferToGamingAmount(customerId, vintage.MarketPrice, soldOnUtc);

            SaveDbSoldVintage(customerId, vintage, soldOnUtc);
        }

        /// <summary>
        /// 
        /// Upsert a sold vintage.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintage"></param>
        /// <param name="soldOnUtc"></param>
        private void SaveDbSoldVintage(
            int customerId, 
            VintageDto vintage, 
            DateTime soldOnUtc) {

            SaveDbSoldVintageCustFundShares(customerId, vintage, soldOnUtc);

            SaveDbSoldVintageInvBalance(customerId, vintage, soldOnUtc);
        }

        /// <summary>
        /// 
        /// Update custFundShares with sold values for a sold vintage
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintage"></param>
        /// <param name="soldOnUtc"></param>
        private void SaveDbSoldVintageInvBalance(int customerId, VintageDto vintage, DateTime soldOnUtc) {

            try {
                var vintageToBeSold = _db.InvBalances
                    .Where(b => b.Id == vintage.InvBalanceId)
                    .Select(b => b)
                    .Single();
                vintageToBeSold.Sold = true;
                vintageToBeSold.SoldAmount = vintage.MarketPrice - vintage.Fees;
                vintageToBeSold.SoldFees = vintage.Fees;
                vintageToBeSold.SoldOnUtc = soldOnUtc.Truncate(TimeSpan.FromSeconds(1));
                vintageToBeSold.UpdatedOnUtc = vintageToBeSold.SoldOnUtc.Value;
                // this is set to the month sold
                vintageToBeSold.SoldYearMonth = soldOnUtc.ToStringYearMonth();

                _db.SaveChanges();
            }
            catch (Exception ex) {
                _logger.Error("While selling vintage: {0}, for customerId: {1}, updating invBalance: Exception {2}", vintage.YearMonthStr, customerId, ex);
            }
        }

        /// <summary>
        /// 
        /// Update InvBalance for a sold vintage.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintage"></param>
        /// <param name="soldOnUtc"></param>
        private void SaveDbSoldVintageCustFundShares(int customerId, VintageDto vintage, DateTime soldOnUtc) {

            try {
                // Copy DTOs back to the InvBalance(vintage).CustFundShare entity
                foreach (var dto in vintage.CustomerVintageShares) {

                    var custFundShare = _db.CustFundShares
                        .SingleOrDefault(s => s.Id == dto.Id);

                    custFundShare.SoldSharesValue = dto.NewSharesValue;
                    custFundShare.SoldOnUtc = soldOnUtc;
                    custFundShare.SharesFundPriceId = dto.SharesFundPriceId;
                }
                _db.SaveChanges();
            }
            catch (Exception ex) {
                _logger.Error("While selling vintage: {0}, for customerId: {1}, updating custFundShares: Exception {2}", vintage.YearMonthStr, customerId, ex);
            }
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
        public void SaveDbSellVintages(int customerId, ICollection<VintageDto> vintages) {

            var soldOnUtc = DateTime.UtcNow;

            foreach (var vintage in vintages) {

                if (vintage.Selected) {
                    SaveDbSellVintage(customerId, vintage, soldOnUtc);
                }
            }
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
        /// <param name="createdOnUtc"></param>
        /// <returns></returns>
        public void SaveDbTransferToGamingAmount(int customerId, decimal investmentAmount, DateTime createdOnUtc) {

            if (investmentAmount <= 0) {

                throw new Exception("Invalid amount to transfer to gaming account. Amount must be greater than 0: " + investmentAmount);

            }

            decimal gzFeesAmount, fundsFeesAmount;
            decimal netAmountToCustomer = investmentAmount - GetWithdrawnFees(investmentAmount, out gzFeesAmount, out fundsFeesAmount);

            // Save Fees Transactions
            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.GzFees, gzFeesAmount, createdOnUtc, null);

            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.FundFee, fundsFeesAmount, createdOnUtc, null);

            // Save the reason for those fees
            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.TransferToGaming, netAmountToCustomer, createdOnUtc, null);

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

            var confTask = _db.GzConfigurations
                .FromCacheAsync(DateTime.UtcNow.AddDays(1));

            var confRow = confTask.Result;

            gzFeesAmount = liquidationAmount *
                // COMMISSION_PCNT: Database Configuration Value
                (decimal)confRow.Select(c => c.COMMISSION_PCNT)
                    .Single() / 100;

            fundsFeesAmount = liquidationAmount *
                // FUND_FEE_PCNT: Database Configuration Value
                (decimal)confRow.Select(c => c.FUND_FEE_PCNT)
                    .Single() / 100;

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
        /// <param name="createdOnUtc"></param>
        /// <param name="lastInvestmentCredit">This months casino credit from player losses that will be used for buying funds becuase of liquidation</param>
        /// <returns></returns>
        public decimal SaveDbLiquidatedPortfolioWithFees(
            int customerId, 
            decimal liquidationAmount, 
            GzTransactionTypeEnum sellingJournalTypeReason, 
            DateTime createdOnUtc,
            out decimal lastInvestmentCredit)
        {

            if (liquidationAmount <= 0) {

                throw new Exception("Invalid investment amount to liquidate. Amount must be greater than 0: " + liquidationAmount);

            }

            decimal gzFeesAmount, fundsFeesAmount;
            decimal reducedAmountToReturn = liquidationAmount - GetWithdrawnFees(liquidationAmount, out gzFeesAmount, out fundsFeesAmount);

            // Save Fees Transactions
            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.GzFees, gzFeesAmount, createdOnUtc, null);

            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.FundFee, fundsFeesAmount, createdOnUtc, null);

            // Save the liquidation transaction
            SaveDbGzTransaction(customerId, sellingJournalTypeReason, reducedAmountToReturn, createdOnUtc, null);

            // Save the transfer to Everymatrix amount out of the shares selling
            SaveDbGzTransaction(customerId, GzTransactionTypeEnum.TransferToGaming, reducedAmountToReturn, 
                createdOnUtc, null);

            // Update any credited player losses back to transfer to Everymatrix amounts without any fees deductions.
            lastInvestmentCredit = SaveDbCashInvestmentsToTrnsfToEverymatrix(customerId, createdOnUtc);

            _db.SaveChanges();

            return reducedAmountToReturn;
        }

        /// <summary>
        /// 
        /// Update any cash investment transactions to transactions indicating cash to be returned
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="createdOnUtc"></param>
        private decimal SaveDbCashInvestmentsToTrnsfToEverymatrix(int customerId, DateTime createdOnUtc) {

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
                lastInvestmentCredit,
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

                t => new { t.CustomerId, t.TypeId, t.YearMonthCtd, t.Amount, t.CreatedOnUtc },
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
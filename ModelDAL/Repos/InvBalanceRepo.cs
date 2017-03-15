using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Runtime.Caching;
using System.Threading.Tasks;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using gzDAL.Models;
using NLog;
using Z.EntityFramework.Plus;

namespace gzDAL.Repos
{

    /// <summary>
    /// Return type of investment balance values
    /// </summary>
    public class InvBalAmountsRow
    {
        public decimal Balance { get; set; }
        public decimal CashInvestment { get; set; }
        // Total player losses for vintages that have not been sold
        public decimal TotalCashInvInHold { get; set; }
        public decimal LowRiskShares { get; set; }
        public decimal MediumRiskShares { get; set; }
        public decimal HighRiskShares { get; set; }
        //-- New monthly gaming amounts imported by the reports
        public decimal BegGmBalance { get; set; }
        public decimal Deposits { get; set; }
        public decimal Withdrawals { get; set; }
        public decimal GmGainLoss { get; set; }
        public decimal EndGmBalance { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }

    public class InvBalanceRepo : IInvBalanceRepo
    {
        private readonly ApplicationDbContext _db;
        private readonly ICustFundShareRepo _customerFundSharesRepo;
        private readonly IGzTransactionRepo _gzTransactionRepo;
        private readonly ICustPortfolioRepo _custPortfolioRepo;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public InvBalanceRepo(
            ApplicationDbContext db,
            ICustFundShareRepo customerFundSharesRepo,
            IGzTransactionRepo gzTransactionRepo,
            ICustPortfolioRepo custPortfolioRepo)
        {

            this._db = db;
            this._customerFundSharesRepo = customerFundSharesRepo;
            this._gzTransactionRepo = gzTransactionRepo;
            this._custPortfolioRepo = custPortfolioRepo;
        }

        /// <summary>
        /// 
        /// CacheBalance specifying the month and ask it asynchronously.
        /// 
        /// Meant to be used with GetCachedLatestBalanceTimestamp() if possible after a short time delay.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="db"></param>
        /// <param name="yyyyMm"></param>
        /// <returns></returns>
        public Task<IEnumerable<InvBalance>> CacheLatestBalanceAsyncByMonth(int customerId, string yyyyMm)
        {

            var lastBalanceRowTask = _db.InvBalances
                .Where(i => i.CustomerId == customerId
                            && i.YearMonth == yyyyMm)
                // Cache 4 hours
                .FromCacheAsync(DateTime.UtcNow.AddHours(4));

            return lastBalanceRowTask;
        }

        /// <summary>
        /// 
        /// CacheBalance and ask it asynchronously.
        /// 
        /// Meant to be used with GetCachedLatestBalanceTimestamp() if possible after a short time delay.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public Task<IEnumerable<InvBalance>> CacheLatestBalanceAsync(int customerId)
        {

            var currentMonth = DateTime.UtcNow.ToStringYearMonth();

            return CacheLatestBalanceAsyncByMonth(customerId, currentMonth);
        }

        /// <summary>
        /// 
        /// Call this after CacheLatestBalance() to get the results.
        /// 
        /// </summary>
        /// <param name="lastBalanceRowTask"></param>
        /// <returns>
        /// 1. Balance Amount of last month
        /// 2. Last updated timestamp of invBalance.
        /// </returns>
        public async Task<InvBalAmountsRow> GetCachedLatestBalanceTimestampAsync(Task<IEnumerable<InvBalance>> lastBalanceRowTask)
        {

            var res = await lastBalanceRowTask;

            var cachedBalanceRow = new InvBalAmountsRow();

            var balRow = res.Select(b => new
            {
                b.Balance,
                b.CashInvestment,
                b.TotalCashInvInHold,
                b.LowRiskShares,
                b.MediumRiskShares,
                b.HighRiskShares,
                b.BegGmBalance,
                b.Deposits,
                b.Withdrawals,
                GmGainLoss = b.GmGainLoss,
                EndGmBalance = b.EndGmBalance,
                b.UpdatedOnUtc
            })
            .SingleOrDefault();

            cachedBalanceRow.Balance = balRow?.Balance ?? 0;
            cachedBalanceRow.CashInvestment = balRow?.CashInvestment ?? 0;
            cachedBalanceRow.TotalCashInvInHold = balRow?.TotalCashInvInHold ?? 0;
            cachedBalanceRow.LowRiskShares = balRow?.LowRiskShares ?? 0;
            cachedBalanceRow.MediumRiskShares = balRow?.MediumRiskShares ?? 0;
            cachedBalanceRow.HighRiskShares = balRow?.HighRiskShares ?? 0;
            cachedBalanceRow.BegGmBalance = balRow?.BegGmBalance ?? 0;
            cachedBalanceRow.Deposits = balRow?.Deposits ?? 0;
            cachedBalanceRow.Withdrawals = balRow?.Withdrawals ?? 0;
            cachedBalanceRow.GmGainLoss = balRow?.GmGainLoss ?? 0;
            cachedBalanceRow.EndGmBalance = balRow?.EndGmBalance ?? 0;
            cachedBalanceRow.UpdatedOnUtc = balRow?.UpdatedOnUtc ?? DateTime.MinValue;

            return cachedBalanceRow;
        }

        #region Vintages

        /// <summary>
        /// 
        /// Data method to enforce the 6? month lock-in period before allowed withdrawals.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<WithdrawEligibilityDTO> GetWithdrawEligibilityDataAsync(int customerId)
        {

            string prompt = "First available withdrawal on: ";

            var tuple = await IsWithdrawalEligible(customerId);
            var eligibleWithdrawDate = tuple.Item2;
            var lockInDays = tuple.Item3;
            bool okToWithdraw = tuple.Item1;

            var retValues = new WithdrawEligibilityDTO()
            {
                LockInDays = lockInDays,
                EligibleWithdrawDate = eligibleWithdrawDate,
                OkToWithdraw = okToWithdraw,
                Prompt = prompt
            };

            return retValues;
        }

        /// <summary>
        /// 
        /// Slack Q and A:
        /// "Post 3 months to sell a vintage" ->  On the 4th month Regardless of the number of days.
        /// 
        /// Biz logic for withdrawal eligibility
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        private async Task<Tuple<bool, DateTime, int>> IsWithdrawalEligible(int customerId)
        {

            var lockInDays = GetDbConfVintageLockInDaysValue();

            var nowUtc = DateTime.UtcNow;
            var monthsLockCnt = lockInDays / 30;

            var oldestVintageYm = await GetAndCacheOldestVintageYm(customerId);

            var oldestVintage = oldestVintageYm != null ? DbExpressions.GetDtYearMonthStrTo1StOfMonth(oldestVintageYm) : nowUtc;
            var eligibleWithdrawDate = oldestVintage.AddMonths(monthsLockCnt + 1); // the 1st of the 4th month
            var okToWithdraw = eligibleWithdrawDate <= nowUtc;

            return Tuple.Create(okToWithdraw, eligibleWithdrawDate, lockInDays);
        }

        private async Task<string> GetAndCacheOldestVintageYm(int customerId)
        {

            string key = "oldestVintage" + customerId;
            var oldestVintageYm = (string)MemoryCache.Default.Get(key)
                                  ??
                                  await _db.InvBalances
                                      .Where(i => i.CustomerId == customerId)
                                      .Select(i => i.YearMonth)
                                      .OrderBy(ym => ym)
                                      .FirstOrDefaultAsync();

            // 1 day cache
            if (oldestVintageYm != null) MemoryCache.Default.Set(key, oldestVintageYm, DateTimeOffset.UtcNow.AddDays(1));
            return oldestVintageYm;
        }

        private int GetDbConfVintageLockInDaysValue()
        {

            var task = _db.GzConfigurations
                .FromCacheAsync(DateTime.UtcNow.AddDays(1));
            var confRow = task.Result;

            var lockInDays = confRow
                .Select(c => c.LOCK_IN_NUM_DAYS)
                .Single();
            return lockInDays;
        }

        /// <summary>
        /// 
        /// Enable or disable the withdrawal button
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<bool> GetEnabledWithdraw(int customerId)
        {

            var tuple = await IsWithdrawalEligible(customerId);
            bool okToWithdraw = tuple.Item1;

            return okToWithdraw;
        }

        /// <summary>
        /// 
        /// Get User Vintages from InvBalance.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public ICollection<VintageDto> GetCustomerVintages(int customerId)
        {

            var monthsLockPeriod = GetDbConfVintageLockInDaysValue() / 30;

            var vintagesList = _db.Database
                .SqlQuery<VintageDto>(
                    "SELECT InvBalanceId, YearMonthStr, InvestmentAmount, Sold, SellingValue," +
                    " SoldFees, SoldYearMonth FROM dbo.GetVintages(@CustomerId)",
                    new SqlParameter("@CustomerId", customerId))
                .ToList();

            foreach (var dto in vintagesList)
            {
                var firstOfMonthUnlocked = DbExpressions.GetDtYearMonthStrTo1StOfMonth(dto.YearMonthStr).AddMonths(monthsLockPeriod + 1);
                dto.Locked = firstOfMonthUnlocked > DateTime.UtcNow;
            }

            return vintagesList;
        }

        /// <summary>
        /// 
        /// Unit Test Supporting verson
        /// 
        /// Calculate the vintage in portfolio market value for a given month.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintageYearMonthStr"></param>
        /// <param name="sellOnThisYearMonth"></param>
        /// <param name="vintageSharesDto"></param>
        /// <param name="fees"></param>
        /// <returns></returns>
        private decimal GetVintageValuePricedOn(
            int customerId,
            string vintageYearMonthStr,
            string sellOnThisYearMonth,
            out VintageSharesDto vintageSharesDto,
            out decimal fees)
        {

            vintageSharesDto = _customerFundSharesRepo.GetVintageSharesMarketValueOn(
                customerId,
                vintageYearMonthStr,
                sellOnThisYearMonth);

            fees = _gzTransactionRepo.GetWithdrawnFees(vintageSharesDto.MarketPrice);

            return vintageSharesDto.MarketPrice;
        }

        /// <summary>
        /// 
        /// Calculate the vintage in latest portfolio market value
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintageYearMonthStr"></param>
        /// <param name="vintageSharesDto"></param>
        /// <param name="fees"></param>
        /// <returns></returns>
        private decimal GetVintageValuePricedNow(
            int customerId,
            string vintageYearMonthStr,
            out VintageSharesDto vintageSharesDto,
            out decimal fees)
        {

            vintageSharesDto = _customerFundSharesRepo.GetVintageSharesMarketValue(
                customerId,
                vintageYearMonthStr);

            fees = _gzTransactionRepo.GetWithdrawnFees(vintageSharesDto.MarketPrice);

            return vintageSharesDto.MarketPrice;
        }

        /// <summary>
        /// 
        /// Unit Test Supporting Version
        /// 
        /// Set the vintages market selling value on a given month
        /// 
        /// *Does not* checks before attempting to return the present selling value.
        /// 
        /// De-selects a Vintage for selling if sold already (slipped through error validation cracks).
        /// 
        /// Throws an exception if vintage is already sold or not available for selling.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <param name="sellOnThisYearMonth"></param>
        /// <returns>Number of vintages marked for selling</returns>
        public int SetVintagesMarketValueOn(int customerId, IEnumerable<VintageDto> vintages, string sellOnThisYearMonth)
        {

            var numOfVintagesSold = 0;
            foreach (var vintageDto in vintages)
            {
                if (vintageDto.Selected)
                {
                    decimal fees;
                    VintageSharesDto vintageShares;
                    vintageDto.MarketPrice = GetVintageValuePricedOn(
                        customerId,
                        vintageDto.YearMonthStr,
                        //-- On this month
                        sellOnThisYearMonth,
                        out vintageShares,
                        out fees);

                    vintageDto.VintageShares = vintageShares;
                    vintageDto.Fees = fees;

                    numOfVintagesSold++;
                }
            }

            return numOfVintagesSold;
        }

        /// <summary>
        /// 
        /// Set the vintages market selling value at present market value.
        /// 
        /// Checks for selling preconditions before attempting to return the present selling value.
        /// 
        /// De-selects a Vintage for selling if sold already or locked (slipped through error validation cracks).
        /// 
        /// Throws an exception if vintage is already sold or not available for selling.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <returns>Number of vintages marked for selling</returns>
        public int SetVintagesPresentMarketValue(int customerId, IEnumerable<VintageDto> vintages)
        {

            var numOfVintagesSold = 0;
            foreach (var vintageDto in vintages)
            {
                if (vintageDto.Selected)
                {
                    decimal fees;
                    VintageSharesDto vintageShares;
                    vintageDto.MarketPrice = GetVintageValuePricedNow(
                        customerId,
                        vintageDto.YearMonthStr,
                        out vintageShares,
                        out fees);

                    vintageDto.VintageShares = vintageShares;
                    vintageDto.Fees = fees;

                    numOfVintagesSold++;
                }
            }

            return numOfVintagesSold;
        }

        /// <summary>
        /// 
        /// Get the vintages with the selling value calculated if not sold
        /// 
        /// -- or
        /// 
        /// their selling value when they were sold.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public ICollection<VintageDto> GetCustomerVintagesSellingValue(int customerId)
        {

            var customerVintages = GetCustomerVintages(customerId)
                .ToList();
            GetCustomerVintagesSellingValue(customerId, customerVintages);

            return customerVintages;
        }

        /// <summary>
        /// 
        /// Unit Test Supporting Version.
        /// 
        /// Get the vintages with the selling value calculated if not sold.
        /// 
        /// Calculate the vintage in the market value and deduct fees. The amount the customer
        /// would receive.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerVintages"></param>
        /// <param name="sellOnThisYearMonth"></param>
        /// <returns></returns>
        public ICollection<VintageDto> GetCustomerVintagesSellingValueOn(int customerId, List<VintageDto> customerVintages, string sellOnThisYearMonth)
        {

            foreach (var dto in customerVintages
                .Where(v => v.SellingValue == 0 && !v.Locked))
            {
                // out var declarations
                VintageSharesDto vintageShares;
                decimal fees;

                // Call to calculate latest selling price
                decimal vintageMarketPrice = GetVintageValuePricedOn(
                        customerId,
                        dto.YearMonthStr,
                        sellOnThisYearMonth,
                        out vintageShares,
                        out fees);

                // Save the selling price and shares
                dto.VintageShares = vintageShares;
                dto.SellingValue = vintageMarketPrice - fees;
            }

            return customerVintages;
        }

        /// <summary>
        /// 
        /// Get the vintages with the selling value calculated if not sold.
        /// 
        /// Calculate the vintage in latest portfolio market value and deduct fees. The amount the customer
        /// would receive.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerVintages"></param>
        /// <returns></returns>
        public ICollection<VintageDto> GetCustomerVintagesSellingValue(int customerId, List<VintageDto> customerVintages)
        {

            foreach (var dto in customerVintages
                .Where(v => v.SellingValue == 0 && !v.Locked))
            {

                // out var declarations
                VintageSharesDto vintageShares;
                decimal fees;

                // Call to calculate latest selling price
                decimal vintageMarketPrice = GetVintageValuePricedNow(
                        customerId,
                        dto.YearMonthStr,
                        out vintageShares,
                        out fees);

                // Save the selling price and shares
                dto.VintageShares = vintageShares;
                dto.SellingValue = vintageMarketPrice - fees;
            }

            return customerVintages;
        }

        /// <summary>
        /// 
        /// Update custFundShares with sold values for a sold vintage
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintage"></param>
        /// <param name="soldOnUtc"></param>
        /// <param name="soldOnYearMonth"></param>
        private void SaveDbSoldVintageInvBalance(int customerId, VintageDto vintage, DateTime soldOnUtc, string soldOnYearMonth = "")
        {

            try
            {
                var vintageToBeSold = _db.InvBalances
                    .Where(b => b.Id == vintage.InvBalanceId)
                    .Select(b => b)
                    .Single();
                vintageToBeSold.Sold = true;
                vintageToBeSold.SoldAmount = vintage.MarketPrice;
                vintageToBeSold.SoldFees = vintage.Fees;
                vintageToBeSold.SoldOnUtc = soldOnUtc.Truncate(TimeSpan.FromSeconds(1));
                vintageToBeSold.UpdatedOnUtc = vintageToBeSold.SoldOnUtc.Value;
                // this is set to the month sold
                vintageToBeSold.SoldYearMonth =
                    soldOnYearMonth.Length == 0
                    ? soldOnUtc.ToStringYearMonth()
                    : soldOnYearMonth;

                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error("While selling vintage: {0}, for customerId: {1}, updating invBalance: Exception {2}", vintage.YearMonthStr, customerId, ex);
            }
        }

        /// <summary>
        /// 
        /// PreCondition Requirements:
        ///     *** This method assumes it's called within transaction.
        ///     *** This method assumes that vintage marketsPrices are up to date.
        /// 
        /// Perform all database operations for selling a vintage.
        /// 
        /// Sell all vintages marked for selling them.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <param name="soldOnYearMonth"></param>
        private void SaveDbSellVintagesBalances(int customerId, ICollection<VintageDto> vintages, string soldOnYearMonth = "")
        {
            var soldOnUtc = DateTime.UtcNow;

            foreach (var vintage in vintages)
            {
                if (vintage.Selected) {

                    SaveDbSoldVintageInvBalance(customerId, vintage, soldOnUtc, soldOnYearMonth);
                }
            }
        }

        /// <summary>
        /// 
        /// Sell any vintages marked for selling. 
        /// 
        /// This method will update the vintages selling values before selling them.
        /// The vintages are sold at the current fund prices as of this method call.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <param name="sellOnThisYearMonth"></param>
        /// <returns></returns>
        public void SaveDbSellVintages(int customerId, ICollection<VintageDto> vintages, string sellOnThisYearMonth = "")
        {

            // Set market price on it.. they may have left the browser window open 
            // for a long time (stock market-wise) before hitting withdraw

            var vintagesToBeSold = 
                sellOnThisYearMonth.Length == 0 
                    ? SetVintagesPresentMarketValue(customerId, vintages) 
                    : SetVintagesMarketValueOn(customerId, vintages, sellOnThisYearMonth);

            if (vintagesToBeSold > 0)
            {
                ConnRetryConf.TransactWithRetryStrategy(_db,

                    () =>
                    {
                        SaveDbSellVintagesBalances(customerId, vintages, sellOnThisYearMonth);

                    });

            }
        }

        #endregion Vintages
        #region Fund Shares Selling

        /// <summary>
        /// 
        /// Sell completely a customer's owned funds shares.
        /// 
        /// Supports selling shares from a previous month in present fund stock value.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="updatedDateTimeUtc">The database creation time-stamp.</param>
        /// <param name="yearCurrent">Optional year value for selling in the past</param>
        /// <param name="monthCurrent">Optional month value for selling in the past</param>
        [Obsolete]
        public bool SaveDbSellAllCustomerFundsShares(
            int customerId,
            DateTime updatedDateTimeUtc,
            int yearCurrent = 0,
            int monthCurrent = 0)
        {

            // Assume we don't sell shares
            var soldShares = false;

            if (yearCurrent == 0)
            {
                yearCurrent = DateTime.UtcNow.Year;
            }
            if (monthCurrent == 0)
            {
                monthCurrent = DateTime.UtcNow.Month;
            }

            if (new DateTime(yearCurrent, monthCurrent, 1) > DateTime.UtcNow)
            {

                throw new Exception("Cannot sell the customer's (id: " + customerId + ") portfolio in the future.");
            }

            var yyyyMm = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent);
            var liquidatedMonth = true;
            //_db.InvBalances
            //.Any(b => b.YearMonth == yyyyMm
            //          && b.CustomerId == customerId
            //          && b.CashBalance > 0
            //          && b.CashInvestment < 0);

            if (!liquidatedMonth)
            {

                // Calculate the value of the fund shares
                RiskToleranceEnum monthsPortfolioRisk;
                var portfolioFundsValuesThisMonth = _customerFundSharesRepo.GetMonthlyFundSharesAfterBuyingSelling(
                    customerId,
                    0,
                    yearCurrent,
                    monthCurrent,
                    out monthsPortfolioRisk);

                // Make sure we have shares to sell
                if (portfolioFundsValuesThisMonth.Sum(f => f.Value.SharesNum) > 0)
                {

                    decimal invGainLoss, monthlyBalance;
                    GetSharesBalanceThisMonth(customerId, portfolioFundsValuesThisMonth, yearCurrent, monthCurrent,
                        out monthlyBalance, out invGainLoss);

                    SaveDbLiquidateCustomerPortfolio(
                        portfolioFundsValuesThisMonth,
                        customerId,
                        yearCurrent,
                        monthCurrent,
                        monthlyBalance,
                        invGainLoss,
                        monthsPortfolioRisk,
                        updatedDateTimeUtc);

                    soldShares = true;
                }
            }

            return soldShares;
        }


        /// <summary>
        /// 
        /// Save the liquidated Customer portfolio funds.
        /// 
        /// Enclosed in a transaction.
        /// 
        /// </summary>
        /// <param name="portfolioFunds"></param>
        /// <param name="customerId"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <param name="newMonthlyBalance"></param>
        /// <param name="invGainLoss"></param>
        /// <param name="monthsPortfolioRisk"></param>
        /// <param name="updatedDateTimeUtc">Set the desired datetime stamp of the db operations</param>
        [Obsolete]
        private void SaveDbLiquidateCustomerPortfolio(
            Dictionary<int, PortfolioFundDTO> portfolioFunds,
            int customerId,
            int yearCurrent,
            int monthCurrent,
            decimal newMonthlyBalance,
            decimal invGainLoss,
            RiskToleranceEnum monthsPortfolioRisk,
            DateTime updatedDateTimeUtc)
        {

            /****************** Liquidate a month ****************/
            var currentYearMonthStr = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent);

            ConnRetryConf.TransactWithRetryStrategy(_db,

            () =>
            {

                // Save the portfolio for the month
                _custPortfolioRepo.SaveDbCustMonthsPortfolioMix(
                    customerId,
                    monthsPortfolioRisk,
                    yearCurrent,
                    monthCurrent,
                    updatedDateTimeUtc);

                // Save fees transactions first and continue with reduced cash amount
                decimal lastInvestmentCredit;
                var remainingCashAmount =
                        _gzTransactionRepo.SaveDbLiquidatedPortfolioWithFees(
                            customerId,
                            newMonthlyBalance,
                            GzTransactionTypeEnum.FullCustomerFundsLiquidation, 
                            currentYearMonthStr,
                            updatedDateTimeUtc,
                            out lastInvestmentCredit);

                _db.InvBalances.AddOrUpdate(i => new { i.CustomerId, i.YearMonth },
                    new InvBalance
                    {
                        YearMonth = currentYearMonthStr,
                        CustomerId = customerId,
                        Balance = 0,
                        //CashBalance = remainingCashAmount,
                        InvGainLoss = invGainLoss,
                        // Save cash from sale trx and last month's credit
                        CashInvestment = -(remainingCashAmount + lastInvestmentCredit),
                        UpdatedOnUtc = updatedDateTimeUtc
                    });

                _customerFundSharesRepo.SaveDbMonthlyCustomerFundShares(boughtShares: false, customerId: customerId,
                    fundsShares: portfolioFunds,
                    year: yearCurrent,
                    month: monthCurrent,
                    updatedOnUtc: updatedDateTimeUtc);

                _db.Database.Log = null;

            });
        }

        #endregion Selling


        /// <summary>
        /// Calculate financial information for a customer on a given month
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <param name="cashToInvest">The positive cash to buy shares</param>
        /// <param name="monthlyBalance">Out -> Monthly Cash Value useful in summary page</param>
        /// <param name="invGainLoss">Out -> Monthly Gain or Loss in cash value Used in summary page</param>
        /// <param name="monthsPortfolioRisk"></param>
        /// <returns></returns>
        private Dictionary<int, PortfolioFundDTO>
            GetCustomerSharesBalancesForMonth(
                int customerId,
                int yearCurrent,
                int monthCurrent,
                decimal cashToInvest,
                out decimal monthlyBalance,
                out decimal invGainLoss,
                out RiskToleranceEnum monthsPortfolioRisk)
        {

            // Buy if cashToInvest amount is positive otherwise if == 0 reprice portfolio
            var fundSharesThisMonth = _customerFundSharesRepo.GetMonthlyFundSharesAfterBuyingSelling(
                customerId,
                cashToInvest,
                yearCurrent,
                monthCurrent,
                out monthsPortfolioRisk);

            GetSharesBalanceThisMonth(customerId, fundSharesThisMonth, yearCurrent, monthCurrent, out monthlyBalance, out invGainLoss);

            return fundSharesThisMonth;
        }


        /// <summary>
        /// 
        /// Calculate the customers investment shares balance this month
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="portfolioFundsValuesThisMonth"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <param name="customerMonthsBalance"></param>
        /// <param name="invGainLoss"></param>
        private void GetSharesBalanceThisMonth(
            int customerId,
            Dictionary<int, PortfolioFundDTO> portfolioFundsValuesThisMonth,
            int yearCurrent,
            int monthCurrent,
            out decimal customerMonthsBalance,
            out decimal invGainLoss)
        {

            string yearMonthCurrentStr = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent);

            var monthlySharesValue = portfolioFundsValuesThisMonth.Sum(f => f.Value.SharesValue);
            var newSharesVal = portfolioFundsValuesThisMonth.Sum(f => f.Value.NewSharesValue);
            var prevMonthsSharesPricedNow = monthlySharesValue - newSharesVal;

            var soldVintagesMarketAmount = _db.InvBalances
                .Where(sv => sv.CustomerId == customerId
                    && sv.SoldYearMonth == yearMonthCurrentStr
                    && sv.Sold
                    )
                .Sum(sv => (decimal?)sv.SoldAmount) ?? 0;

            var prevMonthsSharesBalance = GetPrevMonthInvestmentBalance(customerId, yearCurrent, monthCurrent);

            // if portfolio is liquidated in whole or partly then invGainLoss has no meaning
            invGainLoss = monthlySharesValue > 0
                ? DbExpressions.RoundCustomerBalanceAmount(
                        prevMonthsSharesPricedNow
                      - prevMonthsSharesBalance
                      + soldVintagesMarketAmount)
                : 0;

            customerMonthsBalance = monthlySharesValue > 0
                ? DbExpressions.RoundCustomerBalanceAmount(monthlySharesValue)
                : 0;
        }

        /// <summary>
        /// Get the previous month's investment balance
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <returns></returns>
        private decimal GetPrevMonthInvestmentBalance(int customerId, int yearCurrent, int monthCurrent)
        {

            // Temp expressions
            DateTime prevYearMonth = new DateTime(yearCurrent, monthCurrent, 1).AddMonths(-1);
            var prevYearMonthStr = DbExpressions.GetStrYearMonth(prevYearMonth.Year, prevYearMonth.Month);

            // Get the previous month's value
            var prevMonthBalAmount = _db.InvBalances
                .Where(b => b.CustomerId == customerId &&
                            string.Compare(b.YearMonth, prevYearMonthStr, StringComparison.Ordinal) <= 0)
                .OrderByDescending(b => b.YearMonth)
                .Select(b => b.Balance)
                .FirstOrDefault();

            return prevMonthBalAmount;
        }

        /// <summary>
        /// 
        /// Save in the database customer account the monthly balances
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <param name="cashToInvest">Positive cash amount to invest</param>
        /// <param name="lowRiskShares"></param>
        /// <param name="mediumRiskShares"></param>
        /// <param name="highRiskShares"></param>
        /// <param name="totalCashInvestments"></param>
        /// <param name="totalSoldVintagesValue"></param>
        /// <param name="begGmBalance"></param>
        /// <param name="deposits"></param>
        /// <param name="withdrawals"></param>
        /// <param name="gamingGainLoss"></param>
        /// <param name="endGmBalance"></param>
        /// <param name="totalCashInvInHold"></param>
        private void SaveDbCustomerMonthlyBalanceByCashInv(
                int customerId,
                int yearCurrent,
                int monthCurrent,
                decimal cashToInvest,
                decimal lowRiskShares,
                decimal mediumRiskShares,
                decimal highRiskShares,
                decimal begGmBalance,
                decimal deposits,
                decimal withdrawals,
                decimal gamingGainLoss,
                decimal endGmBalance,
                decimal totalCashInvInHold,
                decimal totalCashInvestments,
                decimal totalSoldVintagesValue
            )
        {

            decimal monthlyBalance, invGainLoss;
            RiskToleranceEnum userPortfolioRiskSelection;
            var portfolioFunds = GetCustomerSharesBalancesForMonth(
                customerId,
                yearCurrent,
                monthCurrent,
                cashToInvest,
                out monthlyBalance,
                out invGainLoss,
                out userPortfolioRiskSelection);

            /************ Update Monthly Balance *****************/

            ConnRetryConf.TransactWithRetryStrategy(_db,

                () =>
                {

                    SetDbMonthlyClearance(
                        customerId,
                        yearCurrent,
                        monthCurrent,
                        cashToInvest,
                        userPortfolioRiskSelection,
                        monthlyBalance,
                        invGainLoss,
                        lowRiskShares,
                        mediumRiskShares,
                        highRiskShares,
                        begGmBalance,
                        deposits,
                        withdrawals,
                        gamingGainLoss,
                        endGmBalance,
                        totalCashInvInHold,
                        totalCashInvestments,
                        totalSoldVintagesValue,
                        portfolioFunds);

                });
        }

        private void SetDbMonthlyClearance(
                int customerId,
                int yearCurrent,
                int monthCurrent,
                decimal cashToInvest,
                RiskToleranceEnum userPortfolioRiskSelection,
                decimal monthlyBalance,
                decimal invGainLoss,
                decimal lowRiskShares,
                decimal mediumRiskShares,
                decimal highRiskShares,
                decimal begGmBalance,
                decimal deposits,
                decimal withdrawals,
                decimal gamingGainLoss,
                decimal endGmBalance,
                decimal totalCashInvInHold,
                decimal totalCashInvestments,
                decimal totalSoldVintagesValue,
                Dictionary<int, PortfolioFundDTO> portfolioFunds)
        {

            var updatedOnUtc = DateTime.UtcNow;

            // Save the portfolio for the month
            _custPortfolioRepo.SaveDbCustMonthsPortfolioMix(
                customerId,
                userPortfolioRiskSelection,
                yearCurrent,
                monthCurrent,
                updatedOnUtc);

            UpsInvBalance(
                customerId,
                userPortfolioRiskSelection,
                yearCurrent,
                monthCurrent,
                cashToInvest,
                monthlyBalance,
                invGainLoss,
                lowRiskShares,
                mediumRiskShares,
                highRiskShares,
                begGmBalance,
                deposits,
                withdrawals,
                gamingGainLoss,
                endGmBalance,
                totalCashInvInHold,
                totalCashInvestments,
                totalSoldVintagesValue,
                updatedOnUtc);

            _customerFundSharesRepo.SaveDbMonthlyCustomerFundShares(
                boughtShares: true,
                customerId: customerId,
                fundsShares: portfolioFunds,
                year: yearCurrent,
                month: monthCurrent,
                updatedOnUtc: updatedOnUtc);
        }

        /// <summary>
        /// Upsert an investment balance monthly row
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="userPortfolioRiskSelection"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <param name="cashToInvest"></param>
        /// <param name="monthlyBalance"></param>
        /// <param name="invGainLoss"></param>
        /// <param name="lowRiskShares"></param>
        /// <param name="mediumRiskShares"></param>
        /// <param name="highRiskShares"></param>
        /// <param name="begGmBalance"></param>
        /// <param name="deposits"></param>
        /// <param name="withdrawals"></param>
        /// <param name="gamingGainLoss"></param>
        /// <param name="endGmBalance"></param>
        /// <param name="totalCashInvInHold"></param>
        /// <param name="totalCashInvestments"></param>
        /// <param name="totalSoldVintagesValue"></param>
        /// <param name="updatedOnUtc"></param>
        public void UpsInvBalance(
                int customerId,
                RiskToleranceEnum userPortfolioRiskSelection,
                int yearCurrent,
                int monthCurrent,
                decimal cashToInvest,
                decimal monthlyBalance,
                decimal invGainLoss,
                decimal lowRiskShares,
                decimal mediumRiskShares,
                decimal highRiskShares,
                decimal begGmBalance,
                decimal deposits,
                decimal withdrawals,
                decimal gamingGainLoss,
                decimal endGmBalance,
                decimal totalCashInvInHold,
                decimal totalCashInvestments,
                decimal totalSoldVintagesValue,
                DateTime updatedOnUtc)
        {

            _db.InvBalances.AddOrUpdate(i => new { i.CustomerId, i.YearMonth },
                new InvBalance
                {
                    YearMonth = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent),
                    CustomerId = customerId,
                    PortfolioId = (int)userPortfolioRiskSelection,
                    Balance = monthlyBalance,
                    InvGainLoss = invGainLoss,
                    CashInvestment = cashToInvest,
                    LowRiskShares = lowRiskShares,
                    MediumRiskShares = mediumRiskShares,
                    HighRiskShares = highRiskShares,
                    BegGmBalance = begGmBalance,
                    Deposits = deposits,
                    Withdrawals = withdrawals,
                    GmGainLoss = gamingGainLoss,
                    EndGmBalance = endGmBalance,
                    TotalCashInvInHold = totalCashInvInHold,
                    TotalCashInvestments = totalCashInvestments,
                    TotalSoldVintagesValue = totalSoldVintagesValue,
                    UpdatedOnUtc = updatedOnUtc
                });
        }

        /// <summary>
        /// 
        /// Process All Monthly Balances for a single customer whether they have transactions or not.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="startYearMonthStr">If null assuming -> GzTransactions.Min(t => t.YearMonthCtd)</param>
        /// <param name="endYearMonthStr">If null assuming -> Now</param>
        public void SaveDbCustomerAllMonthlyBalances(
            int customerId,
            string startYearMonthStr = null,
            string endYearMonthStr = null)
        {

            // Prep in month parameters
            startYearMonthStr = GetTrxMinMaxMonths(startYearMonthStr, ref endYearMonthStr);

            // Loop through all the months activity
            while (startYearMonthStr.BeforeEq(endYearMonthStr))
            {

                // TODO : Replace
                //SaveDbCustomerMonthlyBalance(customerId, startYearMonthStr);

                // month ++
                startYearMonthStr = DbExpressions.AddMonth(startYearMonthStr);
            }
        }

        /// <summary>
        /// 
        /// Process All Monthly Customer Balances whether they have transactions or not.
        /// 
        /// </summary>
        /// <param name="startYearMonthStr">If null assuming -> GzTransactions.Min(t => t.YearMonthCtd)</param>
        /// <param name="endYearMonthStr">If null assuming -> Now</param>
        public void SaveDbAllCustomersMonthlyBalances(string startYearMonthStr = null, string endYearMonthStr = null)
        {

            startYearMonthStr = GetTrxMinMaxMonths(startYearMonthStr, ref endYearMonthStr);

            var activeCustomerIds = _gzTransactionRepo.GetActiveCustomers(startYearMonthStr, endYearMonthStr);

            foreach (var customerId in activeCustomerIds)
            {

                SaveDbCustomerAllMonthlyBalances(customerId, startYearMonthStr, endYearMonthStr);
            }

        }

        /// <summary>
        /// 
        /// Set default transaction processing months
        /// minimum: earliest transaction month
        /// maximum: the present month.
        /// 
        /// </summary>
        /// <param name="startYearMonthStr"></param>
        /// <param name="endYearMonthStr"></param>
        /// <returns>earliest StartYearMonth</returns>
        private string GetTrxMinMaxMonths(string startYearMonthStr, ref string endYearMonthStr)
        {

            if (string.IsNullOrEmpty(startYearMonthStr))
            {
                startYearMonthStr = _db.GzTrxs.Min(t => t.YearMonthCtd);
            }
            if (string.IsNullOrEmpty(endYearMonthStr))
            {
                endYearMonthStr = DateTime.UtcNow.ToStringYearMonth();
            }
            return startYearMonthStr;
        }

        /// <summary>
        /// 
        /// Overloaded: Process the investment and cash balance for a single customer on a single month.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="thisYearMonth"></param>
        public void SaveDbCustomerMonthlyBalance(int customerId, string thisYearMonth)
        {

            var customerMonthlyTrx =
                _db.GzTrxs
                    .Where(t => t.CustomerId == customerId && t.YearMonthCtd == thisYearMonth)
                    .GroupBy(t => t.YearMonthCtd)
                    .SingleOrDefault();

            var yearCurrent = int.Parse(thisYearMonth.Substring(0, 4));
            var monthCurrent = int.Parse(thisYearMonth.Substring(4, 2));

            // Call sibling
            SaveDbCustomerMonthlyBalance(customerId, customerMonthlyTrx, yearCurrent, monthCurrent);
        }

        /// <summary>
        /// 
        /// Overloaded: Process the investment and cash balance for a single customer on a single month.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerMonthlyTrxs"></param>
        private void SaveDbCustomerMonthlyBalance(int customerId, IGrouping<string, GzTrx> customerMonthlyTrxs)
        {

            int yearCurrent = 0, monthCurrent = 0;

            // Initialize year month if needed
            if (yearCurrent == 0 && customerMonthlyTrxs != null)
            {

                yearCurrent = int.Parse(customerMonthlyTrxs.Key.Substring(0, 4));

            }
            if (monthCurrent == 0 && customerMonthlyTrxs != null)
            {

                monthCurrent = int.Parse(customerMonthlyTrxs.Key.Substring(4, 2));
            }

            SaveDbCustomerMonthlyBalance(customerId, customerMonthlyTrxs, yearCurrent, monthCurrent);
        }

        /// <summary>
        /// 
        /// Overloaded: Process the investment and cash balance for a single customer on a single month.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerMonthlyTrxs"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        private void SaveDbCustomerMonthlyBalance(int customerId, IGrouping<string, GzTrx> customerMonthlyTrxs, int yearCurrent, int monthCurrent)
        {

            if (yearCurrent == 0 || monthCurrent == 0)
            {
                _logger.Error("SaveDbCustomerMonthlyBalance(): Cannot have either year or month equal to 0");
            }

            // Process
            var monthlyCashToInvest = GetMonthlyCashToInvest(customerMonthlyTrxs);

            if (monthlyCashToInvest >= 0)
            {
                // TODO : Replace
                //SaveDbCustomerMonthlyBalanceByCashInv(customerId, yearCurrent, monthCurrent, monthlyCashToInvest);
            }
        }

        /// <summary>
        /// 
        /// Calculate monthly aggregated cash amount to invest by examining all the 
        /// customer monthly transactions.
        /// 
        /// </summary>
        /// <param name="monthlyTrxGrouping"></param>
        /// <returns></returns>
        private decimal GetMonthlyCashToInvest(IGrouping<string, GzTrx> monthlyTrxGrouping)
        {

            if (monthlyTrxGrouping == null)
            {
                return 0;
            }

            var monthlyCashToInvest = 0M;

            // These Ids throw exceptions when looked up as navigation properties in a transaction.
            var liquidatedTypeId =
                _db.GzTrxTypes.Where(tt => tt.Code == GzTransactionTypeEnum.FullCustomerFundsLiquidation)
                    .Select(tt => tt.Id)
                    .Single();

            var liquidatedMonth = monthlyTrxGrouping
                .Any(t => t.TypeId == liquidatedTypeId);

            // don't buy stock if the account was liquidated this month
            if (!liquidatedMonth)
            {

                var creditedPlayingLossTypeId =
                    _db.GzTrxTypes.Where(tt => tt.Code == GzTransactionTypeEnum.CreditedPlayingLoss)
                        .Select(tt => tt.Id)
                        .Single();

                monthlyCashToInvest =
                    monthlyTrxGrouping
                        .Where(t => t.TypeId == creditedPlayingLossTypeId)
                        .Select(t => t.Amount)
                        .SingleOrDefault();

            }

            // --------------- Net amount to invest -------------------------
            return monthlyCashToInvest;
        }
    }
}
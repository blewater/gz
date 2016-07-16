using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using gzDAL.Models;
using NLog;
using Z.EntityFramework.Plus;

namespace gzDAL.Repos {
    public class InvBalanceRepo : IInvBalanceRepo {
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
        /// CacheBalance and ask it asynchronously.
        /// 
        /// Meant to be used with GetCachedLatestBalanceTimestamp() if possible after a short time delay.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public Task<IEnumerable<InvBalance>> CacheLatestBalance(int customerId) {

            var lastBalanceRowTask = _db.InvBalances
                .Where(i => i.CustomerId == customerId 
                    && i.YearMonth == _db.InvBalances.Where(b=>b.CustomerId == i.CustomerId)
                        .Select(b=>b.YearMonth)
                        .Max())
                // Cache 4 hours
                .FromCacheAsync(DateTime.UtcNow.AddHours(4));

            return lastBalanceRowTask;
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
        public async Task<Tuple<decimal, DateTime?>> GetCachedLatestBalanceTimestampAsync(Task<IEnumerable<InvBalance>> lastBalanceRowTask) {

            var res = await lastBalanceRowTask;

            var lastMonthsBalanceRow = res
                .Select(b => new { b.Balance, b.UpdatedOnUtc })
                .SingleOrDefault();

            var lastUpdatedBalanceOn = lastMonthsBalanceRow?.UpdatedOnUtc;

            return Tuple.Create(lastMonthsBalanceRow?.Balance??0, lastUpdatedBalanceOn);
        }

        /// <summary>
        /// 
        /// Cache investment returns and ask it asynchronously.
        /// 
        /// Meant to be used with CacheInvestmentReturns() if possible after a short time delay.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public Task<Decimal> CacheInvestmentReturns(int customerId) {

            var invGainSumTask = _db.InvBalances
                .Where(i => i.CustomerId == customerId)
                .Select(i=>i.InvGainLoss)
                .DefaultIfEmpty(0)
                .DeferredSum()
                // Cache 4 Hours
                .FromCacheAsync(DateTime.UtcNow.AddHours(4));

            return invGainSumTask;
        }

        /// <summary>
        /// 
        /// Call this after CacheInvestmentReturns() to get the investment result sum.
        /// 
        /// </summary>
        /// <param name="invGainSumTask"></param>
        /// <returns>Balance Amount of last month.</returns>
        public async Task<decimal> GetCachedInvestmentReturnsAsync(Task<decimal> invGainSumTask) {

            var invGainSum = await invGainSumTask;

            return invGainSum;
        }

        #region Vintages

        /// <summary>
        /// 
        /// Get User Vintages from InvBalance.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public ICollection<VintageDto> GetCustomerVintages(int customerId) {

            //var cacheDuration = new CacheItemPolicy() {
            //    SlidingExpiration = TimeSpan.FromDays(1)
            //};
            var task = _db.GzConfigurations
                .FromCacheAsync(DateTime.UtcNow.AddDays(1));
            var confRow = task.Result;

            var lockInDays = confRow
                .Select(c => c.LOCK_IN_NUM_DAYS)
                .Single();

            var vintagesList = _db.Database
                .SqlQuery<VintageDto>(
                    "SELECT InvBalanceId, YearMonthStr, InvestmentAmount, Sold, SellingValue," + 
                    " SoldFees, SoldYearMonth FROM dbo.GetVintages(@CustomerId)",
                    new SqlParameter("@CustomerId", customerId))
                .ToList();

            foreach (var dto in vintagesList) {
                dto.Locked = lockInDays -
                             (DateTime.UtcNow - DbExpressions.GetDtYearMonthStrToEndOfMonth(dto.YearMonthStr)).TotalDays > 0;
            }

            return vintagesList;
        }

        /// <summary>
        /// 
        /// Calculate the vintage in latest fund market value
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearMonthStr"></param>
        /// <param name="monthsCustomerFunds"></param>
        /// <param name="fees"></param>
        /// <returns></returns>
        private decimal GetVintageValuePricedNow(
            int customerId, 
            string yearMonthStr,
            out IEnumerable<CustFundShareDto> monthsCustomerFunds,
            out decimal fees) {

            monthsCustomerFunds = _customerFundSharesRepo.GetMonthsBoughtFundsValue(
                customerId,
                yearMonthStr);

            // *NewShares* values meaning purchased on this month
            var monthsNewSharesPrice = monthsCustomerFunds.Sum(f => f.NewSharesValue??0);

            fees = _gzTransactionRepo.GetWithdrawnFees(monthsNewSharesPrice);

            return monthsNewSharesPrice;
        }

        /// <summary>
        /// 
        /// Recalculate the vintages selling value with present market values used for selling them.
        /// 
        /// Checks for selling preconditions before attempting to return the present selling value.
        /// 
        /// Throws an exception if vintage is already sold or not available for selling.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <returns></returns>
        public void SetVintagesMarketPrices(int customerId, IEnumerable<VintageDto> vintages) {

            foreach (var vintageDto in vintages) {

                if (vintageDto.Selected) {

                    SetVinageLatestSoldStatus(vintageDto);

                    if (VintageSatisfiesSellingPreConditions(customerId, vintageDto)) {

                        decimal fees;
                        IEnumerable<CustFundShareDto> monthsCustomerShares;
                        vintageDto.MarketPrice = GetVintageValuePricedNow(
                            customerId,
                            vintageDto.YearMonthStr,
                            out monthsCustomerShares,
                            out fees);

                        vintageDto.CustomerVintageShares = monthsCustomerShares;
                        vintageDto.Fees = fees;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// Check before attempting to sell a vintage that it meets the preconditions.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintageDto"></param>
        private bool VintageSatisfiesSellingPreConditions(int customerId, VintageDto vintageDto) {

            // Assume ok to proceed
            bool sellThisVintage = true;

            if (vintageDto.Locked) {
                int vinYear = int.Parse(vintageDto.YearMonthStr.Substring(0, 4)),
                    vinMonth = int.Parse(vintageDto.YearMonthStr.Substring(4, 2));
                _logger.Error(
                    "For customer: {0}, the vintage for the year of {1} and month: {2} is locked and not available for selling it. !",
                    customerId, vinYear, vinMonth);

                sellThisVintage = false;
            }
            // Not Locked
            else { 

                if (vintageDto.Sold) {
                    int vinYear = int.Parse(vintageDto.YearMonthStr.Substring(0, 4)),
                        vinMonth = int.Parse(vintageDto.YearMonthStr.Substring(4, 2));
                    _logger.Error(
                        "For customer: {0}, the vintage for the year of {1} and month: {2} cannot be resold !",
                        customerId, vinYear, vinMonth);

                    sellThisVintage = false;
                }
            }

            return sellThisVintage;
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
        public ICollection<VintageDto> GetCustomerVintagesSellingValue(int customerId) {

            var customerVintages = GetCustomerVintages(customerId)
                .ToList();
            GetCustomerVintagesSellingValue(customerId, customerVintages);

            return customerVintages;
        }

        /// <summary>
        /// 
        /// Get the vintages with the selling value calculated if not sold.
        /// 
        /// Calculate the vintage in latest fund market value and deduct fees. The amount the customer
        /// would receive.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerVintages"></param>
        /// <returns></returns>
        public ICollection<VintageDto> GetCustomerVintagesSellingValue(int customerId, List<VintageDto> customerVintages) {

            foreach (var dto in customerVintages
                .Where(v=>v.SellingValue==0 && !v.Locked)) {

                // out var declarations
                IEnumerable<CustFundShareDto> customerVintageShares;
                decimal fees;

                // Call to calculate latest selling price
                decimal vintageMarketPrice = GetVintageValuePricedNow(
                        customerId,
                        dto.YearMonthStr,
                        out customerVintageShares,
                        out fees);

                // Save the selling price and shares
                dto.CustomerVintageShares = customerVintageShares;
                dto.SellingValue = vintageMarketPrice - fees;
            }

            return customerVintages;
        }

        /// <summary>
        /// 
        /// Read latest sold value from the database
        /// 
        /// We care only if it appears not sold yet.
        /// 
        /// </summary>
        /// <param name="vintageDto"></param>
        private void SetVinageLatestSoldStatus(VintageDto vintageDto) {

            if (!vintageDto.Sold) {

                vintageDto.Sold = _db.InvBalances
                    .Any(v => v.Id == vintageDto.InvBalanceId
                              && v.Sold
                    );
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
        /// <returns></returns>
        public void SaveDbSellVintages(int customerId, ICollection<VintageDto> vintages) {

            ConnRetryConf.TransactWithRetryStrategy(_db,

            () => {

                _gzTransactionRepo.SaveDbSellVintages(customerId, vintages);

            });
        }

        #endregion Vintages
        #region Fund Shares Selling

        /// <summary>
        /// 
        /// Sell completely a customer's owned funds shares.
        /// 
        /// Supports selling shares in previous months respective the fund prices of those months.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="updatedDateTimeUtc">The database creation time-stamp.</param>
        /// <param name="monthsPortfolioRisk"></param>
        /// <param name="yearCurrent">Optional year value for selling in the past</param>
        /// <param name="monthCurrent">Optional month value for selling in the past</param>
        public bool SaveDbSellAllCustomerFundsShares(
            int customerId, 
            DateTime updatedDateTimeUtc,
            out RiskToleranceEnum monthsPortfolioRisk,
            int yearCurrent = 0, 
            int monthCurrent = 0) {

            // Assume we don't sell shares
            var soldShares = false;

            if (yearCurrent == 0) {
                yearCurrent = DateTime.UtcNow.Year;
            }
            if (monthCurrent == 0) {
                monthCurrent = DateTime.UtcNow.Month;
            }

            if (new DateTime(yearCurrent, monthCurrent, 1) > DateTime.UtcNow) {

                throw new Exception("Cannot sell the customer's (id: " + customerId + ") portfolio in the future.");
            }

            // Calculate the value of the fund shares
            var portfolioFundsValuesThisMonth = _customerFundSharesRepo.GetMonthlyFundSharesAfterBuyingSelling(
                customerId, 
                0, 
                yearCurrent, 
                monthCurrent, 
                out monthsPortfolioRisk);

            // Make sure we have shares to sell
            if (portfolioFundsValuesThisMonth.Sum(f => f.Value.SharesNum) > 0) {

                decimal invGainLoss, monthlyBalance;
                GetSharesBalanceThisMonth(customerId, portfolioFundsValuesThisMonth, yearCurrent, monthCurrent, out monthlyBalance, out invGainLoss);

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
        private void SaveDbLiquidateCustomerPortfolio(
            Dictionary<int, PortfolioFundDTO> portfolioFunds,
            int customerId,
            int yearCurrent,
            int monthCurrent,
            decimal newMonthlyBalance,
            decimal invGainLoss,
            RiskToleranceEnum monthsPortfolioRisk,
            DateTime updatedDateTimeUtc) {

            /****************** Liquidate a month ****************/

            ConnRetryConf.TransactWithRetryStrategy(_db,

            () => {

                // Save the portfolio for the month
                _custPortfolioRepo.SaveDbCustMonthsPortfolioMix(
                    customerId, 
                    monthsPortfolioRisk, 
                    yearCurrent, 
                    monthCurrent, 
                    updatedDateTimeUtc);

                // Save fees transactions first and continue with reduced cash amount
                var remainingCashAmount =
                        _gzTransactionRepo.SaveDbLiquidatedPortfolioWithFees(
                            customerId,
                            newMonthlyBalance,
                            GzTransactionTypeEnum.FullCustomerFundsLiquidation,
                            updatedDateTimeUtc);

                    _db.InvBalances.AddOrUpdate(i => new { i.CustomerId, i.YearMonth },
                        new InvBalance {
                            YearMonth = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent),
                            CustomerId = customerId,
                            Balance = 0,
                            CashBalance = remainingCashAmount,
                            InvGainLoss = invGainLoss,
                            CashInvestment = -remainingCashAmount,
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
        /// <returns></returns>
        public Dictionary<int, PortfolioFundDTO>
            GetCustomerSharesBalancesForMonth(
                int customerId,
                int yearCurrent,
                int monthCurrent,
                decimal cashToInvest,
                out decimal monthlyBalance,
                out decimal invGainLoss,
                out RiskToleranceEnum monthsPortfolioRisk) {

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
            out decimal invGainLoss) {

            string yearMonthCurrentStr = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent);

            var monthlySharesValue = portfolioFundsValuesThisMonth.Sum(f => f.Value.SharesValue);
            var newSharesVal = portfolioFundsValuesThisMonth.Sum(f => f.Value.NewSharesValue);
            var prevMonthsSharesPricedNow = monthlySharesValue - newSharesVal;

            var soldVintagesMarketAmount = _db.InvBalances
                .Where(sv => sv.CustomerId == customerId 
                    && sv.SoldYearMonth == yearMonthCurrentStr
                    && sv.Sold
                    )
                .Sum(sv => (decimal ?) sv.SoldAmount) ?? 0;

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
        private decimal GetPrevMonthInvestmentBalance(int customerId, int yearCurrent, int monthCurrent) {

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
        private void SaveDbCustomerMonthlyBalanceByCashInv(int customerId, int yearCurrent, int monthCurrent, decimal cashToInvest) {

            decimal monthlyBalance, invGainLoss;
            RiskToleranceEnum monthsPortfolioRisk;
            var portfolioFunds = GetCustomerSharesBalancesForMonth(
                customerId, 
                yearCurrent, 
                monthCurrent, 
                cashToInvest, 
                out monthlyBalance,
                out invGainLoss, 
                out monthsPortfolioRisk);

            /************ Update Monthly Balance *****************/

            ConnRetryConf.TransactWithRetryStrategy(_db,

                () => {
                    var updatedOnUtc = DateTime.UtcNow;

                    // Save the portfolio for the month
                    _custPortfolioRepo.SaveDbCustMonthsPortfolioMix(
                        customerId,
                        monthsPortfolioRisk,
                        yearCurrent,
                        monthCurrent,
                        updatedOnUtc);

                    _db.InvBalances.AddOrUpdate(i => new { i.CustomerId, i.YearMonth },
                            new InvBalance {
                                YearMonth = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent),
                                CustomerId = customerId,
                                Balance = monthlyBalance,
                                InvGainLoss = invGainLoss,
                                CashInvestment = cashToInvest,
                                UpdatedOnUtc = updatedOnUtc
                    });

                    _customerFundSharesRepo.SaveDbMonthlyCustomerFundShares(
                        boughtShares: true,
                        customerId: customerId,
                        fundsShares: portfolioFunds,
                        year: yearCurrent,
                        month: monthCurrent,
                        updatedOnUtc: updatedOnUtc);

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
            string endYearMonthStr = null) {

            // Prep in month parameters
            startYearMonthStr = GetTrxMinMaxMonths(startYearMonthStr, ref endYearMonthStr);

            // Loop through all the months activity
            while (startYearMonthStr.BeforeEq(endYearMonthStr)) {

                SaveDbCustomerMonthlyBalance(customerId, startYearMonthStr);

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
        public void SaveDbAllCustomersMonthlyBalances(string startYearMonthStr = null, string endYearMonthStr = null) {

            startYearMonthStr = GetTrxMinMaxMonths(startYearMonthStr, ref endYearMonthStr);

            var activeCustomerIds = _gzTransactionRepo.GetActiveCustomers(startYearMonthStr, endYearMonthStr);

            foreach (var customerId in activeCustomerIds) {

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
        private string GetTrxMinMaxMonths(string startYearMonthStr, ref string endYearMonthStr) {

            if (string.IsNullOrEmpty(startYearMonthStr)) {
                startYearMonthStr = _db.GzTrxs.Min(t => t.YearMonthCtd);
            }
            if (string.IsNullOrEmpty(endYearMonthStr)) {
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
        public void SaveDbCustomerMonthlyBalance(int customerId, string thisYearMonth) {

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
        private void SaveDbCustomerMonthlyBalance(int customerId, IGrouping<string, GzTrx> customerMonthlyTrxs) {

            int yearCurrent = 0, monthCurrent = 0;

            // Initialize year month if needed
            if (yearCurrent == 0 && customerMonthlyTrxs != null) {

                yearCurrent = int.Parse(customerMonthlyTrxs.Key.Substring(0, 4));

            }
            if (monthCurrent == 0 && customerMonthlyTrxs != null) {

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
        private void SaveDbCustomerMonthlyBalance(int customerId, IGrouping<string, GzTrx> customerMonthlyTrxs, int yearCurrent, int monthCurrent) {

            if (yearCurrent == 0 || monthCurrent == 0) {
                _logger.Error("SaveDbCustomerMonthlyBalance(): Cannot have either year or month equal to 0");
            }

            // Process
            var monthlyCashToInvest = GetMonthlyCashToInvest(customerMonthlyTrxs);

            if (monthlyCashToInvest >= 0) {
                SaveDbCustomerMonthlyBalanceByCashInv(customerId, yearCurrent, monthCurrent, monthlyCashToInvest);
            }
        }
       
        /// <summary>
        /// 
        /// Check if the portfolio was previously sold and "resell it" otherwise that information will be lost.
        /// Relies on the existence of the GzTransactionJournalTypeEnum.PortfolioLiquidation transaction type 
        /// to trigger reselling.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// 
        private void SaveDbResellCustomerPortfolioIfSoldBefore(int customerId, int yearCurrent, int monthCurrent) {

            if (_gzTransactionRepo.GetLiquidationTrxCount(customerId, yearCurrent, monthCurrent)) {

                // -- Or in the rare of a portfolio liquidation support idempotency by reselling the portfolio
                var soldPortfolioTimestamp = _gzTransactionRepo.GetSoldPortfolioTimestamp(customerId, yearCurrent,
                    monthCurrent);
                RiskToleranceEnum monthsPortfolioRisk;
                SaveDbSellAllCustomerFundsShares(customerId, soldPortfolioTimestamp, out monthsPortfolioRisk, yearCurrent, monthCurrent);
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
        private decimal GetMonthlyCashToInvest(IGrouping<string, GzTrx> monthlyTrxGrouping) {

            if (monthlyTrxGrouping == null) {
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
            if (!liquidatedMonth) {

                var creditedPlayingLossTypeId =
                    _db.GzTrxTypes.Where(tt => tt.Code == GzTransactionTypeEnum.CreditedPlayingLoss)
                        .Select(tt => tt.Id)
                        .Single();

                monthlyCashToInvest =
                    monthlyTrxGrouping.Sum(t => t.TypeId == creditedPlayingLossTypeId ? t.Amount : 0);

            }

            // --------------- Net amount to invest -------------------------
            return monthlyCashToInvest;
        }

        #region By Transaction

        /// <summary>
        /// Save to Database the calculated customer monthly investment balances
        ///     for the given months
        /// -- Or --
        ///     by all monthly transaction activity of the customer.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="monthsToProc">Array of YYYYMM values i.e. [201601, 201502]. If null then select all months with this customers transactional activity.</param>
        public void SaveDbCustomerMonthlyBalancesByTrx(int customerId, string[] monthsToProc = null) {

            IQueryable<IGrouping<string, GzTrx>> monthlyTrx;

            // Step 1: Retrieve all Transactions by player activity
            if (monthsToProc == null || monthsToProc.Length == 0) {

                monthlyTrx = _db.GzTrxs.Where(t => t.CustomerId == customerId)
                    .OrderBy(t => t.YearMonthCtd)
                    .GroupBy(t => t.YearMonthCtd);
            }
            // Add filter condition: given months
            else {

                monthlyTrx = _db.GzTrxs.Where(t => t.CustomerId == customerId
                    && monthsToProc.Contains(t.YearMonthCtd))
                    .OrderBy(t => t.YearMonthCtd)
                    .GroupBy(t => t.YearMonthCtd);
            }

            SaveDbCustomerMonthlyBalancesByTrx(customerId, monthlyTrx);
        }

        /// <summary>
        /// 
        /// Called by public SaveDbCustomerMonthlyBalancesByTrx after selection of Monthly Transactions:
        /// To save to Database the calculated customer monthly investment balances
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerMonthlyTrxs"></param>
        private void SaveDbCustomerMonthlyBalancesByTrx(int customerId, IEnumerable<IGrouping<string, GzTrx>> customerMonthlyTrxs) {

            // Step 2: Loop monthly, calculate Balances based transaction and portfolios return
            foreach (var customerMonthlyTrx in customerMonthlyTrxs) {

                // Step 3: Calculate monthly cash balances before investment
                SaveDbCustomerMonthlyBalance(customerId, customerMonthlyTrx);
            }
        }

        /// <summary>
        /// Multiple Customers Version:
        /// Save to Database the calculated customer monthly investment balances
        ///     otherwise calculate monthly investment balances for all transaction activity months of the players.
        /// </summary>
        /// <param name="customerIds"></param>
        /// <param name="yearMonthsToProc"></param>
        public void SaveDbCustomersMonthlyBalancesByTrx(int[] customerIds, string[] yearMonthsToProc) {

            if (customerIds == null) {
                return;
            }

            foreach (var customerId in customerIds) {
                SaveDbCustomerMonthlyBalancesByTrx(customerId, yearMonthsToProc);
            }
        }

        #endregion

    }
}
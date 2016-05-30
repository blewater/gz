using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Globalization;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using gzDAL.Models;

namespace gzDAL.Repos {
    public class InvBalanceRepo : IInvBalanceRepo {
        private readonly ApplicationDbContext _db;
        private readonly ICustFundShareRepo _customerFundSharesRepo;
        private readonly IGzTransactionRepo _gzTransactionRepo;

        public InvBalanceRepo(ApplicationDbContext db, ICustFundShareRepo customerFundSharesRepo, IGzTransactionRepo gzTransactionRepo) {
            this._db = db;
            this._customerFundSharesRepo = customerFundSharesRepo;
            this._gzTransactionRepo = gzTransactionRepo;
        }

        /// <summary>
        /// 
        /// Return in UTC last updated value of the customer's investment balance calculation
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public DateTime GetLastUpdatedDateTime(int customerId) {

            DateTime lastUpdated = _db.InvBalances
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.Id)
                .Select(i => i.UpdatedOnUTC)
                .FirstOrDefault();

            if (lastUpdated.Year == 1) {
                lastUpdated = DateTime.Today;
            }

            return lastUpdated;
        }

        #region Fund Shares Selling

        /// <summary>
        /// 
        /// Calculate the vintage in latest fund market value and deduct fees. The amount the customer
        /// would receive.
        /// unless it has been sold already in which case return that value
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearMonthStr"></param>
        /// <returns>Sold Value with fees deducted. The amount the customer would receive.</returns>
        private decimal GetVintageSellingValue(int customerId, string yearMonthStr) {

            decimal fees;
            decimal vintageSellingPrice = GetVintageSellingPrice(customerId, yearMonthStr, out fees);

            return vintageSellingPrice - fees;
        }

        /// <summary>
        /// 
        /// Calculate the vintage in latest fund market value
        /// Unless it has been sold already which case return that value
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearMonthStr"></param>
        /// <param name="fees"></param>
        /// <returns>The market priced vintage.</returns>
        private decimal GetVintageSellingPrice(
            int customerId, 
            string yearMonthStr,
            out decimal fees) 
        {

            // Init return value
            decimal monthlySharesValue = 0;

            var vintageSoldValue =
                _db.SoldVintages
                    .Where(v => v.CustomerId == customerId
                                && v.VintageYearMonth == yearMonthStr)
                    .Select(v => new {Amount = v.MarketAmount, v.Fees})
                    .FirstOrDefault();

            if (vintageSoldValue != null) {

                fees = vintageSoldValue.Fees;
                monthlySharesValue = vintageSoldValue.Amount;

            }
            else {

                // If not sold already calculate it now
                IEnumerable<CustFundShare> monthsCustomerShares;
                monthlySharesValue = GetVintageValuePricedNow(
                    customerId, 
                    yearMonthStr, 
                    out monthsCustomerShares, 
                    out fees);
            }

            return monthlySharesValue;
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
            out IEnumerable<CustFundShare> monthsCustomerFunds,
            out decimal fees) {

            int yearCurrent = int.Parse(yearMonthStr.Substring(0, 4)),
                monthCurrent = int.Parse(yearMonthStr.Substring(4, 2));

            monthsCustomerFunds = _customerFundSharesRepo.GetMonthsBoughtFundsValue(
                customerId,
                yearCurrent,
                monthCurrent);

            var monthsNewSharesPrice = monthsCustomerFunds.Sum(f => f.SharesValue);

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
        private void SetVintagesMarketPrices(int customerId, IEnumerable<VintageDto> vintages) {

            foreach (var vintageDto in vintages) {

                if (vintageDto.Selected) {

                    ChkVintageSellingPreConditions(customerId, vintageDto);

                    decimal fees;
                    IEnumerable<CustFundShare> monthsCustomerShares;
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

        /// <summary>
        /// 
        /// Check before attempting to sell a vintage that it meets the preconditions.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintageDto"></param>
        private void ChkVintageSellingPreConditions(int customerId, VintageDto vintageDto) {

            if (vintageDto.Locked) {
                int vinYear = int.Parse(vintageDto.YearMonthStr.Substring(0, 4)),
                    vinMonth = int.Parse(vintageDto.YearMonthStr.Substring(4, 2));
                throw new Exception("For customer: " + customerId + ", the vintage for the year of " + vinYear + " and month: " +
                                    vinMonth + " is locked and not available for selling it. !");
            }

            var alreadySold =
                _db.SoldVintages
                    .Any(v => v.CustomerId == customerId
                              && v.VintageYearMonth == vintageDto.YearMonthStr);

            if (alreadySold) {
                int vinYear = int.Parse(vintageDto.YearMonthStr.Substring(0, 4)),
                    vinMonth = int.Parse(vintageDto.YearMonthStr.Substring(4, 2));
                throw new Exception("For customer: " + customerId + ", the vintage for the year of " + vinYear + " and month: " +
                                    vinMonth + " cannot be resold !");
            }
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
        public IEnumerable<VintageDto> GetCustomerVintagesSellingValue(int customerId) {

            var customerVintages = _gzTransactionRepo
                .GetCustomerVintages(customerId)
                .Select(v => new VintageDto() {
                     SellingValue = GetVintageSellingValue(
                         customerId, 
                         v.YearMonthStr),
                     InvestAmount = v.InvestAmount,
                     YearMonthStr = v.YearMonthStr,
                     Locked = v.Locked,
                     Sold = v.Sold
                 }).ToList();

            return customerVintages;
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
        public IEnumerable<VintageDto> SaveDbSellVintages(int customerId, IEnumerable<VintageDto> vintages) {

            SetVintagesMarketPrices(customerId, vintages);

            ConnRetryConf.TransactWithRetryStrategy(_db,

            () => {

                _gzTransactionRepo.SaveDbSellVintages(customerId, vintages);

                // TODO: Check if recalculating the balance is ok.
                // Recalculate last month's balance
                var startMonthBalanceToRecalc = DateTime.UtcNow.ToStringYearMonth();
                // last completed month that's cleared.
                var lastClearedMonth = startMonthBalanceToRecalc;
                SaveDbCustomerAllMonthlyBalances(
                    customerId, 
                    startMonthBalanceToRecalc, 
                    lastClearedMonth);

            });

            // Reload them & return them
            vintages = GetCustomerVintagesSellingValue(customerId);

            return vintages;
        }

        /// <summary>
        /// 
        /// Sell completely a customer's owned funds shares.
        /// 
        /// Supports selling shares in previous months respective the fund prices of those months.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="updatedDateTimeUtc">The database creation time-stamp.</param>
        /// <param name="yearCurrent">Optional year value for selling in the past</param>
        /// <param name="monthCurrent">Optional month value for selling in the past</param>
        public bool SaveDbSellAllCustomerFundsShares(int customerId, DateTime updatedDateTimeUtc, int yearCurrent = 0, int monthCurrent = 0) {

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
            var portfolioFundsValuesThisMonth = _customerFundSharesRepo.GetMonthlyFundSharesAfterBuyingSelling(customerId, 0, yearCurrent, monthCurrent);

            // Make sure we have shares to sell
            if (portfolioFundsValuesThisMonth.Sum(f => f.Value.SharesNum) > 0) {

                decimal invGainLoss, monthlyBalance;
                GetSharesBalanceThisMonth(customerId, portfolioFundsValuesThisMonth, yearCurrent, monthCurrent, out monthlyBalance, out invGainLoss);

                SaveDbLiquidateCustomerPortfolio(portfolioFundsValuesThisMonth, customerId, yearCurrent, monthCurrent, monthlyBalance, invGainLoss, updatedDateTimeUtc);

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
        /// <param name="updatedDateTimeUtc">Set the desired datetime stamp of the db operations</param>
        private void SaveDbLiquidateCustomerPortfolio(
            Dictionary<int, PortfolioFundDTO> portfolioFunds,
            int customerId,
            int yearCurrent,
            int monthCurrent,
            decimal newMonthlyBalance,
            decimal invGainLoss,
            DateTime updatedDateTimeUtc) {

            //ConnRetryConf.TransactWithRetryStrategy(_db,

            //() => {

                // Save fees transactions first and continue with reduced cash amount
                var remainingCashAmount =
                        _gzTransactionRepo.SaveDbLiquidatedPortfolioWithFees(
                            customerId,
                            newMonthlyBalance,
                            GzTransactionTypeEnum.FullCustomerFundsLiquidation,
                            updatedDateTimeUtc);

                    _customerFundSharesRepo.SaveDbMonthlyCustomerFundShares(boughtShares: false, customerId: customerId,
                        fundsShares: portfolioFunds,
                        year: yearCurrent,
                        month: monthCurrent,
                        updatedOnUtc: updatedDateTimeUtc);

                    _db.InvBalances.AddOrUpdate(i => new { i.CustomerId, i.YearMonth },
                        new InvBalance {
                            YearMonth = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent),
                            CustomerId = customerId,
                            Balance = 0,
                            CashBalance = remainingCashAmount,
                            InvGainLoss = invGainLoss,
                            CashInvestment = -remainingCashAmount,
                            UpdatedOnUTC = updatedDateTimeUtc
                        });
            //});
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
                out decimal invGainLoss) {

            // Buy if cashToInvest amount is positive otherwise if == 0 reprice portfolio
            var fundSharesThisMonth = _customerFundSharesRepo.GetMonthlyFundSharesAfterBuyingSelling(
                customerId,
                cashToInvest,
                yearCurrent,
                monthCurrent);

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
            var monthlySharesValue = portfolioFundsValuesThisMonth.Sum(f => f.Value.SharesValue);
            var newSharesVal = portfolioFundsValuesThisMonth.Sum(f => f.Value.NewSharesValue);
            var prevMonthsSharesPricedNow = monthlySharesValue - newSharesVal;

            var prevMonthsSharesBalance = GetPrevMonthInvestmentBalance(customerId, yearCurrent, monthCurrent);

            // if portfolio is liquidated in whole or partly then invGainLoss has no meaning
            invGainLoss = monthlySharesValue > 0
                ? DbExpressions.RoundCustomerBalanceAmount(prevMonthsSharesPricedNow - prevMonthsSharesBalance)
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
            var portfolioFunds = GetCustomerSharesBalancesForMonth(customerId, yearCurrent, monthCurrent, cashToInvest, out monthlyBalance,
                out invGainLoss);

            //ConnRetryConf.TransactWithRetryStrategy(_db,

            //    () => {

            _customerFundSharesRepo.SaveDbMonthlyCustomerFundShares(
                    boughtShares: true,
                    customerId: customerId,
                    fundsShares: portfolioFunds,
                    year: yearCurrent,
                    month: monthCurrent,
                    updatedOnUtc: DateTime.UtcNow);

            _db.InvBalances.AddOrUpdate(i => new { i.CustomerId, i.YearMonth },
                    new InvBalance {
                        YearMonth = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent),
                        CustomerId = customerId,
                        Balance = monthlyBalance,
                        InvGainLoss = invGainLoss,
                        CashInvestment = cashToInvest,
                        UpdatedOnUTC = DateTime.UtcNow
                    });

            _db.SaveChanges();
            //});
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
                throw new Exception("Cannot have either year or month equal to 0 inside SaveDbCustomerMonthlyBalances()");
            }

            // Process
            var monthlyCashToInvest = GetMonthlyCashToInvest(customerMonthlyTrxs);

            if (monthlyCashToInvest >= 0) {
                SaveDbCustomerMonthlyBalanceByCashInv(customerId, yearCurrent, monthCurrent, monthlyCashToInvest);
            }
            else {
                SaveDbResellCustomerPortfolioIfSoldBefore(customerId, yearCurrent, monthCurrent);
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

                SaveDbSellAllCustomerFundsShares(customerId, soldPortfolioTimestamp, yearCurrent, monthCurrent);
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

            // These Ids throw exceptions when looked up as navigation properties in a transaction.
            var creditedPlayingLossTypeId =
                _db.GzTrxTypes.Where(tt => tt.Code == GzTransactionTypeEnum.CreditedPlayingLoss)
                    .Select(tt => tt.Id)
                    .Single();

            var monthlyPlayingLosses = 
                monthlyTrxGrouping.Sum(t => t.TypeId == creditedPlayingLossTypeId ? t.Amount : 0);


            // TODO: Reexamine account liquidation
            // Reduce fees by the fees amount corresponding to the portfolio liquidation
            //if (fullAccountLiquidation > 0) {
            //    monthlyFees -= this._gzTransactionRepo.GetWithdrawnFees(fullAccountLiquidation);
            //}

            // --------------- Net amount to invest -------------------------
            var monthlyCashToInvest = monthlyPlayingLosses;

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
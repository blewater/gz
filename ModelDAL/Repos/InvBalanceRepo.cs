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
        /// Calculate the vintage in latest fund market value unless it has been sold which case returns that value
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        private decimal GetVintageSellingValue(int customerId, string yearMonthStr, int lastInvestmentId) {

            decimal monthlySharesValue = 0;

            int yearCurrent = int.Parse(yearMonthStr.Substring(0, 4)),
                monthCurrent = int.Parse(yearMonthStr.Substring(4, 2));

            var soldValue =
                _db.GzTransactions
                    .Where(t => t.Type.Code == GzTransactionJournalTypeEnum.TransferToGaming
                                && t.ParentTrxId == lastInvestmentId
                                && t.YearMonthCtd == yearMonthStr
                                && t.CustomerId == customerId
                                )
                    .Sum(t => (decimal?)t.Amount);

            // If not sold calculate it now
            if (!soldValue.HasValue) {

                var fundSharesThisMonth = _customerFundSharesRepo.GetMonthsBoughtFundsValue(
                        customerId,
                        yearCurrent,
                        monthCurrent);

                var monthsNewSharesValue = fundSharesThisMonth.Sum(f => f.Value.SharesValue);

                monthlySharesValue =
                    DbExpressions.RoundCustomerBalanceAmount(monthsNewSharesValue -
                                                             _gzTransactionRepo.GetWithdrawnFees(monthsNewSharesValue));
            }
            else {
                monthlySharesValue = soldValue.Value;
            }

            return monthlySharesValue;
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
                         v.YearMonthStr, 
                         v.LastInvestmentId),
                     InvestAmount = v.InvestAmount,
                     YearMonthStr = v.YearMonthStr,
                     Locked = v.Locked,
                     Sold = v.Sold
                 }).ToList();

            return customerVintages;
        }

        /// <summary>
        /// 
        /// Sell any vintages marked for selling. They are sold at the fund prices current
        /// as of this method call.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintages"></param>
        /// <returns></returns>
        public IEnumerable<VintageDto> SaveDbSellVintages(int customerId, IEnumerable<VintageDto> vintages) {

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

            ConnRetryConf.TransactWithRetryStrategy(_db,

                () => {

                    // Save fees transactions first and continue with reduced cash amount
                    var remainingCashAmount =
                        _gzTransactionRepo.SaveDbLiquidatedPortfolioWithFees(
                            customerId,
                            newMonthlyBalance,
                            GzTransactionJournalTypeEnum.FullCustomerFundsLiquidation,
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

            ConnRetryConf.TransactWithRetryStrategy(_db,

                () => {

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
                startYearMonthStr = _db.GzTransactions.Min(t => t.YearMonthCtd);
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
                _db.GzTransactions
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
        private void SaveDbCustomerMonthlyBalance(int customerId, IGrouping<string, GzTransaction> customerMonthlyTrxs) {

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
        private void SaveDbCustomerMonthlyBalance(int customerId, IGrouping<string, GzTransaction> customerMonthlyTrxs, int yearCurrent, int monthCurrent) {

            if (yearCurrent == 0 || monthCurrent == 0) {
                throw new Exception("Cannot have either year or month equal to 0 inside SaveDbCustomerMonthlyBalances()");
            }

            // Process
            var monthlyCashToInvest = GetMonthlyCashToInvest(customerMonthlyTrxs);

            if (monthlyCashToInvest >= 0) {
                SaveDbCustomerMonthlyBalanceByCashInv(customerId, yearCurrent, monthCurrent, monthlyCashToInvest);
            }

            if (monthlyCashToInvest == 0) {
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
        private decimal GetMonthlyCashToInvest(IGrouping<string, GzTransaction> monthlyTrxGrouping) {

            if (monthlyTrxGrouping == null) {
                return 0;
            }

            var monthlyPlayingLosses = monthlyTrxGrouping.Sum(t => t.Type.Code == GzTransactionJournalTypeEnum.CreditedPlayingLoss ? t.Amount : 0);

            var monthlyWithdrawnAmounts = monthlyTrxGrouping.Sum(t => t.Type.Code == GzTransactionJournalTypeEnum.InvWithdrawal ? t.Amount : 0);

            var monthlyTransfersToGaming =
                monthlyTrxGrouping.Sum(t => t.Type.Code == GzTransactionJournalTypeEnum.TransferToGaming ? t.Amount : 0);

            var monthlyFees =
                monthlyTrxGrouping.Sum(
                    t =>
                        t.Type.Code == GzTransactionJournalTypeEnum.GzFees || t.Type.Code == GzTransactionJournalTypeEnum.FundFee
                            ? t.Amount
                            : 0);

            var monthlyLiquidationAmount =
                monthlyTrxGrouping.Sum(
                    t => t.Type.Code == GzTransactionJournalTypeEnum.FullCustomerFundsLiquidation ? t.Amount : 0);

            // Reduce fees by the fees amount corresponding to the portfolio liquidation
            if (monthlyLiquidationAmount > 0) {
                monthlyFees -= this._gzTransactionRepo.GetWithdrawnFees(monthlyLiquidationAmount);
            }

            // --------------- Net amount to invest -------------------------
            var monthlyCashToInvest = monthlyPlayingLosses - monthlyWithdrawnAmounts - monthlyTransfersToGaming -
                                      monthlyFees;

            // ---------------------------------------------------------------------------------

            // Negative amount has no meaning in the context of investing or buying stock this month
            if (monthlyCashToInvest < 0) monthlyCashToInvest = 0;

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

            IQueryable<IGrouping<string, GzTransaction>> monthlyTrx;

            // Step 1: Retrieve all Transactions by player activity
            if (monthsToProc == null || monthsToProc.Length == 0) {

                monthlyTrx = _db.GzTransactions.Where(t => t.CustomerId == customerId)
                    .OrderBy(t => t.YearMonthCtd)
                    .GroupBy(t => t.YearMonthCtd);
            }
            // Add filter condition: given months
            else {

                monthlyTrx = _db.GzTransactions.Where(t => t.CustomerId == customerId
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
        private void SaveDbCustomerMonthlyBalancesByTrx(int customerId, IEnumerable<IGrouping<string, GzTransaction>> customerMonthlyTrxs) {

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
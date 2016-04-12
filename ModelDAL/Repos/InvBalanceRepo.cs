﻿using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Data.Entity.Migrations;
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

        #region Fund Shares Selling

        /// <summary>
        /// 
        /// Sell completely a customer's portfolio
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="updatedDateTimeUtc">The database creation time-stamp.</param>
        /// <param name="yearCurrent">Optional year value for selling in the past</param>
        /// <param name="monthCurrent">Optional month value for selling in the past</param>
        public bool SaveDbSellCustomerPortfolio(int customerId, DateTime updatedDateTimeUtc, int yearCurrent = 0, int monthCurrent = 0) {

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

            // Trigger selling full portfolio by asking -$1 off of it.
            var portfolioFundsValuesThisMonth = _customerFundSharesRepo.GetMonthlyFundSharesAfterBuyingSelling(customerId, -1, yearCurrent, monthCurrent);

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
                            GzTransactionJournalTypeEnum.PortfolioLiquidation,
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
            out decimal invGainLoss) {
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
        public void SaveDbCustomerMonthBalanceByCashInv(int customerId, int yearCurrent, int monthCurrent, decimal cashToInvest) {

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
        /// Process All Monthly Customer Balances whether they have transactions or not.
        /// 
        /// </summary>
        public void SaveDbAllCustomerMonthlyBalances(string startYearMonthStr, string endYearMonthStr) {

            while (startYearMonthStr.BeforeEq(endYearMonthStr)) {

                var monthCustomers = _db.GzTransactions
                    .Where(t => string.Compare(startYearMonthStr, t.YearMonthCtd, StringComparison.Ordinal) >= 0)
                    .OrderBy(t => t.CustomerId)
                    .Select(t => t.CustomerId)
                    .Distinct()
                    .ToList();

                foreach (var customerId in monthCustomers) {

                    var customerMonthlyTrx =
                        _db.GzTransactions
                            .Where(t => t.CustomerId == customerId && t.YearMonthCtd == startYearMonthStr)
                            .GroupBy(t => t.YearMonthCtd)
                            .SingleOrDefault();

                    var yearCurrent = int.Parse(startYearMonthStr.Substring(0, 4));
                    var monthCurrent = int.Parse(startYearMonthStr.Substring(4, 2));
                    SaveDbCustomerMonthlyBalances(customerId, customerMonthlyTrx, yearCurrent, monthCurrent);
                }

                startYearMonthStr = DbExpressions.AddMonth(startYearMonthStr);
            }

        }

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
        /// <param name="customerMonthlyTrx"></param>
        private void SaveDbCustomerMonthlyBalancesByTrx(int customerId, IEnumerable<IGrouping<string, GzTransaction>> customerMonthlyTrx) {

            // Step 2: Loop monthly, calculate Balances based transaction and portfolios return
            foreach (var g in customerMonthlyTrx) {

                // Step 3: Calculate monthly cash balances before investment
                SaveDbCustomerMonthlyBalances(customerId, g);
            }
        }

        /// <summary>
        /// 
        /// Process the investment and cash balance for a single customer on a single month.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerMonthlyTrx"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        private void SaveDbCustomerMonthlyBalances(int customerId, IGrouping<string, GzTransaction> customerMonthlyTrx, int yearCurrent = 0, int monthCurrent = 0) {

            // Initialize year month if needed
            if (yearCurrent == 0 && customerMonthlyTrx != null) {

                yearCurrent = int.Parse(customerMonthlyTrx.Key.Substring(0, 4));

            }
            if (monthCurrent == 0 && customerMonthlyTrx != null) {

                monthCurrent = int.Parse(customerMonthlyTrx.Key.Substring(4, 2));
            }

            if (yearCurrent == 0 || monthCurrent == 0) {
                throw new Exception("Cannot have either year or month equal to 0 inside SaveDbCustomerMonthlyBalances()");
            }

            // Process

            var monthlyCashToInvest = GetMonthlyCashToInvest(customerMonthlyTrx);

            if (monthlyCashToInvest >= 0) {
                SaveDbCustomerMonthBalanceByCashInv(customerId, yearCurrent, monthCurrent, monthlyCashToInvest);
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

                SaveDbSellCustomerPortfolio(customerId, soldPortfolioTimestamp, yearCurrent, monthCurrent);
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
                    t => t.Type.Code == GzTransactionJournalTypeEnum.PortfolioLiquidation ? t.Amount : 0);

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
    }
}
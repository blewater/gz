using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Data.Entity.Migrations;
using gzDAL.Conf;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using gzDAL.Models;

namespace gzDAL.Repos {
    public class InvBalanceRepo : IInvBalanceRepo {
        private readonly ApplicationDbContext db;
        private readonly ICustFundShareRepo customerFundSharesRepo;

        public InvBalanceRepo(ApplicationDbContext db, ICustFundShareRepo customerFundSharesRepo) {
            this.db = db;
            this.customerFundSharesRepo = customerFundSharesRepo;
        }

#region Selling

        /// <summary>
        /// Sell fully a customer's portfolio
        /// </summary>
        /// <param name="custId"></param>
        /// <param name="yearCurrent">Optional year value for selling in the past</param>
        /// <param name="monthCurrent">Optional month value for selling in the past</param>
        public void SaveDBSellCustomerPortfolio(int custId, int yearCurrent = 0, int monthCurrent = 0) {

            if (yearCurrent == 0) {
                yearCurrent = DateTime.UtcNow.Year;
            }
            if (monthCurrent == 0) {
                monthCurrent = DateTime.UtcNow.Month;
            }

            if (new DateTime(yearCurrent, monthCurrent, 1) > DateTime.UtcNow) {

                throw new Exception("Cannot sell the customer's (id: " + custId + ") portfolio in the future.");
            }

            // Trigger selling full portfolio by asking -$1 off of it.
            var portfolioFundsValuesThisMonth = customerFundSharesRepo.GetCalcCustMonthlyFundShares(custId, -1, yearCurrent, monthCurrent);
            var prevMonSharesPricedNow = portfolioFundsValuesThisMonth.Sum(f => f.Value.SharesValue);
            var prevMonthBalAmount = GetPrevMonthBalCashValue(custId, yearCurrent, monthCurrent);
            var invGainLoss = DbExpressions.RoundCustomerBalanceAmount(prevMonSharesPricedNow - prevMonthBalAmount);
            var monthlyBalance = DbExpressions.RoundCustomerBalanceAmount(prevMonSharesPricedNow);

            SaveDBCustomerMonthBalanceBySelling(portfolioFundsValuesThisMonth, custId, yearCurrent, monthCurrent, monthlyBalance, invGainLoss);
        }

        private void SaveDBCustomerMonthBalanceBySelling(
            Dictionary<int, CustFundShareRepo.PortfolioFundDTO> portfolioFunds,
            int customerId,
            int year,
            int month,
            decimal totSharesValue,
            decimal invGainLoss) {

            ConnRetryConf.SuspendExecutionStrategy = true;
            var executionStrategy = new SqlAzureExecutionStrategy(2, TimeSpan.FromSeconds(10));
            executionStrategy
                    .Execute(() => {
                        using (var dbContextTransaction = db.Database.BeginTransaction()) {
                            customerFundSharesRepo.SaveDBCustomerPurchasedFundShares(customerId, portfolioFunds, year, month, DateTime.UtcNow);

                            db.InvBalances.AddOrUpdate(i => new { i.CustomerId, i.YearMonth },
                                                       new InvBalance {
                                                           YearMonth = DbExpressions.GetStrYearMonth(year, month),
                                                           CustomerId = customerId,
                                                           Balance = totSharesValue,
                                                           InvGainLoss = invGainLoss,
                                                           CashInvestment = -totSharesValue,
                                                           UpdatedOnUTC = DateTime.UtcNow
                                                       });
                            db.SaveChanges();
                            dbContextTransaction.Commit();
                        }
                    });

            ConnRetryConf.SuspendExecutionStrategy = false;
        }

#endregion Selling


        /// <summary>
        /// Calculate financial information for a customer on a given month
        /// </summary>
        /// <param name="custId"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <param name="cashToInvest">The positive cash to buy shares</param>
        /// <param name="monthlyBalance">Out -> Monthly Cash Value useful in summary page</param>
        /// <param name="invGainLoss">Out -> Monthly Gain or Loss in cash value Used in summary page</param>
        /// <returns></returns>
        public Dictionary<int, CustFundShareRepo.PortfolioFundDTO>
            GetCalcMonthlyBalancesForCustomer(
                int custId,
                int yearCurrent,
                int monthCurrent,
                decimal cashToInvest,
                out decimal monthlyBalance,
                out decimal invGainLoss) {

            // Step 1: Buy if amount is positive otherwise sell shares or just reprice for the month the existing shares
            var portfolioFundsValuesThisMonth = customerFundSharesRepo.GetCalcCustMonthlyFundShares(
                custId,
                cashToInvest,
                yearCurrent,
                monthCurrent);

            var totSharesValue = portfolioFundsValuesThisMonth.Sum(f => f.Value.SharesValue);
            var newShareVal = portfolioFundsValuesThisMonth.Sum(f => f.Value.NewSharesValue ?? 0);
            var prevMonSharesPricedNow = totSharesValue - newShareVal;

            // Step 2: Get the previous month balance
            var prevMonthBalAmount = GetPrevMonthBalCashValue(custId, yearCurrent, monthCurrent);

            invGainLoss = DbExpressions.RoundCustomerBalanceAmount(prevMonSharesPricedNow - prevMonthBalAmount);
            monthlyBalance = DbExpressions.RoundCustomerBalanceAmount(totSharesValue);

            return portfolioFundsValuesThisMonth;
        }

        /// <summary>
        /// Get the previous month's cash value
        /// </summary>
        /// <param name="custId"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <returns></returns>
        private decimal GetPrevMonthBalCashValue(int custId, int yearCurrent, int monthCurrent) {

            // Temp expressions
            DateTime prevYearMonth = new DateTime(yearCurrent, monthCurrent, 1).AddMonths(-1);
            var prevYearMonthStr = DbExpressions.GetStrYearMonth(prevYearMonth.Year, prevYearMonth.Month);

            // Get the previous month's value
            var prevMonthBalAmount = db.InvBalances
                .Where(b => b.CustomerId == custId &&
                            string.Compare(b.YearMonth, prevYearMonthStr, StringComparison.Ordinal) <= 0)
                .OrderByDescending(b => b.YearMonth)
                .Select(b => b.Balance)
                .FirstOrDefault();

            return prevMonthBalAmount;
        }

        /// <summary>
        /// Save in the database account the monthly customer balance
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="cashToInvest">Positive cash amount to invest</param>
        public void SaveDBCustomerMonthBalanceByCashInv(int customerId, int year, int month, decimal cashToInvest) {

            decimal monthlyBalance, invGainLoss;
            var portfolioFunds = GetCalcMonthlyBalancesForCustomer(customerId, year, month, cashToInvest, out monthlyBalance,
                out invGainLoss);

            ConnRetryConf.SuspendExecutionStrategy = true;
            var executionStrategy = new SqlAzureExecutionStrategy(2, TimeSpan.FromSeconds(10));
            executionStrategy
                    .Execute(() =>
                             {
                                 using (var dbContextTransaction = db.Database.BeginTransaction())
                                 {
                                     customerFundSharesRepo.SaveDBCustomerPurchasedFundShares(customerId, portfolioFunds, year, month, DateTime.UtcNow);

                                     db.InvBalances.AddOrUpdate(i => new {i.CustomerId, i.YearMonth},
                                                                new InvBalance
                                                                {
                                                                        YearMonth = DbExpressions.GetStrYearMonth(year, month),
                                                                        CustomerId = customerId,
                                                                        Balance = monthlyBalance,
                                                                        InvGainLoss = invGainLoss,
                                                                        CashInvestment = cashToInvest,
                                                                        UpdatedOnUTC = DateTime.UtcNow
                                                                });
                                     db.SaveChanges();
                                     dbContextTransaction.Commit();
                                 }
                             });

            ConnRetryConf.SuspendExecutionStrategy = false;
        }

        /// <summary>
        /// Save to Database the calculated customer monthly investment balances
        ///     for the given months
        /// -- Or --
        ///     by all monthly transaction activity of the customer.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="monthsToProc">Array of YYYYMM values i.e. [201601, 201502]. If null then select all months with this customers transactional activity.</param>
        public void SaveDBCustomerMonthlyBalancesByTrx(int customerId, string[] monthsToProc = null) {

            List<IGrouping<string, GzTransaction>> monthlyTrx;

            // Step 1: Retrieve all Transactions by player activity
            if (monthsToProc == null || monthsToProc.Length == 0) {

                monthlyTrx = db.GzTransactions.Where(t => t.CustomerId == customerId)
                    .OrderBy(t => t.YearMonthCtd)
                    .GroupBy(t => t.YearMonthCtd)
                    .ToList();
            }
            // Add filter condition: given months
            else {

                monthlyTrx = db.GzTransactions.Where(t => t.CustomerId == customerId && monthsToProc.Contains(t.YearMonthCtd))
                    .OrderBy(t => t.YearMonthCtd)
                    .GroupBy(t => t.YearMonthCtd)
                    .ToList();
            }

            SaveDBCustomerMonthlyBalancesByTrx(customerId, monthlyTrx);
        }

        /// <summary>
        /// Called by public SaveDBCustomerMonthlyBalancesByTrx after selection of Monthly Transactions:
        /// To save to Database the calculated customer monthly investment balances
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="monthlyTrx"></param>
        private void SaveDBCustomerMonthlyBalancesByTrx(int customerId, List<IGrouping<string, GzTransaction>> monthlyTrx) {

            // Step 2: Loop monthly, calculate Balances based transaction and portfolios return
            foreach (var g in monthlyTrx) {

                int curYear = int.Parse(g.Key.Substring(0, 4));
                int curMonth = int.Parse(g.Key.Substring(4, 2));


                // Step 3: Calculate monthly cash balances before investment
                var monthlyCashToInvest = GetMonthlyCashToInvest(g);

                // If not positive, we are off for some reason.
                System.Diagnostics.Trace.Assert(monthlyCashToInvest >= 0,
                    "Cash to invest should be positive or 0. Selling stock is supported only for the whole portfolio.");

                if (monthlyCashToInvest >= 0) {
                    SaveDBCustomerMonthBalanceByCashInv(customerId, curYear, curMonth, monthlyCashToInvest);
                }
            }
        }

        /// <summary>
        /// Calculate monthly aggregated cash amount to invest
        /// </summary>
        /// <param name="monthlyTrxGrouping"></param>
        /// <returns></returns>
        private decimal GetMonthlyCashToInvest(IGrouping<string, GzTransaction> monthlyTrxGrouping) {

            var monthlyPlayingLosses = monthlyTrxGrouping.Sum(t => t.Type.Code == TransferTypeEnum.CreditedPlayingLoss ? t.Amount : 0);
            var monthlyWithdrawnAmounts = monthlyTrxGrouping.Sum(t => t.Type.Code == TransferTypeEnum.InvWithdrawal ? t.Amount : 0);
            var monthlyTransfersToGaming =
                monthlyTrxGrouping.Sum(t => t.Type.Code == TransferTypeEnum.TransferToGaming ? t.Amount : 0);
            var monthlyFees =
                monthlyTrxGrouping.Sum(
                    t =>
                        t.Type.Code == TransferTypeEnum.GzFees || t.Type.Code == TransferTypeEnum.FundFee
                            ? t.Amount
                            : 0);


            // Net amount to invest
            var monthlyCashToInvest = monthlyPlayingLosses - monthlyWithdrawnAmounts - monthlyTransfersToGaming -
                                      monthlyFees;
            return monthlyCashToInvest;
        }

        /// <summary>
        /// Multiple Customers Version:
        /// Save to Database the calculated customer monthly investment balances
        ///     otherwise calculate monthly investment balances for all transaction activity months of the players.
        /// </summary>
        /// <param name="customerIds"></param>
        /// <param name="yearMonthsToProc"></param>
        public void SaveDBCustomersMonthlyBalancesByTrx(int[] customerIds, string[] yearMonthsToProc) {

            if (customerIds == null) {
                return;
            }

            foreach (var customerId in customerIds) {
                SaveDBCustomerMonthlyBalancesByTrx(customerId, yearMonthsToProc);
            }
        }
    }
}
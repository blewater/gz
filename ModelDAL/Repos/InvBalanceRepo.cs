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

        /// <summary>
        /// Calculate numeric balance metrics for a customer on a given month
        /// </summary>
        /// <param name="custId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="cashToInvest">The positive cash to buy shares</param>
        /// <param name="monthlyBalance">Useful in summary page</param>
        /// <param name="invGainLoss">Used in summary page</param>
        /// <returns></returns>
        public Dictionary<int, CustFundShareRepo.PortfolioFundDTO> GetCalcMonthlyBalancesForCustomer(int custId,
            int year, int month, decimal cashToInvest, out decimal monthlyBalance, out decimal invGainLoss) {

            // Previous Month DateTime                
            DateTime prevYearMonth = new DateTime(year, month, 1).AddMonths(-1);

            // Step 1: Buy if amount is positive otherwise sell shares or just reprice for the month the existing shares
            var portfolioFundsValuesThisMonth = customerFundSharesRepo.GetCalcCustMonthlyFundShares(custId, cashToInvest,
                year, month);
            var totSharesValue = portfolioFundsValuesThisMonth.Sum(f => f.Value.SharesValue);
            var newShareVal = portfolioFundsValuesThisMonth.Sum(f => f.Value.NewSharesValue ?? 0);
            var prevMonSharesPricedNow = totSharesValue - newShareVal;

            // Step 2: Get the previous month balance
            var prevYearMonthStr = DbExpressions.GetStrYearMonth(prevYearMonth.Year, prevYearMonth.Month);
            var prevMonthBalAmount = db.InvBalances
                .Where(b => b.CustomerId == custId &&
                            string.Compare(b.YearMonth, prevYearMonthStr) <= 0)
                .OrderByDescending(b => b.YearMonth)
                .Select(b => b.Balance)
                .FirstOrDefault();

            invGainLoss = DbExpressions.RoundCustomerBalanceAmount(prevMonSharesPricedNow - prevMonthBalAmount);
            monthlyBalance = DbExpressions.RoundCustomerBalanceAmount(totSharesValue);

            return portfolioFundsValuesThisMonth;

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
                                     customerFundSharesRepo.SaveDbCustPurchasedFundShares(customerId, portfolioFunds, year, month, DateTime.UtcNow);

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
        /// Calculate customer monthly investment balances for the given months if monthsToProc is not null
        ///     otherwise calculate monthly investment balances for all transaction activity months of a player.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="monthsToProc">Array of YYYYMM values i.e. [201601, 201502]</param>
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

            SaveDBCustomerMonthBalance(customerId, monthlyTrx);
        }

        private void SaveDBCustomerMonthBalance(int customerId, List<IGrouping<string, GzTransaction>> monthlyTrx) {
            
            // Step 2: Loop monthly, calculate Balances based transaction and portfolios return
            foreach (var g in monthlyTrx) {

                int curYear = int.Parse(g.Key.Substring(0, 4));
                int curMonth = int.Parse(g.Key.Substring(4, 2));


                // Step 3: Calculate monthly cash balances before investment
                var monthlyPlayingLosses = g.Sum(t => t.Type.Code == TransferTypeEnum.CreditedPlayingLoss ? t.Amount : 0);
                var monthlyWithdrawnAmounts = g.Sum(t => t.Type.Code == TransferTypeEnum.InvWithdrawal ? t.Amount : 0);
                var monthlyTransfersToGaming =
                    g.Sum(t => t.Type.Code == TransferTypeEnum.TransferToGaming ? t.Amount : 0);
                var monthlyFees =
                    g.Sum(
                        t =>
                            t.Type.Code == TransferTypeEnum.GzFees || t.Type.Code == TransferTypeEnum.FundFee
                                ? t.Amount
                                : 0);


                // Net amount to invest
                var monthlyCashToInvest = monthlyPlayingLosses - monthlyWithdrawnAmounts - monthlyTransfersToGaming -
                                          monthlyFees;

                System.Diagnostics.Trace.Assert(monthlyCashToInvest >= 0,
                    "Cash to invest should be positive or 0. Selling stock is supported only for the whole portfolio.");

                if (monthlyCashToInvest >= 0) {
                    SaveDBCustomerMonthBalanceByCashInv(customerId, curYear, curMonth, monthlyCashToInvest);
                }
            }
        }

        /// <summary>
        /// Multiple Customers Version:
        /// Calculate customer monthly investment balances for the given months if monthsToProc is not null
        ///     otherwise calculate monthly investment balances for all transaction activity months of a player.
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
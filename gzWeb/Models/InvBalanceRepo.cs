using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using gzWeb.Utl;

namespace gzWeb.Models {
    public class InvBalanceRepo {

        public List<CustFundShareRepo.PortfolioFundDTO> GetCalcMonthlyBalancesForCustomer(int custId, int year, int month, out decimal monthlyBalance, out decimal invGainLoss, out decimal cashToInvest) {

            using (var db = new ApplicationDbContext()) {

                // Step 1: Retrieve all the months Transactions
                var monthlyTrx = db.GzTransactions.Where(t => t.CustomerId == custId
                    && t.YearMonthCtd == Expressions.GetStrYearMonth(year, month));

                var withdrawals = monthlyTrx
                    .Where(t => t.Type.Code == TransferTypeEnum.Withdrawal || t.Type.Code == TransferTypeEnum.TransferToGaming)
                    .Select(t => t.Amount)
                    .Sum();
                var newInvTransfers = monthlyTrx
                    .Where(t => t.Type.Code == TransferTypeEnum.CreditedPlayingLoss)
                    .Select(t => t.Amount)
                    .Sum();
                cashToInvest = newInvTransfers - withdrawals;

                // Previous Month DateTime                
                DateTime prevYearMonth = new DateTime(year, month, 1).AddMonths(-1);

                // Step 2: Buy if amount is positive otherwise sell shares or just reprice for the month the existing shares
                var customerFundSharesRepo = new CustFundShareRepo();
                var portfolioFundsValuesThisMonth = customerFundSharesRepo.GetCalcCustMonthlyFundShares(custId, cashToInvest, year, month);
                var prevMonSharesPricedNow = portfolioFundsValuesThisMonth.Sum(f => f.SharesValue - f.NewSharesValue ?? 0);

                // Step 3: Get the previous month balance
                var prevMonthBalAmount = db.InvBalances
                    .Where(b => b.CustomerId == custId &&
                    string.Compare(b.YearMonth, Expressions.GetStrYearMonth(prevYearMonth.Year, prevYearMonth.Month)) < 0)
                    .OrderByDescending(b => b.YearMonth)
                    .Select(b => b.Balance)
                    .Take(1)
                    .Single();

                invGainLoss = prevMonSharesPricedNow - prevMonthBalAmount;
                monthlyBalance = portfolioFundsValuesThisMonth.Sum(f => f.SharesValue);

                return portfolioFundsValuesThisMonth;
            }
        }

        /// <summary>
        /// Save in the database account the monthly customer balance
        /// </summary>
        /// <param name="custId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        public void SaveMonthlyBalanceForCustomer(int custId, int year, int month) {

            decimal monthlyBalance, invGainLoss, cashInvestment;
            var portfolioFunds = GetCalcMonthlyBalancesForCustomer(custId, year, month, out monthlyBalance, out invGainLoss, out cashInvestment);

            var customerFundSharesRepo = new CustFundShareRepo();
            using (var db = new ApplicationDbContext()) {

                using (var dbContextTransaction = db.Database.BeginTransaction()) {

                    try {
                        customerFundSharesRepo.SaveDbCustPurchasedFundShares(db, custId, portfolioFunds, year, month, DateTime.UtcNow);

                        db.InvBalances.AddOrUpdate(
                            i => new { i.CustomerId, i.YearMonth },
                            new InvBalance {
                                YearMonth = Expressions.GetStrYearMonth(year, month),
                                CustomerId = custId,
                                Balance = monthlyBalance,
                                InvGainLoss = invGainLoss,
                                CashInvestment = cashInvestment,
                                UpdatedOnUTC = DateTime.UtcNow
                            }
                            );

                        dbContextTransaction.Commit();
                    } catch (Exception e) {
                        dbContextTransaction.Rollback();
                        var msg = e.Message;
                    }
                }
            }

        }

        /// <summary>
        /// Calculate monthly investment balances for all months of a player.
        /// </summary>
        /// <param name="custId"></param>
        public void SaveCustTrxsBalances(int custId) {

            using (var db = new ApplicationDbContext()) {

                // Step 1: Retrieve all Transactions by YearMonth
                var monthlyTrx = db.GzTransactions.Where(t => t.CustomerId == custId)
                .OrderBy(t => t.YearMonthCtd)
                .GroupBy(t => t.YearMonthCtd)
                .ToList();


                // Step 2: Loop monthly, calculate Balances based transaction and portfolios return
                foreach (var g in monthlyTrx) {

                    int curYear = int.Parse(g.Key.Substring(0, 3));
                    int curMonth = int.Parse(g.Key.Substring(4, 2));


                    // Step 3: Calculate monthly cash balances before investment
                    var monthlyPlayingLosses = g.Sum(t => t.Type.Code == TransferTypeEnum.CreditedPlayingLoss ? t.Amount : 0);
                    var monthlyWithdrawnAmounts = g.Sum(t => t.Type.Code == TransferTypeEnum.Withdrawal || t.Type.Code == TransferTypeEnum.TransferToGaming ? t.Amount : 0);
                    var monthlyCashToInvest = monthlyPlayingLosses - monthlyWithdrawnAmounts;

                    // TODO Remove invGainLoss type of transaction

                    //var monthlyCashBalance = prevMonBal + monthlyCashToInvest;
                    SaveMonthlyBalanceForCustomer(custId, curYear, curMonth);
                }
            }
        }
    }
}
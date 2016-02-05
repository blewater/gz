using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;

namespace gzWeb.Models {
    public class InvBalanceRepo {

        /// <summary>
        /// Calculate monthly investment balances for all months of a player.
        /// </summary>
        /// <param name="custId"></param>
        public void CalcMonthlyBalancesForCustomer(int custId) {

            using (var db = new ApplicationDbContext()) {

                // Step 1: Retrieve all Transactions by YearMonth
                var monthlyTrx = db.GzTransactions.Where(t => t.CustomerId == custId)
                .OrderBy(t => t.YearMonthCtd)
                .GroupBy(t => t.YearMonthCtd)
                .ToList();


                // Step 2: Calculate Balances based transaction and portfolios return
                var prevMonBal = new decimal(0.00);
                foreach (var g in monthlyTrx) {

                    int year = int.Parse(g.Key.Substring(0, 3));
                    int month = int.Parse(g.Key.Substring(4, 2));

                    var InvAmount = g.Sum(t => t.Type.Code == TransferTypeEnum.CreditedPlayingLoss ? t.Amount : 0);

                    // Informational field only. Store the monthly amount and use it for the calculation of the next month.
                    var invGainLoss = g.Sum(t => t.Type.Code == TransferTypeEnum.InvestmentRet ? t.Amount : 0);

                    //Step 3 Calc & save Portfolio Value
                    
                    // Select max month for the current transaction activity
                    var lastMonthPort = db.CustPortfolios
                        .Where(p => p.CustomerId == custId && string.Compare(p.YearMonth, g.Key) <= 0)
                        .Select(p => p.YearMonth)
                        .Max();

                    if (lastMonthPort != null) {
                        var customerFundSharesRepo = new CustFundShareRepo();
                        var portfolioFundsValues = customerFundSharesRepo.CalcCustMonthlyFundShares(custId, InvAmount, year, month);

                        customerFundSharesRepo.AddCustPurchasedFundShares(custId, portfolioFundsValues, year, month, DateTime.UtcNow);
                    }

                    var WithdrawnAmounts = g.Sum(t => t.Type.Code == TransferTypeEnum.Withdrawal || t.Type.Code == TransferTypeEnum.TransferToGaming ? t.Amount : 0);

                    var gBalance = prevMonBal + InvAmount + WithdrawnAmounts;

                    db.InvBalances.AddOrUpdate(
                        b => new { b.CustomerId, b.YearMonth },
                        new InvBalance {
                            Balance = gBalance,
                            CustomerId = custId,
                            YearMonth = g.Key,
                            InvGainLoss = invGainLoss,
                            UpdatedOnUTC = DateTime.UtcNow
                        }
                    );
                    //This is the previous month balance
                    prevMonBal = gBalance;
                }
            }
        }
    }
}
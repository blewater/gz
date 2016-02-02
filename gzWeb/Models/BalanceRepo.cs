using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;

namespace gzWeb.Models {
    public class BalanceRepo {

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

                    // Select max month for the current transaction activity
                    var lastMonthPort = db.CustPortfolios
                        .Where(p => p.CustomerId == custId && string.Compare(p.YearMonth, g.Key) <= 0)
                        .Select(p => p.YearMonth)
                        .Max();

                    decimal investmentRet = 0;

                    if (lastMonthPort != null) {

                        //Take last portfolio configuration (multiple rows or null?) at the current Month
                        var portWeights = db.CustPortfolios
                            .Where(p => p.CustomerId == custId && string.Compare(p.YearMonth, lastMonthPort) <= 0)
                            .Select(p => new { p.PortfolioId, p.Portfolio.RiskTolerance, p.Weight});

                        //var fundWeights = db.PortFunds.
                    }



                    //portWeights.
                    var InvAmount = g.Sum(t => t.Type.Code == TransferTypeEnum.CreditedPlayingLoss ? t.Amount : 0);
                    var InvGain = g.Sum(t => t.Type.Code == TransferTypeEnum.InvestmentRet ? t.Amount : 0);
                    var WithdrawnAmounts = g.Sum(t => t.Type.Code == TransferTypeEnum.Withdrawal || t.Type.Code == TransferTypeEnum.TransferToGaming ? t.Amount : 0);

                    var gBalance = prevMonBal + InvAmount + InvGain - WithdrawnAmounts;

                    db.InvBalances.AddOrUpdate(
                        b => new { b.CustomerId, b.YearMonthCtd },
                        new InvBalance {
                            Balance = gBalance,
                            CustomerId = custId,
                            YearMonthCtd = g.Key,
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
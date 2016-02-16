﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using gzWeb.Utl;

namespace gzWeb.Models {
    public class InvBalanceRepo {

        public List<CustFundShareRepo.PortfolioFundDTO> GetCalcMonthlyBalancesForCustomer(int custId, int year, int month, decimal cashToInvest, out decimal monthlyBalance, out decimal invGainLoss) {

            using (var db = new ApplicationDbContext()) {

                // Previous Month DateTime                
                DateTime prevYearMonth = new DateTime(year, month, 1).AddMonths(-1);

                // Step 1: Buy if amount is positive otherwise sell shares or just reprice for the month the existing shares
                var customerFundSharesRepo = new CustFundShareRepo();
                var portfolioFundsValuesThisMonth = customerFundSharesRepo.GetCalcCustMonthlyFundShares(custId, cashToInvest, year, month);
                var totSharesValue = portfolioFundsValuesThisMonth.Sum(f => f.SharesValue);
                var newShareVal = portfolioFundsValuesThisMonth.Sum(f => f.NewSharesValue ?? 0);
                var prevMonSharesPricedNow = totSharesValue - newShareVal;

                // Step 2: Get the previous month balance
                var prevYearMonthStr = Expressions.GetStrYearMonth(prevYearMonth.Year, prevYearMonth.Month);
                var prevMonthBalAmount = db.InvBalances
                    .Where(b => b.CustomerId == custId &&
                    string.Compare(b.YearMonth, prevYearMonthStr) <= 0)
                    .OrderByDescending(b => b.YearMonth)
                    .Select(b => b.Balance)
                    .FirstOrDefault();

                invGainLoss = prevMonSharesPricedNow - prevMonthBalAmount;
                monthlyBalance = totSharesValue;

                return portfolioFundsValuesThisMonth;
            }
        }

        /// <summary>
        /// Save in the database account the monthly customer balance
        /// </summary>
        /// <param name="custId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        public void SaveMonthlyBalanceForCustomer(int custId, int year, int month, decimal cashToInvest) {

            decimal monthlyBalance, invGainLoss, cashInvestment;
            var portfolioFunds = GetCalcMonthlyBalancesForCustomer(custId, year, month, cashToInvest, out monthlyBalance, out invGainLoss);

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
                                CashInvestment = cashToInvest,
                                UpdatedOnUTC = DateTime.UtcNow
                            }
                            );
                        db.SaveChanges();
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

                    int curYear = int.Parse(g.Key.Substring(0, 4));
                    int curMonth = int.Parse(g.Key.Substring(4, 2));


                    // Step 3: Calculate monthly cash balances before investment
                    var monthlyPlayingLosses = g.Sum(t => t.Type.Code == TransferTypeEnum.CreditedPlayingLoss ? t.Amount : 0);
                    var monthlyWithdrawnAmounts = g.Sum(t => t.Type.Code == TransferTypeEnum.Withdrawal || t.Type.Code == TransferTypeEnum.TransferToGaming ? t.Amount : 0);
                    var monthlyCashToInvest = monthlyPlayingLosses - monthlyWithdrawnAmounts;

                    // TODO Remove invGainLoss type of transaction

                    //var monthlyCashBalance = prevMonBal + monthlyCashToInvest;
                    SaveMonthlyBalanceForCustomer(custId, curYear, curMonth, monthlyCashToInvest);
                }
            }
        }
    }
}
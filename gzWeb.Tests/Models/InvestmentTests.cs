using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using gzDAL.Models;
using gzDAL.Repos;
using gzDAL.ModelUtil;
using GzBalances;
using System.Configuration;
using GzDb;

namespace gzWeb.Tests.Models {
    [TestFixture]
    public class InvestmentTests {

        [OneTimeSetUp]
        public void Setup() {
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        // Excel numbers for asserts
        //TotalCashInv CashInv Balance Share Price Total Shares VintageShares   InvGain
        //    1000	    1000	1000	    1	    1000	    1000	        0
        //    2000	    1000	2100	    1.1	    1909.090909	909.0909091	    100
        //    3000	    1000	3290.909091	1.2	    2742.424242	833.3333333	    290.9090909
        //    4000	    1000	4565.151515	1.3	    3511.655012	769.2307692	    565.1515152
        //    5000	    1000	5916.317016	1.4	    4225.940726	714.2857143	    916.3170163
        //    6000	    1000	7338.911089	1.5	    4892.607393	666.6666667	    1338.911089
        //    7000	    1000	8828.171828	1.6	    5517.607393	625	            1828.171828
        [Test]
        public void SetDb6MonthInvBalancesWithFreshTrx() {

            string[] customerEmails = new string[] {
                "salem8@gmail.com",
                "joe@mymail.com",
                "testuser@gz.com",
                "info@nessos.gr",
                "info1@nessos.gr",
                "testuser@gz.com",
                "6month@allocation.com"
            };

            var devDbConnString = ConfigurationManager.ConnectionStrings["gzDevDb"].ConnectionString;
            using (ApplicationDbContext db = new ApplicationDbContext(null))
            using (var dbSimpleCtx = DbUtil.getOpenDb(devDbConnString)) {

                ClearTrxHistory(customerEmails, db);

                var now = DateTime.UtcNow;
                var startYearMonthStr = now.AddMonths(-6).ToStringYearMonth();
                var endYearMonthStr = now.ToStringYearMonth();

                int monthsCnt = 0;
                var usersFound = new List<int>();
                // Loop through all the months activity
                while (startYearMonthStr.BeforeEq(endYearMonthStr)) {

                    ProcessInvBalances(db, customerEmails, usersFound, monthsCnt, startYearMonthStr, dbSimpleCtx);

                    if (string.Compare(startYearMonthStr, endYearMonthStr, StringComparison.Ordinal) == 0) {

                        AssertBalanceNumbers(usersFound, db, endYearMonthStr);
                    }

                    monthsCnt++;
                    startYearMonthStr = DbExpressions.AddMonth(startYearMonthStr);
                }
            }
        }

        private static void AssertBalanceNumbers(List<int> usersFound, ApplicationDbContext db, string endYearMonthStr) {

            var expectedBal = DbExpressions.RoundCustomerBalanceAmount(8828.171828m);
            var expectedLastVintageShares = DbExpressions.RoundCustomerBalanceAmount(625);
            var expectedInvGain = DbExpressions.RoundCustomerBalanceAmount(1828.171828m);
            foreach (var userId in usersFound) {
                var invB = db.InvBalances
                    .Single(i => i.CustomerId == userId &&
                            string.Compare(i.YearMonth, endYearMonthStr, StringComparison.Ordinal) == 0);
                var bal = invB.Balance;
                var totalShares = invB.LowRiskShares + invB.MediumRiskShares + invB.HighRiskShares;
                var gain = invB.InvGainLoss;
                var totalInvestments = invB.TotalCashInvestments;
                Assert.AreEqual(expectedBal, DbExpressions.RoundCustomerBalanceAmount(bal));
                Assert.AreEqual(expectedLastVintageShares, DbExpressions.RoundCustomerBalanceAmount(totalShares));
                Assert.AreEqual(DbExpressions.RoundCustomerBalanceAmount(gain), DbExpressions.RoundCustomerBalanceAmount(bal-totalInvestments));
                Assert.AreEqual(expectedInvGain, DbExpressions.RoundCustomerBalanceAmount(gain));
            }
        }

        private static void ProcessInvBalances(ApplicationDbContext db, string[] customerEmails, List<int> usersFound, int monthsCnt,
            string startYearMonthStr, DbUtil.DbSchema.ServiceTypes.SimpleDataContextTypes.GzDbDev dbSimpleCtx) {

            var cpRepo = new CustPortfolioRepo(db);
            var gzTrx = new GzTransactionRepo(db);

            foreach (var customerEmail in customerEmails) {

                int custId = db.Users
                    .Where(u => u.Email == customerEmail)
                    .Select(u => u.Id)
                    .SingleOrDefault();

                if (custId != 0) {

                    usersFound.Add(custId);

                    SetDbMonthlyPortfolioLossesForTestPlayers(db, custId, monthsCnt, cpRepo, startYearMonthStr,
                        gzTrx);
                }
            }

            SetDbClearMonth(startYearMonthStr, monthsCnt, dbSimpleCtx);
        }

        private static void ClearTrxHistory(string[] customerEmails, ApplicationDbContext db) {

            foreach (var customerEmail in customerEmails) {

                int custId = db.Users
                    .Where(u => u.Email == customerEmail)
                    .Select(u => u.Id)
                    .SingleOrDefault();

                if (custId != 0) {

                    // Clear all customer activity
                    db.Database.ExecuteSqlCommand("Delete GzTrxs Where CustomerId = " + custId);
                    db.Database.ExecuteSqlCommand("Delete VintageShares Where UserId = " + custId);
                    db.Database.ExecuteSqlCommand("Update InvBalances Set Sold = 0 Where CustomerId = " + custId);

                }
            }
        }

        private static
            void SetDbMonthlyPortfolioLossesForTestPlayers(ApplicationDbContext db, int custId, int monthsCnt,
                CustPortfolioRepo cpRepo, string startYearMonthStr, GzTransactionRepo gzTrx) {

            SetDbMonthlyPortfolioSelection(monthsCnt, cpRepo, custId, startYearMonthStr);

            SetDbMonthlyPlayerLossTrx(startYearMonthStr, gzTrx, custId);
        }

        private static void SetDbClearMonth(string startYearMonthStr, int monthCnt, DbUtil.DbSchema.ServiceTypes.SimpleDataContextTypes.GzDbDev dbSimpleCtx) {

            var stockPrice = 1 + (monthCnt / 10.0);

            var lowPortfolioPrice = new PortfolioTypes.PortfolioSharePrice((int)RiskToleranceEnum.Low, stockPrice,
                DbExpressions.GetDtYearMonthStrToEndOfMonth(startYearMonthStr));
            var mediumPortfolioPrice = new PortfolioTypes.PortfolioSharePrice((int)RiskToleranceEnum.Medium, stockPrice,
                DbExpressions.GetDtYearMonthStrToEndOfMonth(startYearMonthStr));
            var highPortfolioPrice = new PortfolioTypes.PortfolioSharePrice((int)RiskToleranceEnum.High, stockPrice,
                DbExpressions.GetDtYearMonthStrToEndOfMonth(startYearMonthStr));
            var portfolioPricesEoM = new PortfolioTypes.PortfoliosPrices(lowPortfolioPrice, mediumPortfolioPrice,
                highPortfolioPrice);
            var portfoliosPriceDict = new Dictionary<string, PortfolioTypes.PortfoliosPrices>() {
                    {
                        DbExpressions.GetDtYearMonthStrToEndOfMonth(startYearMonthStr)
                            .ToStringYearMonthDay(),
                        portfolioPricesEoM
                    }
                };

            var portfoliosPriceMap = PortfolioTypes.toMap(portfoliosPriceDict);

            UserTrx.processGzTrx(dbSimpleCtx, startYearMonthStr, portfoliosPriceMap);
        }

        private static void SetDbMonthlyPlayerLossTrx(string trxYearMonthStr, GzTransactionRepo gzTrx, int custId) {

            var createdOnUtc =
                new DateTime(
                    DbExpressions.GetYear(trxYearMonthStr),
                    DbExpressions.GetMonth(trxYearMonthStr),
                    15,
                    19,
                    12,
                    59,
                    333,
                    DateTimeKind.Utc);

            gzTrx.SaveDbPlayingLoss(
                custId,
                2000,
                createdOnUtc,
                3000, 3000, 1000, -2000, 3000);
        }

        private static void SetDbMonthlyPortfolioSelection(int monthsCnt, CustPortfolioRepo cpRepo, int custId, string startYearMonthStr) {

            var monthPortfolioRisk =
                monthsCnt % 3 == 0
                    ? RiskToleranceEnum.Low
                    : monthsCnt % 3 == 1
                        ? RiskToleranceEnum.Medium
                        : RiskToleranceEnum.High;

            // Create loss transaction for the month
            cpRepo.SaveDbCustMonthsPortfolioMix(
                custId,
                monthPortfolioRisk,
                DbExpressions.GetYear(startYearMonthStr),
                DbExpressions.GetMonth(startYearMonthStr),
                DateTime.UtcNow);
        }
    }

}

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

        [Test]
        public void SetDb6MonthInvBalancesWithFreshTrx() {

            string[] customerEmails = new string[] {
                "salem8@gmail.com",
                "info@nessos.gr",
                "info1@nessos.gr",
                "testuser@gz.com",
                "6month@allocation.com"
            };

            var devDbConnString = ConfigurationManager.ConnectionStrings["gzDevDb"].ConnectionString;
            using (ApplicationDbContext db = new ApplicationDbContext(null))
            using (var dbSimpleCtx = DbUtil.getOpenDb(devDbConnString)) {

                var now = DateTime.UtcNow;
                var startYearMonthStr = now.AddMonths(-6).ToStringYearMonth();
                var endYearMonthStr = now.ToStringYearMonth();

                int monthsCnt = 0;
                // Loop through all the months activity
                while (startYearMonthStr.BeforeEq(endYearMonthStr)) {

                    monthsCnt++;

                    var cpRepo = new CustPortfolioRepo(db);
                    var gzTrx = new GzTransactionRepo(db);

                    SetDbMonthlyPortfolioLossesForTestPlayers(customerEmails, db, monthsCnt, cpRepo,
                        startYearMonthStr, gzTrx);

                    SetDbClearMonth(startYearMonthStr, dbSimpleCtx);

                    // month ++
                    startYearMonthStr = DbExpressions.AddMonth(startYearMonthStr);
                }
            }
        }

        private static void SetDbMonthlyPortfolioLossesForTestPlayers(string[] customerEmails, ApplicationDbContext db,
            int monthsCnt, CustPortfolioRepo cpRepo, string startYearMonthStr, GzTransactionRepo gzTrx) {

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

                    SetDbMonthlyPortfolioSelection(monthsCnt, cpRepo, custId, startYearMonthStr);

                    SetDbMonthlyPlayerLossTrx(startYearMonthStr, gzTrx, custId);
                }
            }
        }

        private static void SetDbClearMonth(string startYearMonthStr, DbUtil.DbSchema.ServiceTypes.SimpleDataContextTypes.Gzdevdb dbSimpleCtx) {

            var lowPortfolioPrice = new PortfolioTypes.PortfolioSharePrice((int)RiskToleranceEnum.Low, 1f,
                DbExpressions.GetDtYearMonthStrToEndOfMonth(startYearMonthStr));
            var mediumPortfolioPrice = new PortfolioTypes.PortfolioSharePrice((int)RiskToleranceEnum.Medium, 1f,
                DbExpressions.GetDtYearMonthStrToEndOfMonth(startYearMonthStr));
            var highPortfolioPrice = new PortfolioTypes.PortfolioSharePrice((int)RiskToleranceEnum.High, 1f,
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

        private static void SetDbMonthlyPortfolioSelection(int monthsCnt, CustPortfolioRepo cpRepo, int custId,
            string startYearMonthStr) {

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

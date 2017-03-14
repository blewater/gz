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
using gzDAL.DTO;
using GzDb;
using Microsoft.FSharp.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = NUnit.Framework.Assert;

namespace gzWeb.Tests.Models
{
    [TestFixture]
    public class InvestmentTests
    {
        private CustPortfolioRepo cpRepo;
        private GzTransactionRepo gzTrx;
        private InvBalanceRepo invBalRepo;
        private ApplicationDbContext db;
        private DbUtil.DbSchema.ServiceTypes.SimpleDataContextTypes.GzDbDev SqlProviderCtx;

        private readonly string[] userEmails = new string[] {
                "salem8@gmail.com",
                "joe@mymail.com",
                "testuser@gz.com",
                "info@nessos.gr",
                "info1@nessos.gr",
                "testuser@gz.com",
                "6month@allocation.com"
            };

        [OneTimeSetUp]
        public void Setup()
        {
            Database.SetInitializer<ApplicationDbContext>(null);

            db = new ApplicationDbContext();
            var devDbConnString = ConfigurationManager.ConnectionStrings["gzDevDb"].ConnectionString;
            SqlProviderCtx = DbUtil.getOpenDb(devDbConnString);
            cpRepo = new CustPortfolioRepo(db);
            gzTrx = new GzTransactionRepo(db);
            var userPortfolio = new CustPortfolioRepo(db);
            invBalRepo = new InvBalanceRepo(db, new CustFundShareRepo(db, userPortfolio), gzTrx, userPortfolio);
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
        public void SetDb6MonthInvBalancesWithFreshTrx()
        {

            var devDbConnString = ConfigurationManager.ConnectionStrings["gzDevDb"].ConnectionString;
            using (ApplicationDbContext db = new ApplicationDbContext(null))
            using (var dbSimpleCtx = DbUtil.getOpenDb(devDbConnString))
            {

                ClearTrxHistory(userEmails);

                var now = DateTime.UtcNow;
                var startYearMonthStr = now.AddMonths(-6).ToStringYearMonth();
                var endYearMonthStr = now.ToStringYearMonth();

                int monthsCnt = 0;
                var usersFound = new List<int>();
                // Loop through all the months activity
                while (startYearMonthStr.BeforeEq(endYearMonthStr))
                {

                    ProcessInvBalances(usersFound, monthsCnt, startYearMonthStr);

                    monthsCnt++;
                    startYearMonthStr = DbExpressions.AddMonth(startYearMonthStr);
                }
                AssertBalanceNumbers(usersFound, endYearMonthStr);
            }
        }

        private void AssertBalanceNumbers(List<int> usersFound, string endYearMonthStr)
        {

            var expectedBal = DbExpressions.RoundCustomerBalanceAmount(8828.171828m);
            var expectedLastVintageShares = DbExpressions.RoundCustomerBalanceAmount(625);
            var expectedInvGain = DbExpressions.RoundCustomerBalanceAmount(1828.171828m);
            foreach (var userId in usersFound)
            {
                var invB = db.InvBalances
                    .Single(i => i.CustomerId == userId &&
                            string.Compare(i.YearMonth, endYearMonthStr, StringComparison.Ordinal) == 0);
                var bal = invB.Balance;
                var totalShares = invB.LowRiskShares + invB.MediumRiskShares + invB.HighRiskShares;
                var gain = invB.InvGainLoss;
                var totalInvestments = invB.TotalCashInvestments;
                Assert.AreEqual(expectedBal, DbExpressions.RoundCustomerBalanceAmount(bal));
                Assert.AreEqual(expectedLastVintageShares, DbExpressions.RoundCustomerBalanceAmount(totalShares));
                Assert.AreEqual(DbExpressions.RoundCustomerBalanceAmount(gain), DbExpressions.RoundCustomerBalanceAmount(bal - totalInvestments));
                Assert.AreEqual(expectedInvGain, DbExpressions.RoundCustomerBalanceAmount(gain));
            }
        }

        private void ProcessInvBalances(
            List<int> usersFound, 
            int monthsCnt,
            string startYearMonthStr)
        {
            foreach (var email in userEmails)
            {
                int custId = db.Users
                    .Where(u => u.Email == email)
                    .Select(u => u.Id)
                    .SingleOrDefault();

                if (custId != 0)
                {
                    usersFound.Add(custId);

                    SetDbMonthlyPortfolioLossesForTestPlayers(custId, monthsCnt, startYearMonthStr);
                }
            }

            SetDbClearMonth(startYearMonthStr, monthsCnt);
        }

        private void ClearTrxHistory(string[] customerEmails)
        {

            foreach (var customerEmail in customerEmails)
            {

                int custId = db.Users
                    .Where(u => u.Email == customerEmail)
                    .Select(u => u.Id)
                    .SingleOrDefault();

                if (custId != 0)
                {

                    // Clear all customer activity
                    db.Database.ExecuteSqlCommand("Delete GzTrxs Where CustomerId = " + custId);
                    db.Database.ExecuteSqlCommand("Delete VintageShares Where UserId = " + custId);
                    db.Database.ExecuteSqlCommand("Delete InvBalances Where CustomerId = " + custId);

                }
            }
        }

        private void SetDbMonthlyPortfolioLossesForTestPlayers(
                int userId, 
                int monthsCnt,
                string startYearMonthStr)
        {

            SetDbMonthlyPortfolioSelection(monthsCnt, userId, startYearMonthStr);

            SetDbMonthlyPlayerLossTrx(startYearMonthStr, userId);
        }

        private void SetDbClearMonth(string startYearMonthStr, int monthCnt)
        {

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

            UserTrx.processGzTrx(SqlProviderCtx, startYearMonthStr, portfoliosPriceMap);
        }

        private void SetDbMonthlyPlayerLossTrx(string trxYearMonthStr, int custId)
        {

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
                trxYearMonthStr,
                createdOnUtc,
                3000, 3000, 1000, -2000, 3000);
        }

        private void SetDbMonthlyPortfolioSelection(int monthsCnt, int userId, string startYearMonthStr)
        {

            var monthPortfolioRisk =
                monthsCnt % 3 == 0
                    ? RiskToleranceEnum.Low
                    : monthsCnt % 3 == 1
                        ? RiskToleranceEnum.Medium
                        : RiskToleranceEnum.High;

            // Create loss transaction for the month
            cpRepo.SaveDbCustMonthsPortfolioMix(
                userId,
                monthPortfolioRisk,
                DbExpressions.GetYear(startYearMonthStr),
                DbExpressions.GetMonth(startYearMonthStr),
                DateTime.UtcNow);
        }

        private void SetDbMonthlyPlayerLossTrx_D(string trxYearMonthStr, int userId)
        {

            var createdOnUtc =
                new DateTime(
                    DbExpressions.GetYear(trxYearMonthStr),
                    DbExpressions.GetMonth(trxYearMonthStr),
                    15,
                    19,12,59,333,
                    DateTimeKind.Utc);

            switch (trxYearMonthStr) {
                case "201611":
                case "201612":
                case "201702":
                    gzTrx.SaveDbPlayingLoss(
                        userId,
                        20,
                        trxYearMonthStr,
                        createdOnUtc,
                        30, 30, 10, -20, 30);
                    break;
                case "201701" :
                    gzTrx.SaveDbPlayingLoss(
                        userId,
                        30, 
                        trxYearMonthStr,
                        createdOnUtc,
                        30, 40, 10, -30, 30);
                    break;
                case "201703":
                    gzTrx.SaveDbPlayingLoss(
                        userId,
                        40, 
                        trxYearMonthStr,
                        createdOnUtc,
                        30, 50, 10, -40, 30);
                    break;
            }
        }

        private PortfolioTypes.PortfoliosPrices createMonthlyPortfolioPrices(string currentYearMonthStr, double stockPrice) {

            var lowPortfolioPrice = new PortfolioTypes.PortfolioSharePrice((int) RiskToleranceEnum.Low, stockPrice,
                DbExpressions.GetDtYearMonthStrToEndOfMonth(currentYearMonthStr));
            var mediumPortfolioPrice = new PortfolioTypes.PortfolioSharePrice((int) RiskToleranceEnum.Medium, stockPrice,
                DbExpressions.GetDtYearMonthStrToEndOfMonth(currentYearMonthStr));
            var highPortfolioPrice = new PortfolioTypes.PortfolioSharePrice((int) RiskToleranceEnum.High, stockPrice,
                DbExpressions.GetDtYearMonthStrToEndOfMonth(currentYearMonthStr));
            var portfolioPricesEoM = new PortfolioTypes.PortfoliosPrices(lowPortfolioPrice, mediumPortfolioPrice,
                highPortfolioPrice);
            return portfolioPricesEoM;
        }

        private FSharpMap<string, PortfolioTypes.PortfoliosPrices> GetPortfoliosPricesMapTable(string currentYearMonthStr)
        {

            double stockPrice = -1;
            PortfolioTypes.PortfoliosPrices monthlyPortfolioPrices = null;
            switch (currentYearMonthStr)
            {
                case "201611":
                case "201703":
                    stockPrice = 1;
                    break;
                case "201612":
                    stockPrice = 2;
                    break;
                case "201701":
                    stockPrice = 3;
                    break;
                case "201702":
                    stockPrice = 5;
                    break;
                default:
                    Assert.Fail("Not Agreed Month: " + currentYearMonthStr);
                    break;
            }
            monthlyPortfolioPrices = createMonthlyPortfolioPrices(currentYearMonthStr, stockPrice);

            var portfoliosPriceDict = new Dictionary<string, PortfolioTypes.PortfoliosPrices>() {
                {
                    DbExpressions.GetDtYearMonthStrToEndOfMonth(currentYearMonthStr).ToStringYearMonthDay(),
                    monthlyPortfolioPrices
                }
            };
            // Convert .net dictinary to F# map
            var portfoliosPriceMap = PortfolioTypes.toMap(portfoliosPriceDict);
            return portfoliosPriceMap;
        }

        private FSharpMap<string, PortfolioTypes.PortfoliosPrices> SetDbPortfolioPrices(string currentYearMonthStr, int monthCnt)
        {
            var portfoliosPriceMap = GetPortfoliosPricesMapTable(currentYearMonthStr);

            // Call F# implementation
            // Save portfolio prices
            DailyPortfolioShares.setDbPortfoliosPrices(SqlProviderCtx, portfoliosPriceMap);

            return portfoliosPriceMap;
        }

        private void ProcessInvBalances_D(
            List<int> usersFound,
            int monthsCnt,
            string currentYearMonthStr) {

            FSharpMap<string, PortfolioTypes.PortfoliosPrices> portfoliosPriceMap = null;

            foreach (var email in userEmails) {

                int userId = db.Users
                    .Where(u => u.Email == email)
                    .Select(u => u.Id)
                    .SingleOrDefault();

                if (userId != 0) {
                    usersFound.Add(userId);

                    SetDbMonthlyPortfolioSelection(monthsCnt, userId, currentYearMonthStr);

                    SetDbMonthlyPlayerLossTrx_D(currentYearMonthStr, userId);

                    var userVintages = AssertUserVintagesCount(userId, monthsCnt);

                    portfoliosPriceMap = SetDbPortfolioPrices(currentYearMonthStr, monthsCnt);

                    SellVintages(currentYearMonthStr, userVintages, userId);
                }
            }

            // Process losses -> invbalance
            UserTrx.processGzTrx(SqlProviderCtx, currentYearMonthStr, portfoliosPriceMap);

            AssertAllUsersVintagesCountAfterMonthClearance(usersFound, monthsCnt + 1);
        }

        private void SellVintages(string currentYearMonthStr, ICollection<VintageDto> userVintages, int userId) {

            switch (currentYearMonthStr) {
                case "201702":
                    var selectedVintages =
                        userVintages
                            .Where(v => v.YearMonthStr == "201611" || v.YearMonthStr == "201612")
                            .ToList();
                    foreach (var selectedVintage in selectedVintages) {
                        selectedVintage.Selected = true;
                        // Override locking period
                        selectedVintage.Locked = false;
                    }
                    break;
            }
            invBalRepo.SaveDbSellVintages(userId, userVintages, currentYearMonthStr);
        }

        private ICollection<VintageDto> AssertUserVintagesCount(int userId, int numberOfVintages) {

            var userVintages = invBalRepo.GetCustomerVintages(userId);

            // Pre Clearance vintages.count check: First month (Nov) is 0 vintages
            Assert.AreEqual(numberOfVintages, userVintages.Count);
            return userVintages;
        }

        private void AssertAllUsersVintagesCountAfterMonthClearance(List<int> usersFound, int numberOfVintages) {

            foreach (var userId in usersFound) {

                AssertUserVintagesCount(userId, numberOfVintages);
            }
        }

        private void AssertBalanceNumbers_D(List<int> usersFound, string currentYearMonthStr)
        {

            decimal expectedBal = int.MinValue, expectedLastVintageShares = int.MinValue, expectedTotalShares = int.MinValue, expectedInvGain = int.MinValue;

            switch (currentYearMonthStr) {
                case "201611":
                    expectedBal = 10;
                    expectedLastVintageShares = expectedTotalShares = 10;
                    expectedInvGain = 0;
                    break;
                case "201612":
                    expectedBal = 30;
                    expectedLastVintageShares = 5;
                    expectedTotalShares = 15;
                    expectedInvGain = 10;
                    break;
                case "201701":
                    expectedBal = 60;
                    expectedLastVintageShares = 5;
                    expectedTotalShares = 20;
                    expectedInvGain = 25;
                    break;
                case "201702":
                    expectedBal = 35;
                    expectedLastVintageShares = 2;
                    expectedTotalShares = 7;
                    expectedInvGain = 10;
                    break;
                case "201703":
                    expectedBal = 27;
                    expectedLastVintageShares = 20;
                    expectedTotalShares = 27;
                    expectedInvGain = -18;
                    break;
            }
            AssertUsersBalances(usersFound, currentYearMonthStr, expectedBal, expectedLastVintageShares, expectedTotalShares, expectedInvGain);
        }

        private void AssertUsersBalances(
            List<int> usersFound, 
            string currentYearMonthStr, 
            decimal expectedBal,
            decimal expectedLastVintageShares,
            decimal expectedTotalShares,
            decimal expectedInvGain) {

            foreach (var userId in usersFound) {
                var invB = db.InvBalances
                    .Single(
                        i => 
                            i.CustomerId == userId 
                            &&
                            string.Compare(i.YearMonth, currentYearMonthStr, StringComparison.Ordinal) == 0);
                var totalVintagesShares = db.VintageShares
                    .Single(
                        s =>
                            s.UserId == userId 
                            &&
                            string.Compare(s.YearMonth, currentYearMonthStr, StringComparison.Ordinal) == 0);
                var bal = invB.Balance;
                var thisVintageShares = invB.LowRiskShares + invB.MediumRiskShares + invB.HighRiskShares;
                var totalShares = totalVintagesShares.PortfolioLowShares + totalVintagesShares.PortfolioMediumShares +
                                  totalVintagesShares.PortfolioHighShares;
                var gain = invB.InvGainLoss;
                var totalInvestments = invB.TotalCashInvInHold;

                Assert.AreEqual(expectedBal, DbExpressions.RoundCustomerBalanceAmount(bal));
                Assert.AreEqual(expectedLastVintageShares, DbExpressions.RoundCustomerBalanceAmount(thisVintageShares));
                Assert.AreEqual(expectedTotalShares, DbExpressions.RoundCustomerBalanceAmount(totalShares));

                // Assert invGain in both ways: 
                // 1. bal-total cash invested
                Assert.AreEqual(DbExpressions.RoundCustomerBalanceAmount(gain),
                    DbExpressions.RoundCustomerBalanceAmount(bal - totalInvestments));

                // 2. db gain figure
                Assert.AreEqual(expectedInvGain, DbExpressions.RoundCustomerBalanceAmount(gain));
            }
        }

        [Test]
        public void SetDbInvBalanceSoldVintages_D()
        {

            ClearTrxHistory(userEmails);

            var now = DateTime.UtcNow;
            var startYearMonthStr = new DateTime(2016, 11, 1).ToStringYearMonth();
            var endYearMonthStr = new DateTime(2017, 3, 1).ToStringYearMonth();

            int monthsCnt = 0;
            var usersFound = new List<int>();
            // Loop through all the months activity
            while (startYearMonthStr.BeforeEq(endYearMonthStr))
            {
                ProcessInvBalances_D(usersFound, monthsCnt, startYearMonthStr);

                AssertBalanceNumbers_D(usersFound, startYearMonthStr);

                monthsCnt++;
                startYearMonthStr = DbExpressions.AddMonth(startYearMonthStr);
            }
        }
    }
}

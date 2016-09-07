using NUnit.Framework;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.Repos;
using Microsoft.AspNet.Identity;
using Assert = NUnit.Framework.Assert;
using System.Collections.Generic;
using gzDAL.ModelUtil;

namespace gzWeb.Tests.Models {
    [TestFixture]
    public class InvestmentTests {

        [OneTimeSetUp]
        public void Setup() {
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public void CreateTestCustomerPortfolioSelections(int custId) {
            var db = new ApplicationDbContext(null);
            var cpRepo = new CustPortfolioRepo(db);

            /*** For phase I we test only single portfolio selections ***/
            //// Jan 2015
            cpRepo.SaveDbCustMonthsPortfolioMix(custId, RiskToleranceEnum.Low, 100, 2015, 1, new DateTime(2015, 1, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Medium, 50, 2015, 1, new DateTime(2015, 1, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.High, 20, 2015, 1, new DateTime(2015, 1, 1));
            //// Feb
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Low, 10, 2015, 2, new DateTime(2015, 2, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Medium, 50, 2015, 2, new DateTime(2015, 2, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.High, 40, 2015, 2, new DateTime(2015, 2, 1));
            // Mar
            cpRepo.SaveDbCustMonthsPortfolioMix(custId, RiskToleranceEnum.High, 100, 2015, 3, new DateTime(2015, 3, 1));
            //// Apr
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Low, 30, 2015, 4, new DateTime(2015, 4, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Medium, 30, 2015, 4, new DateTime(2015, 4, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.High, 40, 2015, 4, new DateTime(2015, 4, 1));
            //// May
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Low, 10, 2015, 5, new DateTime(2015, 5, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Medium, 20, 2015, 5, new DateTime(2015, 5, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.High, 70, 2015, 5, new DateTime(2015, 5, 1));
            // Jun
            cpRepo.SaveDbCustMonthsPortfolioMix(custId, RiskToleranceEnum.Medium, 100, 2015, 6, new DateTime(2015, 6, 1));

            cpRepo.SaveDbCustomerSelectNextMonthsPortfolio(custId, RiskToleranceEnum.High);
        }

        [Test]
        public void SaveDbSellPortfolio() {

            using (var db = new ApplicationDbContext(null)) {

                var custId = CreateTestCustomer(db, TestUser6Month(db));

                // Last Day of present Month @ 23:00
                var yearCurrent = DateTime.UtcNow.Year;
                var monthCurrent = DateTime.UtcNow.Month;
                var lastMonthDay = new DateTime(yearCurrent, monthCurrent, DateTime.DaysInMonth(yearCurrent, monthCurrent),
                    23, 00, 00);

                var custPortfolioRepo = new CustPortfolioRepo(db);
                var soldShares = new InvBalanceRepo(
                    db, 
                    new CustFundShareRepo(db, custPortfolioRepo),
                    new GzTransactionRepo(db),
                    custPortfolioRepo)
                    .SaveDbSellAllCustomerFundsShares(
                        custId, 
                        lastMonthDay);

                Console.WriteLine("SaveDbSellPortfolio() returned soldShares: " + soldShares);

            }
        }

        [Test]
        public void SaveDbAfterCleanTrxTblUpdBalances() {

            string[] customerEmails = new string[] {
                "salem8@gmail.com",
                "info@nessos.gr",
                "info1@nessos.gr",
                "testuser@gz.com",
                "6month@allocation.com"
            };

            using (ApplicationDbContext db = new ApplicationDbContext(null)) {

                var custPortfolioRepo = new CustPortfolioRepo(db);
                var invBalance = new InvBalanceRepo(
                    db,
                    new CustFundShareRepo(db, custPortfolioRepo),
                    new GzTransactionRepo(db),
                    custPortfolioRepo);

                    foreach (var customerEmail in customerEmails) {

                    int custId = db.Users
                        .Where(u => u.Email == customerEmail)
                        .Select(u => u.Id)
                        .SingleOrDefault();

                    if (custId != 0) {

                        db.Database.ExecuteSqlCommand("Delete GzTrxs Where CustomerId = " + custId);
                        db.Database.ExecuteSqlCommand("Delete CustFundShares Where CustomerId = " + custId);
                        db.Database.ExecuteSqlCommand("Update InvBalances Set Sold = 0 Where CustomerId = " + custId);

                        // Add invested Customer Portfolio
                        CreateTestCustomerPortfolioSelections(custId);

                        CreateTestPlayerLossTransactions(custId);

                        invBalance.SaveDbCustomerAllMonthlyBalances(custId);
                    }

                }
            }
        }

        public void CreateTestPlayerDepositWidthdrawnTransactions(int custId) {
            using (var db = new ApplicationDbContext(null)) {
                var gzTrx = new GzTransactionRepo(db);

                // Add deposit, withdrawals
                gzTrx.SaveDbTransferToGamingAmount(custId, 50, new DateTime(2015, 4, 30));

                gzTrx.SaveDbGmTransaction(customerId: custId, gzTransactionType: GmTransactionTypeEnum.Deposit,
                    amount: 100, createdOnUtc: new DateTime(2015, 7, 15));

                gzTrx.SaveDbTransferToGamingAmount(custId, 40, new DateTime(2015, 5, 31));
                gzTrx.SaveDbTransferToGamingAmount(custId, 20, new DateTime(2015, 6, 15));
            }
        }

        public void CreateTestPlayerLossTransactions(int custId) {
            using (var db = new ApplicationDbContext(null)) {
                var gzTrx = new GzTransactionRepo(db);

                var startYearMonthStr = "201501";
                var endYeerMonthStr = DateTime.UtcNow.AddMonths(2).ToStringYearMonth();

                while (startYearMonthStr.BeforeEq(endYeerMonthStr)) {

                    var createdOnUtc = 
                        new DateTime(
                            int.Parse(startYearMonthStr.Substring(0, 4)),
                            int.Parse(startYearMonthStr.Substring(4, 2)), 
                            15,
                            19,
                            12,
                            59,
                            333,
                            DateTimeKind.Utc);

                    gzTrx.SaveDbPlayingLoss(
                        customerId: custId, 
                        totPlayinLossAmount: 200, 
                        createdOnUtc: createdOnUtc);

                    startYearMonthStr = DbExpressions.AddMonth(startYearMonthStr);
                }
            }
        }

        [Test]
        public void SaveDbUpdAllCustomerBalances() {
            using (var db = new ApplicationDbContext(null)) {

                var custPortfolioRepo = new CustPortfolioRepo(db);
                new InvBalanceRepo(db, 
                    new CustFundShareRepo(db, custPortfolioRepo), 
                    new GzTransactionRepo(db), 
                    custPortfolioRepo)
                    
                    .SaveDbAllCustomersMonthlyBalances();
            }
        }

        [Test]
        public void SaveDbUpdOneCustomerOneMonthBalance() {
            UpdateInvBalanceOneMonth("201501");
        }

        private static void UpdateInvBalanceOneMonth(string yearMonth) {

            using (var db = new ApplicationDbContext(null)) {

                var custPortfolioRepo = new CustPortfolioRepo(db);
                new InvBalanceRepo(
                    db,
                    new CustFundShareRepo(db, custPortfolioRepo), new GzTransactionRepo(db), custPortfolioRepo)
                    .SaveDbCustomerMonthlyBalance(
                        /** 6month User **/
                        db.Users.Where(u => u.Email == "salem8@gmail.com")
                            .Select(u => u.Id)
                            /** Update month balance **/
                            .Single(), yearMonth);
            }
        }

        [Test]
        public void SaveDbUpdOneCustomerPresentMonthBalance() {
            var yearMonthNow = DateTime.UtcNow.ToStringYearMonth();
            UpdateInvBalanceOneMonth(yearMonthNow);
        }

        private int CreateTestCustomer(ApplicationDbContext db, ApplicationUser newUser) {

            db.Users.AddOrUpdate(c => new { c.Email }, newUser);
            db.SaveChanges();

            var custId = db.Users.Where(u => u.Email == newUser.Email).Select(u => u.Id).Single();
            return custId;
        }

#region TestUsers

        private ApplicationUser TestInfo(ApplicationDbContext db) {

            var manager = new ApplicationUserManager(new CustomUserStore(db),
                new DataProtectionProviderFactory(() => null));

            var newUser = new ApplicationUser {
                UserName = "infonesos",
                Email = "info@nessos.gr",
                FirstName = "Info",
                LastName = "Nessos",
                Birthday = new DateTime(1975, 10, 13),
                Currency = "SEK",
                PasswordHash = manager.PasswordHasher.HashPassword("gz2016!@"),
                EmailConfirmed = true
            };
            return newUser;
        }

        private ApplicationUser TestUser6Month(ApplicationDbContext db) {

            var manager = new ApplicationUserManager(new CustomUserStore(db),
                new DataProtectionProviderFactory(() => null));

            var newUser = new ApplicationUser() {
                UserName = "6month@allocation.com",
                Email = "6month@allocation.com",
                EmailConfirmed = true,
                FirstName = "Six",
                LastName = "Month",
                Birthday = new DateTime(1990, 1, 1),
                Currency = "SEK",
                PasswordHash = manager.PasswordHasher.HashPassword("1q2w3e")
            };
            return newUser;
        }

        private ApplicationUser TestUser(ApplicationDbContext db) {

            var manager = new ApplicationUserManager(new CustomUserStore(db),
                new DataProtectionProviderFactory(() => null));

            var newUser = new ApplicationUser {
                UserName = "testuser",
                Email = "testuser@gz.com",
                FirstName = "test",
                LastName = "user",
                Birthday = new DateTime(1975, 10, 13),
                Currency = "EUR",
                PasswordHash = manager.PasswordHasher.HashPassword("gz2016!@"),
                EmailConfirmed = true
            };
            return newUser;
        }

#endregion

    }
}

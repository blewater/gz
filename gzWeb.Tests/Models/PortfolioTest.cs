using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gzDAL.Models;
using System.Diagnostics;
using gzDAL.Repos;
using AutoMapper;
using gzDAL.DTO;
using gzDAL.Conf;

namespace gzWeb.Tests.Models {
    [TestClass]
    public class PortfolioTest {

        [ClassInitialize]
        public static void PortfolioTestInitialize(TestContext context)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<CustomerDTO, ApplicationUser>());
        }

        /// <summary>
        ///
        /// Test expected returns formula below based on a industry standard
        /// calculation i.e. Time weighted return.
        ///
        /// Asserts taken from calculator on https://smartasset.com/investing/investment-calculator#iw7vdcEJRj
        ///
        /// </summary>
        [TestMethod]
        public void TimeWeightedReturnCalc() {

            // Case 1
            decimal startingAmount = 1000;
            decimal RoR = 10; // 10%
            int yearsOfInv = 10;
            decimal annualContribution = 100;

            var expectedRet = ExpectedRet(yearsOfInv, startingAmount, RoR, annualContribution);

            Assert.AreEqual(4187, Math.Round(expectedRet, 0));

            // Case 2
            startingAmount = 5356;
            RoR = 11.43m; // 11.43%
            yearsOfInv = 13;
            annualContribution = 141;

            expectedRet = ExpectedRet(yearsOfInv, startingAmount, RoR, annualContribution);

            // Expected by https://smartasset.com/investing/investment-calculator#iw7vdcEJRj
            Assert.AreEqual(25675, Math.Round(expectedRet, 0));
        }

        /// <summary>
        ///
        /// Time weighted return formula I think :)
        /// Based on calculator on https://smartasset.com/investing/investment-calculator#iw7vdcEJRj
        ///
        /// It is assumed that all cash distributions
        /// are reinvested in the portfolio.
        /// The effect of the Cash inflows timing is eliminated
        /// by assuming a single investment at the end of the year.
        ///
        /// </summary>
        /// <param name="yearsOfInv">Years To Grow</param>
        /// <param name="startingAmount"></param>
        /// <param name="roR">Rate at which investment will grow. Human Rate Input i.e. 14.25 for 14.25%</param>
        /// <param name="annualContribution">Annual capital investment amount</param>
        /// <returns>End Balance at year <see cref="yearsOfInv"/></returns>
        private static decimal ExpectedRet(int yearsOfInv, decimal startingAmount, decimal roR, decimal annualContribution) {
            for (var i = 0; i < yearsOfInv; i++) {
                startingAmount = startingAmount*(1 + roR/100) + annualContribution;
            }
            return startingAmount;
        }

        /// <summary>
        /// Get the Apr returns based on the 3 year or 5 year returns whichever is greater
        /// </summary>
        [TestMethod]
        public void PortfolioReturns() {
            var db = new ApplicationDbContext();
            var portfolioLines = new PortfolioRepository(db).GetPortfolioRetLines();

            foreach (var l in portfolioLines) {
                Console.WriteLine(l);
            }
        }
        [TestMethod]
        public void SaveDailyCurrenciesRates() {
            var currencyRateRepo = new CurrencyRateRepo(new ApplicationDbContext());
            var quotes = currencyRateRepo.SaveDbDailyCurrenciesRates();

            Assert.IsNotNull(quotes);

            //Assert we have a closing price for first symbol
            Assert.IsTrue(quotes[0].TradeDateTime.HasValue);
        }
        [TestMethod]
        public void SaveDailyFundClosingPrice() {
            var db = new ApplicationDbContext();
            var fundRepo = new FundRepo(db);
            var quotes = fundRepo.SaveDbDailyFundClosingPrices();

            Assert.IsNotNull(quotes);

            //Assert we have a closing price for first symbol
            Assert.IsTrue(quotes[0].LastTradePrice.HasValue);
        }

        [TestMethod]
        public void SaveDbSellPortfolio() {

            int custId = CreateTestCustomer();

            var db = new ApplicationDbContext();
            var soldShares = new InvBalanceRepo(db, new CustFundShareRepo(db), new GzTransactionRepo(db))
                .SaveDbSellCustomerPortfolio(custId);

            Console.WriteLine("SaveDbSellPortfolio() returned soldShares: " + soldShares);
        }

        [TestMethod]
        public void SaveDbUpdCustomerInvBalances() {

            int custId = CreateTestCustomer();

            // Add invested Customer Portfolio
            CreateTestCustomerPortfolioSelections(custId);

            CreateTestPlayerLossTransactions(custId);
            CreateTestPlayerDepositWidthdrawnTransactions(custId);

            var db = new ApplicationDbContext();
            new InvBalanceRepo(db, new CustFundShareRepo(db), new GzTransactionRepo(db))
                .SaveDbCustomerMonthlyBalancesByTrx(custId);
        }

        public void CreateTestCustomerPortfolioSelections(int custId) {
            var db = new ApplicationDbContext();
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
        }

        public void CreateTestPlayerLossTransactions(int custId) {
            var db = new ApplicationDbContext();
            var gzTrx = new GzTransactionRepo(db);

            // Add playing losses for first 6 months of 2015
            gzTrx.SaveDbPlayingLoss(customerId: custId, totPlayinLossAmount: 160, creditPcnt: 50, createdOnUtc: new DateTime(2015, 1, 1));
            gzTrx.SaveDbPlayingLoss(customerId: custId, totPlayinLossAmount: 120, creditPcnt: 50, createdOnUtc: new DateTime(2015, 2, 1));
            gzTrx.SaveDbPlayingLoss(customerId: custId, totPlayinLossAmount: 200, creditPcnt: 50, createdOnUtc: new DateTime(2015, 3, 1));
            gzTrx.SaveDbPlayingLoss(customerId: custId, totPlayinLossAmount: 170, creditPcnt: 50, createdOnUtc: new DateTime(2015, 4, 1));
            gzTrx.SaveDbPlayingLoss(customerId: custId, totPlayinLossAmount: 300, creditPcnt: 50, createdOnUtc: new DateTime(2015, 5, 1));
            gzTrx.SaveDbPlayingLoss(customerId: custId, totPlayinLossAmount: 200, creditPcnt: 50, createdOnUtc: new DateTime(2015, 6, 1));
            gzTrx.SaveDbPlayingLoss(customerId: custId, totPlayinLossAmount: 200, creditPcnt: 50, createdOnUtc: new DateTime(2016, 1, 1));
            gzTrx.SaveDbPlayingLoss(customerId: custId, totPlayinLossAmount: 100, creditPcnt: 50, createdOnUtc: new DateTime(2015, 9, 30));
        }


        public void CreateTestPlayerDepositWidthdrawnTransactions(int custId) {
            var db = new ApplicationDbContext();
            var gzTrx = new GzTransactionRepo(db);

            // Add deposit, withdrawals
            gzTrx.SaveDbTransferToGamingAmount(custId, 50, new DateTime(2015, 4, 30));
            gzTrx.SaveDbGzTransaction(customerId: custId, gzTransactionType: GzTransactionJournalTypeEnum.Deposit, amount: 100, createdOnUtc: new DateTime(2015, 7, 15));
            gzTrx.SaveDbInvWithdrawalAmount(custId, 30, new DateTime(2015, 5, 30));
            gzTrx.SaveDbTransferToGamingAmount(custId, 40, new DateTime(2015, 5, 31));
            gzTrx.SaveDbInvWithdrawalAmount(custId, 40, new DateTime(2015, 6, 1));
            gzTrx.SaveDbTransferToGamingAmount(custId, 20, new DateTime(2015, 6, 15));
        }

        public static int CreateTestCustomer() {

            Random rnd = new Random();
            int rndPlatformId = rnd.Next(1, int.MaxValue);

            var newUserDTO = new CustomerDTO() {
                UserName = "6month@allocation.com",
                Email = "6month@allocation.com",
                EmailConfirmed = true,
                FirstName = "Six",
                LastName = "Month",
                Birthday = new DateTime(1990, 1, 1),
                PlatformCustomerId = rndPlatformId,
                GamBalance = new decimal(4200.54),
                GamBalanceUpdOnUTC = DateTime.UtcNow
            };
            var newUser = new ApplicationUser();
            Mapper.Map<CustomerDTO, ApplicationUser>(newUserDTO, newUser);

            var db = new ApplicationDbContext();
            var custRepo = new CustomerRepo(new ApplicationUserManager(new CustomUserStore(db)));
            return custRepo.CreateOrUpdateUser(newUser, "1q2w3e");
        }
    }
}

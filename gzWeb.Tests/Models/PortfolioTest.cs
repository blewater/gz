using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gzWeb.Models;
using System.Diagnostics;
using gzWeb.Repo;
using AutoMapper;

namespace gzWeb.Tests.Models {
    [TestClass]
    public class PortfolioTest {

        [ClassInitialize]
        public static void PortfolioTestInitialize(TestContext context)
        {
            Mapper.Initialize(cfg => cfg.CreateMap<CustomerDTO, ApplicationUser>());
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
            var quotes = currencyRateRepo.SaveDBDailyCurrenciesRates();

            Assert.IsNotNull(quotes);

            //Assert we have a closing price for first symbol
            Assert.IsTrue(quotes[0].TradeDateTime.HasValue);
        }
        [TestMethod]
        public void SaveDailyFundClosingPrice() {
            var db = new ApplicationDbContext();
            var fundRepo = new FundRepo(db);
            var quotes = fundRepo.SaveDBDailyFundClosingPrices();

            Assert.IsNotNull(quotes);

            //Assert we have a closing price for first symbol
            Assert.IsTrue(quotes[0].LastTradePrice.HasValue);
        }
        [TestMethod]
        public void SaveDBPortfolioReturns() {

            int custId = CreateTestCustomer();

            // Add invested Customer Portfolio 
            CreateTestCustomerPortfolioSelections(custId);

            CreateTestPlayerLossTransactions(custId);
            CreateTestPlayerDepositWidthdrawnTransactions(custId);

            var db = new ApplicationDbContext();
            new InvBalanceRepo(db, new CustFundShareRepo(db)).SaveCustTrxsBalances(custId);
        }

        public void CreateTestCustomerPortfolioSelections(int custId) {
            var db = new ApplicationDbContext();
            var cpRepo = new CustPortfolioRepo(db);

            /*** For phase I we test only single portfolio selections ***/
            //// Jan 2015
            cpRepo.SaveDBCustMonthsPortfolioMix(custId, RiskToleranceEnum.Low, 100, 2015, 1, new DateTime(2015, 1, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Medium, 50, 2015, 1, new DateTime(2015, 1, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.High, 20, 2015, 1, new DateTime(2015, 1, 1));
            //// Feb
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Low, 10, 2015, 2, new DateTime(2015, 2, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Medium, 50, 2015, 2, new DateTime(2015, 2, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.High, 40, 2015, 2, new DateTime(2015, 2, 1));
            // Mar
            cpRepo.SaveDBCustMonthsPortfolioMix(custId, RiskToleranceEnum.High, 100, 2015, 3, new DateTime(2015, 3, 1));
            //// Apr
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Low, 30, 2015, 4, new DateTime(2015, 4, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Medium, 30, 2015, 4, new DateTime(2015, 4, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.High, 40, 2015, 4, new DateTime(2015, 4, 1));
            //// May
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Low, 10, 2015, 5, new DateTime(2015, 5, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Medium, 20, 2015, 5, new DateTime(2015, 5, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.High, 70, 2015, 5, new DateTime(2015, 5, 1));
            // Jun
            cpRepo.SaveDBCustMonthsPortfolioMix(custId, RiskToleranceEnum.Medium, 100, 2015, 6, new DateTime(2015, 6, 1));
        }

        public void CreateTestPlayerLossTransactions(int custId) {
            var db = new ApplicationDbContext();
            var gzTrx = new GzTransactionRepo(db);

            // Add playing losses for first 6 months of 2015
            gzTrx.SaveDBPlayingLoss(customerId: custId, totPlayinLossAmount: 160, creditPcnt: 50, createdOnUTC: new DateTime(2015, 1, 1));
            gzTrx.SaveDBPlayingLoss(customerId: custId, totPlayinLossAmount: 120, creditPcnt: 50, createdOnUTC: new DateTime(2015, 2, 1));
            gzTrx.SaveDBPlayingLoss(customerId: custId, totPlayinLossAmount: 200, creditPcnt: 50, createdOnUTC: new DateTime(2015, 3, 1));
            gzTrx.SaveDBPlayingLoss(customerId: custId, totPlayinLossAmount: 170, creditPcnt: 50, createdOnUTC: new DateTime(2015, 4, 1));
            gzTrx.SaveDBPlayingLoss(customerId: custId, totPlayinLossAmount: 300, creditPcnt: 50, createdOnUTC: new DateTime(2015, 5, 1));
            gzTrx.SaveDBPlayingLoss(customerId: custId, totPlayinLossAmount: 200, creditPcnt: 50, createdOnUTC: new DateTime(2015, 6, 1));
            gzTrx.SaveDBPlayingLoss(customerId: custId, totPlayinLossAmount: 200, creditPcnt: 50, createdOnUTC: new DateTime(2016, 1, 1));
            gzTrx.SaveDBPlayingLoss(customerId: custId, totPlayinLossAmount: 100, creditPcnt: 50, createdOnUTC: new DateTime(2015, 9, 30));
        }


        public void CreateTestPlayerDepositWidthdrawnTransactions(int custId) {
            var db = new ApplicationDbContext();
            var gzTrx = new GzTransactionRepo(db);

            // Add deposit, withdrawals
            gzTrx.SaveDBTransferToGamingAmount(custId, 50, new DateTime(2015, 4, 30));
            gzTrx.SaveDBGzTransaction(customerId: custId, gzTransactionType: TransferTypeEnum.Deposit, amount: 100, createdOnUTC: new DateTime(2015, 7, 15));
            gzTrx.SaveDBInvWithdrawalAmount(custId, 30, new DateTime(2015, 5, 30));
            gzTrx.SaveDBTransferToGamingAmount(custId, 40, new DateTime(2015, 5, 31));
            gzTrx.SaveDBInvWithdrawalAmount(custId, 40, new DateTime(2015, 6, 1));
            gzTrx.SaveDBTransferToGamingAmount(custId, 20, new DateTime(2015, 6, 15));
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

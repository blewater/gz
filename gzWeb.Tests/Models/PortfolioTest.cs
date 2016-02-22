using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gzWeb.Models;
using System.Diagnostics;

namespace gzWeb.Tests.Models {
    [TestClass]
    public class PortfolioTest {
        /// <summary>
        /// Get the Apr returns based on the 3 year or 5 year returns whichever is greater
        /// </summary>
        [TestMethod]
        public void PortfolioReturns() {

            var portfolios = new PortfolioRepository().GetAllPortfolios();

            foreach (var p in portfolios.ToList()) {
                
                Console.Write(p.RiskTolerance.ToString() + " portfolio return ");
                var r = p.PortFunds.Select(f => f.Weight * Math.Max(f.Fund.ThreeYrReturnPcnt, f.Fund.FiveYrReturnPcnt)).Sum();
                Console.WriteLine(r + "%");
            }
        }
        [TestMethod]
        public void SaveDailyCurrenciesRates() {
            var currencyRateRepo = new CurrencyRateRepo();
            var quotes = currencyRateRepo.SaveDBDailyCurrenciesRates();

            Assert.IsNotNull(quotes);

            //Assert we have a closing price for first symbol
            Assert.IsTrue(quotes[0].TradeDateTime.HasValue);
        }
        [TestMethod]
        public void SaveDailyFundClosingPrice() {
            var fundRepo = new FundRepo();
            var quotes = fundRepo.SaveDBDailyFundClosingPrices();

            Assert.IsNotNull(quotes);

            //Assert we have a closing price for first symbol
            Assert.IsTrue(quotes[0].LastTradePrice.HasValue);
        }
        [TestMethod]
        public async Task CalculatePortfolioReturns() {

            int custId = CreateTestCustomer();

            // Add invested Customer Portfolio 
            await CreateTestCustomerPortfolioSelections(custId);

            await CreateTestPlayerLossTransactions(custId);
            await CreateTestPlayerDepositWidthdrawnTransactions(custId);

            new InvBalanceRepo().SaveCustTrxsBalances(custId);
        }

        [TestMethod]
        public static async Task CreateTestCustomerPortfolioSelections(int custId) {

            var cpRepo = new CustPortfolioRepo();

            /*** For phase I we test only single portfolio selections ***/
            //// Jan 2015
            await cpRepo.SetCustMonthsPortfolioMix(custId, RiskToleranceEnum.Low, 100, 2015, 1, new DateTime(2015, 1, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Medium, 50, 2015, 1, new DateTime(2015, 1, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.High, 20, 2015, 1, new DateTime(2015, 1, 1));
            //// Feb
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Low, 10, 2015, 2, new DateTime(2015, 2, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Medium, 50, 2015, 2, new DateTime(2015, 2, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.High, 40, 2015, 2, new DateTime(2015, 2, 1));
            // Mar
            await cpRepo.SetCustMonthsPortfolioMix(custId, RiskToleranceEnum.High, 100, 2015, 3, new DateTime(2015, 3, 1));
            //// Apr
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Low, 30, 2015, 4, new DateTime(2015, 4, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Medium, 30, 2015, 4, new DateTime(2015, 4, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.High, 40, 2015, 4, new DateTime(2015, 4, 1));
            //// May
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Low, 10, 2015, 5, new DateTime(2015, 5, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.Medium, 20, 2015, 5, new DateTime(2015, 5, 1));
            //await cpRepo.SetCustMonthsPortfolio(custId, RiskToleranceEnum.High, 70, 2015, 5, new DateTime(2015, 5, 1));
            // Jun
            await cpRepo.SetCustMonthsPortfolioMix(custId, RiskToleranceEnum.Medium, 100, 2015, 6, new DateTime(2015, 6, 1));
        }

        [TestMethod]
        public static async Task CreateTestPlayerLossTransactions(int custId) {
            var gzTrx = new GzTransactionRepo();

            // Add playing losses for first 6 months of 2015
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 160, creditPcnt: 50, createdOnUTC: new DateTime(2015, 1, 1));
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 120, creditPcnt: 50, createdOnUTC: new DateTime(2015, 2, 1));
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 200, creditPcnt: 50, createdOnUTC: new DateTime(2015, 3, 1));
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 170, creditPcnt: 50, createdOnUTC: new DateTime(2015, 4, 1));
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 300, creditPcnt: 50, createdOnUTC: new DateTime(2015, 5, 1));
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 200, creditPcnt: 50, createdOnUTC: new DateTime(2015, 6, 1));
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 200, creditPcnt: 50, createdOnUTC: new DateTime(2016, 1, 1));
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 100, creditPcnt: 50, createdOnUTC: new DateTime(2015, 9, 30));
        }


        [TestMethod]
        public static async Task CreateTestPlayerDepositWidthdrawnTransactions(int custId) {
            var gzTrx = new GzTransactionRepo();

            // Add deposit, withdrawals
            await gzTrx.AddGzTransaction(customerId: custId, gzTransactionType:TransferTypeEnum.Deposit, amount:100, createdOnUTC: new DateTime(2015, 7, 15));
            await gzTrx.AddTransferToGamingAmount(custId, 100, new DateTime(2015, 7, 31));
            await gzTrx.AddInvWithdrawalAmount(custId, 50, new DateTime(2015, 8, 30));
            await gzTrx.AddTransferToGamingAmount(custId, 50, new DateTime(2015, 8, 31));
            await gzTrx.AddInvWithdrawalAmount(custId, 50, new DateTime(2015, 9, 1));
            await gzTrx.AddTransferToGamingAmount(custId, 50, new DateTime(2015, 9, 15));
        }

        [TestMethod]
        public static int CreateTestCustomer() {

            Random rnd = new Random();
            int rndPlatformId = rnd.Next(1, int.MaxValue);

            var newUser = new CustomerViewModel() {
                UserName = "6month@allocation.com",
                Email = "6month@allocation.com",
                Password = "1q2w3e",
                EmailConfirmed = true,
                FirstName = "Six",
                LastName = "Month",
                Birthday = new DateTime(1990, 1, 1),
                PlatformCustomerId = rndPlatformId,
                GamBalance = new decimal(4200.54),
                GamBalanceUpdOnUTC = DateTime.UtcNow
            };

            var custRepo = new CustomerRepo();

            return custRepo.CreateUpdUser(newUser);
        }
    }
}

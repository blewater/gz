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
using gzDAL.ModelUtil;

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
            using (var db = new ApplicationDbContext(null)) {
                var portfolioLines = new PortfolioRepository(db).GetPortfolioRetLines();

                foreach (var l in portfolioLines) {
                    Console.WriteLine(l);
                }
            }
        }
        [TestMethod]
        public void SaveDailyCurrenciesRates() {
            var currencyRateRepo = new CurrencyRateRepo(new ApplicationDbContext(null));
            var quotes = currencyRateRepo.SaveDbDailyCurrenciesRates();

            Assert.IsNotNull(quotes);

            //Assert we have a closing price for first symbol
            Assert.IsTrue(quotes[0].TradeDateTime.HasValue);
        }
        [TestMethod]
        public void SaveDailyFundClosingPrice() {
            using (var db = new ApplicationDbContext(null)) {
                var fundRepo = new FundRepo(db);
                var quotes = fundRepo.SaveDbDailyFundClosingPrices();

                Assert.IsNotNull(quotes);

                //Assert we have a closing price for first symbol
                Assert.IsTrue(quotes[0].LastTradePrice.HasValue);
            }
        }

        [TestMethod]
        public void SaveDbUpdAllCustomerBalances() {
            using (var db = new ApplicationDbContext(null)) {
                new InvBalanceRepo(db, new CustFundShareRepo(db), new GzTransactionRepo(db))
                    .SaveDbAllCustomersMonthlyBalances();
            }
        }

    }
}

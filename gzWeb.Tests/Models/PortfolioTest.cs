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
        public void SaveDailyFundClosingPrice() {
            var fundRepo = new FundRepo();
            fundRepo.AddDailyFundClosingPrices();
        }
        [TestMethod]
        public async Task CalculateReturns() {

            int custId = CreateUpd6MonthAllocationCustomer();

            var gzTrx = new GzTransactionRepo();

            // First 6 months of 2015
            await gzTrx.AddPlayingLoss(customerId:custId,totPlayinLossAmount:160, creditPcnt:50, createdOnUTC:new DateTime(2015, 1, 1));
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 120, creditPcnt: 50, createdOnUTC: new DateTime(2015, 2, 1));
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 200, creditPcnt: 50, createdOnUTC: new DateTime(2015, 3, 1));
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 170, creditPcnt: 50, createdOnUTC: new DateTime(2015, 4, 1));
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 300, creditPcnt: 50, createdOnUTC: new DateTime(2015, 5, 1));
            await gzTrx.AddPlayingLoss(customerId: custId, totPlayinLossAmount: 200, creditPcnt: 50, createdOnUTC: new DateTime(2015, 6, 1));

            var cpRepo = new CustPortfolioRepo();
            // Jan
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.Low, 30, new DateTime(2015, 1, 1));
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.Medium, 50, new DateTime(2015, 2, 1));
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.High, 20, new DateTime(2015, 3, 1));
            // Feb
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.Low, 10, new DateTime(2015, 1, 1));
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.Medium, 50, new DateTime(2015, 2, 1));
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.High, 40, new DateTime(2015, 3, 1));
            // Mar
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.High, 100, new DateTime(2015, 3, 1));
            // Apr
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.Low, 30, new DateTime(2015, 1, 1));
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.Medium, 30, new DateTime(2015, 2, 1));
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.High, 40, new DateTime(2015, 3, 1));
            // May
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.Low, 10, new DateTime(2015, 1, 1));
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.Medium, 20, new DateTime(2015, 2, 1));
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.High, 70, new DateTime(2015, 3, 1));
            // Jun
            await cpRepo.SetMonthCustPortfolio(custId, RiskToleranceEnum.Medium, 100, new DateTime(2015, 2, 1));

            //            var creditedPlayingLoss =

            //db.GzTransactions.AddOrUpdate(
            //    t => new { t.CustomerId, t.CreatedOnUTC, t.TypeId },
            //    //March
            //    new GzTransaction {
            //        CustomerId = custId,
            //        YearMonthCtd = "201503",
            //        CreatedOnUTC = new DateTime(2015, 3, 4, 7, 23, 42),
            //        Amount = new decimal(10000),
            //        TypeId = context.gzTransactionTypes.Where(t => t.Code == TransferTypeEnum.Deposit).Select(t => t.Id).FirstOrDefault(),
            //    },
            //    new GzTransaction {
            //        CustomerId = custId,
            //        YearMonthCtd = "201503",
            //        CreatedOnUTC = new DateTime(2015, 3, 18, 18, 22, 13),
            //        Amount = new decimal(9000),
            //        TypeId = context.gzTransactionTypes.Where(t => t.Code == TransferTypeEnum.Deposit).Select(t => t.Id).FirstOrDefault(),
            //    },
            //    new GzTransaction {
            //        CustomerId = custId,
            //        YearMonthCtd = "201503",
            //        CreatedOnUTC = new DateTime(2015, 3, 31, 23, 46, 01),
            //        Amount = new decimal(9853),
            //        TypeId = context.gzTransactionTypes.Where(t => t.Code == TransferTypeEnum.PlayingLoss).Select(t => t.Id).FirstOrDefault(),
            //    },
            //    new GzTransaction {
            //        CustomerId = custId,
            //        YearMonthCtd = "201503",
            //        CreatedOnUTC = new DateTime(2015, 3, 31, 23, 46, 02, 853),
            //        Amount = new decimal(4926.5),
            //        TypeId = context.gzTransactionTypes.Where(t => t.Code == TransferTypeEnum.CreditedPlayingLoss).Select(t => t.Id).FirstOrDefault(),
            //    },


        }

        private static int CreateUpd6MonthAllocationCustomer() {

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

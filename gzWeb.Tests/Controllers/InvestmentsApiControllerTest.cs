using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using AutoMapper;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.Repos;
using gzDAL.Repos.Interfaces;
using gzWeb.Contracts;
using gzWeb.Controllers;
using gzWeb.Models;
using Microsoft.AspNet.Identity;
using NUnit.Framework;

namespace gzWeb.Tests.Controllers
{
    [TestFixture]
    public class InvestmentsApiControllerTest
    {
        protected const string UnitTestDb = "gzTestDb";

        [OneTimeSetUp]
        public void Setup() {
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        [Test]
        public void InvestmentSummaryDataUserNull()
        {

            var result = GetSummaryData();

            // Assert
            Assert.IsNotNull(result);
        }


        [Test]
        public void GetSummaryDataWithUser()
        {
            InvestmentsApiController investmentsApiController;
            var db = CreateInvestmentsApiController(out investmentsApiController);

            var manager = new ApplicationUserManager(new CustomUserStore(db),
                                                     new DataProtectionProviderFactory(() => null));
            //var user = manager.FindByEmail("6month@allocation.com");
            var user = manager.FindByEmail("u9@nessos.gr");

            // Act
            var result = ((IInvestmentsApi) investmentsApiController).GetSummaryData(user);
            Assert.IsNotNull(result);

            // Is this formula correct?
            // var gainLossDiff = result.TotalInvestmentsReturns - (result.InvestmentsBalance - result.TotalInvestments);
            // Assert.IsTrue(gainLossDiff == 0);
        }

        [Test]
        public void GetVintagesSellingValues() {
            InvestmentsApiController investmentsApiController;
            var db = CreateInvestmentsApiController(out investmentsApiController);

            var manager = new ApplicationUserManager(new CustomUserStore(db),
                                                     new DataProtectionProviderFactory(() => null));
            var user = manager.FindByEmail("6month@allocation.com");

            // Act
            
            var vintages = investmentsApiController.GetVintagesSellingValuesByUser(user);
            foreach (var vintageViewModel in vintages) {
                Console.WriteLine("{0} Investment: {1}, SellingValue: {2}, Sold: {3}, Locked: {4}", 
                    vintageViewModel.YearMonthStr, 
                    vintageViewModel.InvestAmount, 
                    vintageViewModel.SellingValue,
                    vintageViewModel.Sold,
                    vintageViewModel.Locked);
                Assert.IsNotNull(vintageViewModel.SellingValue);
            }
        }

        private static IHttpActionResult GetSummaryData()
        {

            InvestmentsApiController controller;
            var db = CreateInvestmentsApiController(out controller);

            // Act
            IHttpActionResult result = controller.GetSummaryData();
            return result;
        }

        private static ApplicationDbContext CreateInvestmentsApiController(out InvestmentsApiController controller)
        {

            ApplicationDbContext db = new ApplicationDbContext();
            ICustFundShareRepo custFundShareRepo = new CustFundShareRepo(db);
            IGzTransactionRepo gzTransactionRepo = new GzTransactionRepo(db);
            IInvBalanceRepo invBalanceRepo = new InvBalanceRepo(db, custFundShareRepo, gzTransactionRepo);
            ICurrencyRateRepo currencyRateRepo = new CurrencyRateRepo(db);

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<VintageDto, VintageViewModel>();
            });
            var mapper = config.CreateMapper();

            // Arrange
            controller = new InvestmentsApiController(
                    db,
                    invBalanceRepo,
                    gzTransactionRepo,
                    custFundShareRepo,
                    currencyRateRepo,
                    mapper,
                    new ApplicationUserManager(new CustomUserStore(db), new DataProtectionProviderFactory(() => null)));
            return db;
        }
    }
}

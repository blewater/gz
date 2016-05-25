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

        private InvestmentsApiController investmentsApiController;
        private ApplicationDbContext db;
        private ApplicationUserManager manager;
        private IMapper mapper;

        [OneTimeSetUp]
        public void Setup() {
            Database.SetInitializer<ApplicationDbContext>(null);

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<VintageDto, VintageViewModel>().ReverseMap();
            });
            mapper = config.CreateMapper();

            db = CreateInvestmentsApiController(out investmentsApiController);

            manager = new ApplicationUserManager(new CustomUserStore(db),
                                                     new DataProtectionProviderFactory(() => null));
        }

        [Test]
        public void InvestmentSummaryDataUserNull()
        {
            var result = GetSummaryData();

            // Assert
            Assert.IsNotNull(result);
        }

        private IHttpActionResult GetSummaryData() {
            // Act
            IHttpActionResult result = investmentsApiController.GetSummaryData();
            return result;
        }

        /// <summary>
        /// 
        /// Precondition is u9@nessos.gr has to exist as user.
        /// 
        /// </summary>
        [Test]
        public void GetSummaryDataWithNewCustomer() {

            var user = manager.FindByEmail("u9@nessos.gr");

            // Act
            var result = ((IInvestmentsApi)investmentsApiController).GetSummaryData(user);
            Assert.IsNotNull(result);
        }

        [Test]
        public void SaveDbVintages() {

            var user = manager.FindByEmail("6month@allocation.com");

            var vintagesVms = investmentsApiController.GetVintagesSellingValuesByUser(user).ToList();
            var vintages = 
                vintagesVms.Select(v =>mapper.Map<VintageViewModel, VintageDto>(v)).ToList();

            // Mark for selling most recent that's allowed
            vintages.Where(v => !v.Locked && !v.Sold)
                .OrderByDescending(v => v.YearMonthStr)
                .First()
                .Selected = true;

            investmentsApiController.SaveDbSellVintages(user.Id, vintages);

            // Mark for selling earliest and latest available
            vintages.Where(v => !v.Locked && !v.Sold)
                .OrderBy(v => v.YearMonthStr)
                .First()
                .Selected = true;
            vintages.Where(v => !v.Locked && !v.Sold)
                .OrderByDescending(v => v.YearMonthStr)
                .First()
                .Selected = true;

            investmentsApiController.SaveDbSellVintages(user.Id, vintages);
        }

        [Test]
        public void GetSummaryDataWithUser()
        {
            var user = manager.FindByEmail("6month@allocation.com");

            // Act
            var result = ((IInvestmentsApi) investmentsApiController).GetSummaryData(user);
            Assert.IsNotNull(result);

            // Is this formula correct?
            // var gainLossDiff = result.TotalInvestmentsReturns - (result.InvestmentsBalance - result.TotalInvestments);
            // Assert.IsTrue(gainLossDiff == 0);
        }

        [Test]
        public void GetVintagesSellingValues() {

            var user = manager.FindByEmail("6month@allocation.com");

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

        private ApplicationDbContext CreateInvestmentsApiController(out InvestmentsApiController controller)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            ICustPortfolioRepo custPortfolioRepo = new CustPortfolioRepo(db);
            ICustFundShareRepo custFundShareRepo = new CustFundShareRepo(db, custPortfolioRepo);
            IGzTransactionRepo gzTransactionRepo = new GzTransactionRepo(db);
            IInvBalanceRepo invBalanceRepo = new InvBalanceRepo(db, custFundShareRepo, gzTransactionRepo);
            ICurrencyRateRepo currencyRateRepo = new CurrencyRateRepo(db);

            // Arrange
            controller = new InvestmentsApiController(
                    db,
                    invBalanceRepo,
                    gzTransactionRepo,
                    custFundShareRepo,
                    currencyRateRepo,
                    custPortfolioRepo,
                    mapper,
                    new ApplicationUserManager(new CustomUserStore(db), new DataProtectionProviderFactory(() => null)));
            return db;
        }
    }
}

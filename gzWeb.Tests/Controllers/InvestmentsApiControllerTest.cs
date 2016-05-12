using System.Data.Entity;
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
            InvestmentsApiController controller;
            var db = CreateInvestmentsApiController(out controller);

            var manager = new ApplicationUserManager(new CustomUserStore(db),
                                                     new DataProtectionProviderFactory(() => null));
            var user = manager.FindByEmail("info@nessos.gr");

            // Act
            var result = ((IInvestmentsApi) controller).GetSummaryData(user);
            Assert.IsNotNull(result);

            var gainLossDiff = result.TotalInvestmentsReturns - (result.InvestmentsBalance - result.TotalInvestments);
            Assert.IsTrue(gainLossDiff == 0);
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

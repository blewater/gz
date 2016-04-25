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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gzWeb.Tests.Controllers {
    [TestClass]
    public class InvestmentsApiControllerTest {
        [TestMethod]
        public void InvestmentSummaryDataUserNull() {

            var result = GetSummaryData();

            // Assert
            Assert.IsNotNull(result);
        }
        

        [TestMethod]
        public void GetSummaryDataWithUser() {

            InvestmentsApiController controller;
            var db = CreateInvestmentsApiController(out controller);

            var manager = new ApplicationUserManager(new CustomUserStore(db), null);
            var user = manager.FindByEmail("gz@gz.com");

            // Act
            var result = ((IInvestmentsApi)controller).GetSummaryData(user);
            Assert.IsNotNull(result);
        }

        private static IHttpActionResult GetSummaryData() {

            InvestmentsApiController controller;
            var db = CreateInvestmentsApiController(out controller);

            // Act
            IHttpActionResult result = controller.GetSummaryData();
            return result;
        }

        private static ApplicationDbContext CreateInvestmentsApiController(out InvestmentsApiController controller) {

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
                new ApplicationUserManager(new CustomUserStore(db), null));
            return db;
        }
    }
}

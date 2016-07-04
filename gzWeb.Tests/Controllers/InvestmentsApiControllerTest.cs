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
        private UserRepo _userRepo;
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
            var trxRepo = new GzTransactionRepo(db);
            _userRepo = new UserRepo(
                db
                , trxRepo,
                new InvBalanceRepo(db, new CustFundShareRepo(db, new CustPortfolioRepo(db)), trxRepo));
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
        public void InvestementApiController_GetCustomerPlans() {

            var user = manager.FindByEmail("info@nessos.gr");

            // Act
            var result = investmentsApiController.GetCustomerPlans(user.Id);

            // 3 Active Portfolios
            Assert.IsNotNull(result.Count() == 3);
        }

        [Test]
        public void GetCustomerPlans() {
            var user = manager.FindByEmail("info@nessos.gr");

            var vintagesVMs = investmentsApiController.GetVintagesSellingValuesByUser(user).ToList();
        }

        [Test]
        public void SaveDbVintages() {

            var user = manager.FindByEmail("info@nessos.gr");

            var vintagesVMs = investmentsApiController.GetVintagesSellingValuesByUser(user).ToList();

            // Mark for selling earliest that's allowed
            var sellVintage = vintagesVMs.Where(v => !v.Locked && !v.Sold)
                .OrderBy(v => v.YearMonthStr)
                .First();

            sellVintage.Selected = true;

            ICollection<VintageDto> vintagesDto = vintagesVMs.Select(v => mapper.Map<VintageViewModel, VintageDto>(v))
                .ToList();
            vintagesDto = investmentsApiController.SaveDbSellVintages(user.Id, vintagesDto);

            // Mark for selling earliest and latest available
            var earliestVin = vintagesDto.Where(v => !v.Locked && !v.Sold)
                .OrderBy(v => v.YearMonthStr)
                .First();
            earliestVin.Selected = true;
            var latestVin = vintagesDto.Where(v => !v.Locked && !v.Sold)
                .OrderByDescending(v => v.YearMonthStr)
                .First();
            latestVin.Selected = true;

            investmentsApiController.SaveDbSellVintages(user.Id, vintagesDto);
        }

        [Test]
        public void GetSummaryDataWithUser() {

            var userId = db.Users
                .Where(u => u.Email == "info@nessos.gr")
                .Select(u => u.Id).Single();
            ApplicationUser user;
            var summaryDto = _userRepo.GetSummaryData(userId, out user);

            // Act
            var result = ((IInvestmentsApi) investmentsApiController).GetSummaryData(user, summaryDto);
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

        [Test]
        public void GetPortfoliosAllocationValues() {

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

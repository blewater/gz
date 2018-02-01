using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
    public class InvestmentsApiControllerTest : IDisposable
    {
        protected const string UnitTestDb = "gzTestDb";

        private InvestmentsApiController investmentsApiController;
        private ApplicationDbContext db;
        private ApplicationUserManager manager;
        private UserRepo userRepo;
        private IMapper mapper;
        private InvBalanceRepo invBalanceRepo;
        private UserPortfolioRepo custPortfolioRepo;
        private GzTransactionRepo gzTransactionRepo;

        [OneTimeSetUp]
        public void Setup() {
            Database.SetInitializer<ApplicationDbContext>(null);

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<VintageDto, VintageViewModel>().ReverseMap();
            });
            mapper = config.CreateMapper();

            db = new ApplicationDbContext();
            manager = new ApplicationUserManager(new CustomUserStore(db),
                                                     new DataProtectionProviderFactory(() => null));
            var confRepo = new ConfRepo(db);
            gzTransactionRepo = new GzTransactionRepo(db, confRepo);
            userRepo = new UserRepo(db);
            custPortfolioRepo = new UserPortfolioRepo(db, confRepo, userRepo);
            invBalanceRepo = new InvBalanceRepo(
                db, 
                new UserPortfolioSharesRepo(db), 
                gzTransactionRepo, 
                custPortfolioRepo, 
                confRepo,
                userRepo
            );

            db = CreateInvestmentsApiController(out investmentsApiController);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            manager.Dispose();
            db.Dispose();
        }

        [Test]
        public void InvestmentSummaryDataUserNull()
        {
            var result = GetSummaryData();

            // Assert
            Assert.IsNotNull(result);
        }

        private async Task<IHttpActionResult> GetSummaryData() {
            // Act
            IHttpActionResult result = await investmentsApiController.GetSummaryData();
            return result;
        }

        /// <summary>
        /// 
        /// Precondition is u9@nessos.gr has to exist as user.
        /// 
        /// </summary>
        [Test]
        public async Task InvestementApiController_GetCustomerPlans() {

            var user = await manager.FindByEmailAsync("info@nessos.gr");

            // Act
            var result = await investmentsApiController.GetCustomerPlansAsync(user.Id);

            // 3 Active Portfolios
            Assert.IsNotNull(result.Count() == 3);
        }

        [Test]
        public async Task SaveDbVintages() {

            string queueAzureConnString = ConfigurationManager.AppSettings["QueueAzureConnString"];
            string queueName = ConfigurationManager.AppSettings["QueueName"];
            var user = manager.FindByEmail("info@nessos.gr");

            var vintagesDto = await SellOneVintage(user);

            // Mark for selling earliest and latest available vintages even if sold or locked
            var earliestVin = vintagesDto
                .OrderBy(v => v.YearMonthStr)
                .First();
            earliestVin.Selected = true;
            var latestVin = vintagesDto
                .OrderByDescending(v => v.YearMonthStr)
                .First();
            latestVin.Selected = true;

            invBalanceRepo.SetAllSelectedVintagesPresentMarketValue(user.Id, vintagesDto);

            invBalanceRepo
                .SaveDbSellAllSelectedVintagesInTransRetry(
                    user.Id,
                    vintagesDto,
                    false,
                    queueAzureConnString,
                    queueName,
                    null,
                    null,
                    null
                );
        }

        [Test]
        public async Task TestSellingAVintage() {

            var user = manager.FindByEmail("alaa_el-chami@hotmail.com");
            var vintagesDto = await SellOneVintage(user);
        }

        private async Task<ICollection<VintageDto>> SellOneVintage(ApplicationUser user) {

            string queueAzureConnString = ConfigurationManager.AppSettings["QueueAzureConnString"];
            string queueName = ConfigurationManager.AppSettings["QueueName"];
            var vintagesVMs = await investmentsApiController.GetVintagesSellingValuesByUserTestHelper(user);

            // Mark for selling earliest even if sold already or is locked
            var sellVintage = vintagesVMs
                .OrderBy(v => v.YearMonthStr)
                .First();

            sellVintage.Selected = true;
            sellVintage.Locked = false;
            sellVintage.Sold = true;

            ICollection<VintageDto> vintagesDto = vintagesVMs.Select(v => mapper.Map<VintageViewModel, VintageDto>(v))
                .ToList();

            invBalanceRepo.SetAllSelectedVintagesPresentMarketValue(user.Id, vintagesDto);

            vintagesDto = 
                invBalanceRepo
                    .SaveDbSellAllSelectedVintagesInTransRetry(
                        user.Id,
                        vintagesDto,
                        false,
                        queueAzureConnString,
                        queueName,
                        null,
                        null,
                        null,
                        null
                    );

            return vintagesDto;
        }

        [Test]
        public async Task GetSummaryDataWithUser() {

            var s = Stopwatch.StartNew();
            var userId = db.Users
                .Where(u => u.Email == "testuser@gz.com")
                .Select(u => u.Id).Single();
            var tuple = await invBalanceRepo.GetSummaryDataAsync(userId);
            var user = tuple.Item2;
            var summaryDto = tuple.Item1;

            // Act
            var result = ((IInvestmentsApi) investmentsApiController).GetSummaryData(user, summaryDto);
            Assert.IsNotNull(result);

            // Is this formula correct?
            // var gainLossDiff = result.TotalInvestmentsReturns - (result.InvestmentsBalance - result.TotalInvestments);
            // Assert.IsTrue(gainLossDiff == 0);

            var elapsed = s.Elapsed;
            Console.WriteLine($"Elapsed: {elapsed.Milliseconds.ToString("F")} milliseconds.");
        }

        [Test]
        public async Task GetVintagesSellingValues() {

            var user = manager.FindByEmail("6month@allocation.com");

            var vintages = await investmentsApiController.GetVintagesSellingValuesByUserTestHelper(user);
            foreach (var vintageViewModel in vintages) {
                Console.WriteLine("{0} Investment: {1}, SellingValue: {2}, Sold: {3}, Locked: {4}",
                    vintageViewModel.YearMonthStr,
                    vintageViewModel.InvestmentAmount,
                    vintageViewModel.SellingValue,
                    vintageViewModel.Sold,
                    vintageViewModel.Locked);
                Assert.IsNotNull(vintageViewModel.SellingValue);
            }
        }

        private ApplicationDbContext CreateInvestmentsApiController(out InvestmentsApiController controller) {

            // Arrange
            controller = new InvestmentsApiController(
                    db,
                    invBalanceRepo,
                    gzTransactionRepo,
                    this.custPortfolioRepo,
                    userRepo,
                    mapper,
                    new ApplicationUserManager(
                        new CustomUserStore(db), 
                        new DataProtectionProviderFactory(
                            () => null
                        )
                    ));
            return db;
        }
    }
}

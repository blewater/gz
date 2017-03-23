using System;
using System.Collections.Generic;
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

        private InvestmentsApiController _investmentsApiController;
        private ApplicationDbContext _db;
        private ApplicationUserManager manager;
        private UserRepo _userRepo;
        private IMapper mapper;
        private InvBalanceRepo _invBalanceRepo;
        private CustPortfolioRepo _custPortfolioRepo;
        private GzTransactionRepo _gzTransactionRepo;

        [OneTimeSetUp]
        public void Setup() {
            Database.SetInitializer<ApplicationDbContext>(null);

            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<VintageDto, VintageViewModel>().ReverseMap();
            });
            mapper = config.CreateMapper();

            _db = new ApplicationDbContext();
            manager = new ApplicationUserManager(new CustomUserStore(_db),
                                                     new DataProtectionProviderFactory(() => null));
            var confRepo = new ConfRepo(_db);
            _gzTransactionRepo = new GzTransactionRepo(_db, confRepo);
            _custPortfolioRepo = new CustPortfolioRepo(_db,confRepo);
            _invBalanceRepo = new InvBalanceRepo(_db, 
                new CustFundShareRepo(_db,
                _custPortfolioRepo), 
                _gzTransactionRepo, 
                _custPortfolioRepo, 
                confRepo);
            _userRepo = new UserRepo(
                _db,
                _gzTransactionRepo,
                _invBalanceRepo);

            _db = CreateInvestmentsApiController(out _investmentsApiController);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            manager.Dispose();
            _db.Dispose();
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
            IHttpActionResult result = await _investmentsApiController.GetSummaryData();
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
            var result = await _investmentsApiController.GetCustomerPlansAsync(user.Id);

            // 3 Active Portfolios
            Assert.IsNotNull(result.Count() == 3);
        }

        [Test]
        public async Task SaveDbVintages() {

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

            _invBalanceRepo.SetVintagesPresentMarketValue(user.Id, vintagesDto);

            _investmentsApiController.SaveDbSellVintages(user.Id, vintagesDto, bypassQueue: true);
        }

        [Test]
        public void TestSellingAVintage() {

            var user = manager.FindByEmail("salem8@gmail.com");
            var vintagesDto = SellOneVintage(user);
        }

        private async Task<ICollection<VintageDto>> SellOneVintage(ApplicationUser user) {

            
            var vintagesVMs = await _investmentsApiController.GetVintagesSellingValuesByUserTestHelper(user);

            // Mark for selling earliest even if sold already or is locked
            var sellVintage = vintagesVMs
                .OrderBy(v => v.YearMonthStr)
                .First();

            sellVintage.Selected = true;
            sellVintage.Locked = false;
            sellVintage.Sold = true;

            ICollection<VintageDto> vintagesDto = vintagesVMs.Select(v => mapper.Map<VintageViewModel, VintageDto>(v))
                .ToList();

            _invBalanceRepo.SetVintagesPresentMarketValue(user.Id, vintagesDto);

            vintagesDto = _investmentsApiController.SaveDbSellVintages(
                user.Id, 
                vintagesDto,
                bypassQueue: true);
            return vintagesDto;
        }

        [Test]
        public async Task GetSummaryDataWithUser() {

            var s = Stopwatch.StartNew();
            var userId = _db.Users
                .Where(u => u.Email == "testuser@gz.com")
                .Select(u => u.Id).Single();
            var tuple = await _userRepo.GetSummaryDataAsync(userId);
            var user = tuple.Item2;
            var summaryDto = tuple.Item1;

            // Act
            var result = ((IInvestmentsApi) _investmentsApiController).GetSummaryData(user, summaryDto);
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

            var vintages = await _investmentsApiController.GetVintagesSellingValuesByUserTestHelper(user);
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

        [Test]
        public async Task GetVintagesSellingValue()
        {

            var user = manager.FindByEmail("6month@allocation.com");

            var vintages = await _investmentsApiController.GetVintagesSellingValuesByUserTestHelper(user);
            foreach (var vintageViewModel in vintages)
            {
                Console.WriteLine("{0} Investment: {1}, SellingValue: {2}, Sold: {3}, Locked: {4}",
                    vintageViewModel.YearMonthStr,
                    vintageViewModel.InvestmentAmount,
                    vintageViewModel.SellingValue,
                    vintageViewModel.Sold,
                    vintageViewModel.Locked);
                Assert.IsNotNull(vintageViewModel.SellingValue);
            }
        }

        private ApplicationDbContext CreateInvestmentsApiController(out InvestmentsApiController controller)
        {
            ICustPortfolioRepo custPortfolioRepo = new CustPortfolioRepo(_db, new ConfRepo(_db));
            ICustFundShareRepo custFundShareRepo = new CustFundShareRepo(_db, custPortfolioRepo);
            ICurrencyRateRepo currencyRateRepo = new CurrencyRateRepo(_db);

            // Arrange
            controller = new InvestmentsApiController(
                    _db,
                    _invBalanceRepo,
                    _gzTransactionRepo,
                    _custPortfolioRepo,
                    _userRepo,
                    mapper,
                    new ApplicationUserManager(new CustomUserStore(_db), new DataProtectionProviderFactory(() => null)));
            return _db;
        }
    }
}

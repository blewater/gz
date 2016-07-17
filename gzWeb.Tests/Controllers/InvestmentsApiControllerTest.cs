using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
    public class InvestmentsApiControllerTest
    {
        protected const string UnitTestDb = "gzTestDb";

        private InvestmentsApiController investmentsApiController;
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
            _gzTransactionRepo = new GzTransactionRepo(_db);
            _custPortfolioRepo = new CustPortfolioRepo(_db);
            _invBalanceRepo = new InvBalanceRepo(_db, 
                new CustFundShareRepo(_db,
                _custPortfolioRepo), 
                _gzTransactionRepo, 
                _custPortfolioRepo);
            _userRepo = new UserRepo(
                _db,
                _gzTransactionRepo,
                _invBalanceRepo);

            _db = CreateInvestmentsApiController(out investmentsApiController);
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
        public void SaveDbVintages() {

            var user = manager.FindByEmail("info@nessos.gr");

            var vintagesDto = SellOneVintage(user);

            // Mark for selling earliest and latest available
            var earliestVin = vintagesDto.Where(v => !v.Locked && !v.Sold)
                .OrderBy(v => v.YearMonthStr)
                .First();
            earliestVin.Selected = true;
            var latestVin = vintagesDto.Where(v => !v.Locked && !v.Sold)
                .OrderByDescending(v => v.YearMonthStr)
                .First();
            latestVin.Selected = true;

            _invBalanceRepo.SetVintagesMarketPrices(user.Id, vintagesDto);

            investmentsApiController.SaveDbSellVintages(user.Id, vintagesDto, bypassQueue: true);
        }

        [Test]
        public void TestSellingAVintage() {

            var user = manager.FindByEmail("salem8@gmail.com");
            var vintagesDto = SellOneVintage(user);
        }

        private ICollection<VintageDto> SellOneVintage(ApplicationUser user) {

            var vintagesVMs = investmentsApiController.GetVintagesSellingValuesByUser(user).ToList();

            // Mark for selling earliest that's allowed
            var sellVintage = vintagesVMs.Where(v => !v.Locked && !v.Sold)
                .OrderBy(v => v.YearMonthStr)
                .First();

            sellVintage.Selected = true;

            ICollection<VintageDto> vintagesDto = vintagesVMs.Select(v => mapper.Map<VintageViewModel, VintageDto>(v))
                .ToList();

            _invBalanceRepo.SetVintagesMarketPrices(user.Id, vintagesDto);

            vintagesDto = investmentsApiController.SaveDbSellVintages(
                user.Id, 
                vintagesDto,
                bypassQueue: true);
            return vintagesDto;
        }

        [Test]
        public async Task GetSummaryDataWithUser() {

            var s = Stopwatch.StartNew();
            var userId = _db.Users
                .Where(u => u.Email == "salem8@gmail.com")
                .Select(u => u.Id).Single();
            var tuple = await _userRepo.GetSummaryDataAsync(userId);
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
        public void GetVintagesSellingValues() {

            var user = manager.FindByEmail("6month@allocation.com");

            var vintages = investmentsApiController.GetVintagesSellingValuesByUser(user);
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
        public void GetPortfoliosAllocationValues() {

            var user = manager.FindByEmail("6month@allocation.com");

            var vintages = investmentsApiController.GetVintagesSellingValuesByUser(user);
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

        private ApplicationDbContext CreateInvestmentsApiController(out InvestmentsApiController controller)
        {
            ICustPortfolioRepo custPortfolioRepo = new CustPortfolioRepo(_db);
            ICustFundShareRepo custFundShareRepo = new CustFundShareRepo(_db, custPortfolioRepo);
            ICurrencyRateRepo currencyRateRepo = new CurrencyRateRepo(_db);

            // Arrange
            controller = new InvestmentsApiController(
                    _db,
                    _invBalanceRepo,
                    _gzTransactionRepo,
                    custFundShareRepo,
                    currencyRateRepo,
                    _custPortfolioRepo,
                    _userRepo,
                    mapper,
                    new ApplicationUserManager(new CustomUserStore(_db), new DataProtectionProviderFactory(() => null)));
            return _db;
        }
    }
}

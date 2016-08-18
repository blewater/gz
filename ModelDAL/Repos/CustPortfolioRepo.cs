using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using gzDAL.Conf;
using gzDAL.DTO;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using gzDAL.Models;
using NLog;
using Z.EntityFramework.Plus;

namespace gzDAL.Repos
{
    public class CustPortfolioRepo : ICustPortfolioRepo
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationDbContext db;
        public CustPortfolioRepo(ApplicationDbContext db)
        {
            this.db = db;
        }
        /// <summary>
        /// Phase 1 Implementation
        /// Save the month's full portfolio mix for a customer.
        /// For phase I you can only select 1 portfolio in 100%
        /// V2 will accept a collection of portfolios for example, 30% for Low, 50% Medium, 20% High
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="riskType"></param>
        /// <param name="portfYear">i.e. 2015 the year this portfolio weight applies</param>
        /// <param name="portfMonth">1..12 the month this portfolio weight applies</param>
        /// <param name="UpdatedOnUTC"></param>
        /// <returns></returns>
        public void SaveDbCustMonthsPortfolioMix(int customerId, RiskToleranceEnum riskType, int portfYear, int portfMonth, DateTime UpdatedOnUTC) {

            if (customerId <= 0) {

                _logger.Error("SaveDbCustMonthsPortfolioMix(): Invalid Customer Id: {0}", customerId);
            }

            this.SaveDbCustMonthsPortfolioMix(customerId, riskType, 100, portfYear, portfMonth, UpdatedOnUTC);
        }

        /// <summary>
        /// 
        /// UI Simplified Wrapper for setting the portfolio for the following month from UtcNow.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="riskType"></param>
        public void SaveDbCustomerSelectNextMonthsPortfolio(int customerId, RiskToleranceEnum riskType) {

            var nextMonth = DateTime.UtcNow.AddMonths(1);

            SaveDbCustMonthsPortfolioMix(customerId, riskType, nextMonth.Year, nextMonth.Month, DateTime.UtcNow);

        }

        /// <summary>
        /// Phase 2 Implementation allowing multiple portfolios linked to a *single* customer with a weight mix.
        /// Note Does not impose present month restriciton. Can set a portfolio mix for the past.
        /// Used internally by Phase 1 method for 100% single customer portfolio selection and for Seeding outside of the current month.
        /// Use overloaded SetCustMonthsPortfolioMix instead for phase I
        /// Though it's use is internal it's declared Public to unit test it.
        /// Save the month's portfolio mix for a customer.
        /// For example, 30% for Low, 50% Medium, 20% High
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="riskType"></param>
        /// <param name="weight">Percentage weight for example 33.3 for 1/3 of the balance</param>
        /// <param name="portfYear">i.e. 2015 the year this portfolio weight applies</param>
        /// <param name="portfMonth">1..12 the month this portfolio weight applies</param>
        /// <param name="UpdatedOnUTC"></param>
        /// <returns></returns>
        public void SaveDbCustMonthsPortfolioMix(int customerId, RiskToleranceEnum riskType, float weight, int portfYear, int portfMonth, DateTime UpdatedOnUTC) {

            if (weight < 0 || weight > 100)
                throw new Exception("Invalid percentage not within range 0..100: " + weight);

            SaveDbCustMonthsPortfolioMixImpl(customerId, riskType, weight, portfYear, portfMonth, UpdatedOnUTC);
            db.SaveChanges();
        }

        private void SaveDbCustMonthsPortfolioMixImpl(int customerId, RiskToleranceEnum riskType, float weight,
                                                      int portfYear, int portfMonth, DateTime updatedOnUtc)
        {
            db.CustPortfolios.AddOrUpdate(
                    cp => new { cp.CustomerId, cp.YearMonth },
                        new CustPortfolio
                        {
                            CustomerId = customerId,
                            // Get the most recent active portfolio for the risk type
                            PortfolioId = db.Portfolios
                                .Where(p => p.RiskTolerance == riskType && p.IsActive)
                                .Select(p => p.Id)
                                .Single(),
                            YearMonth = DbExpressions.GetStrYearMonth(portfYear, portfMonth),
                            UpdatedOnUTC = updatedOnUtc,
                            Weight = weight
                        }
                    );
        }

        /// <summary>
        /// 
        /// Get the present in Utc customer selected portfolio.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public Portfolio GetCurrentCustomerPortfolio(int customerId) {

            return GetCustomerPortfolioForMonth(customerId, DateTime.UtcNow.ToStringYearMonth());
        }

        /// <summary>
        /// 
        /// Get the next month's portfolio in Utc customer selected portfolio.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public Portfolio GetNextMonthsCustomerPortfolio(int customerId) {

            return GetCustomerPortfolioForMonth(customerId, DateTime.UtcNow.AddMonths(1).ToStringYearMonth());
        }

        /// <summary>
        /// 
        /// Get the customer selected portfolio for any given month in their history.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearMonthStr"></param>
        /// <returns></returns>
        public Portfolio GetCustomerPortfolioForMonth(int customerId, string yearMonthStr) {

            Portfolio customerMonthPortfolioReturn;

            var portfolioMonth = GetCustPortfYearMonth(customerId, yearMonthStr);

            if (portfolioMonth != null) {

                customerMonthPortfolioReturn = db.CustPortfolios
                    .Where(p => p.YearMonth == portfolioMonth && p.CustomerId == customerId)
                    .Select(cp => cp.Portfolio)
                    .DeferredSingleOrDefault()
                    .FromCacheAsync(DateTime.UtcNow.AddHours(2))
                    .Result;
            }
            /**
             * Edge Case for leaky user registrations: 
             * Use default portfolio for new registered users without
             * portfolio in their account. Cpc adds a portfolio next time 
             * it runs.
             */
            else {

                // Cached already
                var defaultRisk = db.GzConfigurations
                    .Select(c => c.FIRST_PORTFOLIO_RISK_VAL)
                    .Single();

                customerMonthPortfolioReturn = db
                    .Portfolios
                    .DeferredSingle(p => p.RiskTolerance == defaultRisk && p.IsActive)
                    .FromCacheAsync(DateTime.UtcNow.AddHours(2))
                    .Result;
            }

            return customerMonthPortfolioReturn;
        }

        public void SaveDefaultPorfolio(int customerId, int gmUserId)
        {
            var user = db.Users.SingleOrDefault(x => x.Id == customerId);
            if (user==null)
                throw new InvalidOperationException("User not found.");

            ConnRetryConf.TransactWithRetryStrategy(
                    db,
                    () =>
                    {
                        if (!user.GmCustomerId.HasValue)
                            user.GmCustomerId = gmUserId;
                        var now = DateTime.UtcNow;
                        SaveDbCustMonthsPortfolioMixImpl(customerId, RiskToleranceEnum.Medium, 100, now.Year, now.Month, now);
                    });
        }

        /// <summary>
        /// 
        /// Get all available portfolios along with customer allocations.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="nextInvestAmount"></param>
        /// <returns></returns>
        public async Task<IEnumerable<PortfolioDto>> GetCustomerPlansAsync(int customerId) {

            var selCustomerPortfolioId = GetNextMonthsCustomerPortfolio(customerId).Id;

            var portfolioDtos = (await (from p in db.Portfolios
                join c in db.CustPortfolios on p.Id equals c.PortfolioId
                join b in db.InvBalances on
                    new {CustomerId = c.CustomerId, YearMonth = c.YearMonth} equals
                    new {CustomerId = b.CustomerId, YearMonth = b.YearMonth}
                where c.CustomerId == customerId
                      && !b.Sold
                group b by p
                into g
                select new PortfolioDto {
                    Id = g.Key.Id,
                    Title = g.Key.Title,
                    Color = g.Key.Color,
                    ROI = g.Key.PortFunds.Select(f => f.Weight*f.Fund.YearToDate/100).Sum(),
                    Risk = (RiskToleranceEnum) g.Key.RiskTolerance,
                    AllocatedAmount = g.Sum(b => b.CashInvestment),
                    Holdings = g.Key.PortFunds.Select(f => new HoldingDto {
                        Name = f.Fund.HoldingName,
                        Weight = f.Weight
                    })
                })

                // Cache 1 Day
                .FromCacheAsync(DateTime.UtcNow.AddDays(1)))
                .AsEnumerable()
                /*** Union with non-allocated customer portfolio ****/
                .Union(
                    await (from p in db.Portfolios
                        where p.IsActive
                        select new PortfolioDto {
                            Id = p.Id,
                            Title = p.Title,
                            Color = p.Color,
                            ROI = p.PortFunds.Select(f => f.Weight*f.Fund.YearToDate/100).Sum(),
                            Risk = (RiskToleranceEnum) p.RiskTolerance,
                            AllocatedAmount = 0M,
                            Holdings = p.PortFunds.Select(f => new HoldingDto {
                                Name = f.Fund.HoldingName,
                                Weight = f.Weight
                            })
                        })

                        // Cache 1 day
                        .FromCacheAsync(DateTime.UtcNow.AddDays(1))
                        , new PortfolioComparer())
                .ToList();

            // Calculate allocation percentage
            var totalSum = portfolioDtos.Sum(p => p.AllocatedAmount);
            foreach (var portfolioDto in portfolioDtos) {
                if (totalSum != 0) {
                    portfolioDto.AllocatedPercent = (float) (100*portfolioDto.AllocatedAmount/totalSum);
                }
                portfolioDto.Selected = portfolioDto.Id == selCustomerPortfolioId;
            }

            return portfolioDtos;
        }

        /// <summary>
        /// 
        /// Get the latest YearMonth of a saved customer portfolio selection
        /// By business rules there will be a globally default portfolio for all customers
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearMonthStr">The year month YYYYMM i.e. April 2016 = 201604 for which you want to get the portfolio </param>
        /// 
        /// <returns></returns>
        private string GetCustPortfYearMonth(int customerId, string yearMonthStr) {

            return db.CustPortfolios
                .Where(
                    p =>
                        p.CustomerId == customerId &&
                        string.Compare(p.YearMonth, yearMonthStr, StringComparison.Ordinal) <= 0)
                .Max(p => p.YearMonth);
        }
    }
    /// <summary>
    /// 
    /// Union comparer for the GetCustomerP
    /// 
    /// </summary>
    class PortfolioComparer : IEqualityComparer<PortfolioDto> {
        public bool Equals(PortfolioDto p1, PortfolioDto p2) {
            return p1.Id == p2.Id;
        }

        public int GetHashCode(PortfolioDto p) {
            return p.Id.GetHashCode();
        }
    }

}
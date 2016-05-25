using System;
using System.Collections.Generic;
using System.Linq;
using gzDAL.DTO;
using gzDAL.Models;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;

namespace gzDAL.Repos
{

    /// <summary>
    /// Used in unit testing
    /// </summary>
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly ApplicationDbContext db;
        public PortfolioRepository(ApplicationDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// 
        /// Calculate the year to date rate for all portfolios.
        /// 
        /// Note this method returns inactive portfolios.
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PortfolioReturnsDTO> GetPortfolioReturns() {

            var portfolioRates = new List<PortfolioReturnsDTO>();

            foreach (var p in db.Portfolios.OrderBy(p=>p.RiskTolerance).ThenByDescending(p=>p.IsActive).ToList()) {

                var r = p.PortFunds.Select(f => f.Weight * f.Fund.YearToDate / 100).Sum();
                portfolioRates.Add(new PortfolioReturnsDTO() {

                    PortfolioId = p.Id,
                    RoI = r,
                    RiskEnumValue = p.RiskTolerance,
                    IsActive = p.IsActive
                    
                });
            }

            return portfolioRates;
        }

        public IList<string> GetPortfolioRetLines() {

            List<string> portfolioRetLine = new List<string>();

            var portfoliosRates = GetPortfolioReturns();

            foreach (var p in portfoliosRates) {

                portfolioRetLine.Add("Portfolio Id: " + p.PortfolioId + ", Risk: " + p.RiskEnumValue.ToString() + ", Is active: " + p.IsActive + ", portfolio RoI: " + p.RoI + "%");
            }

            return portfolioRetLine;
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
        private Portfolio GetCustomerPortfolioForMonth(int customerId, string yearMonthStr) {

            var portfolioMonth = GetCustPortfYearMonth(customerId, yearMonthStr);

            return
                db.CustPortfolios
                    .Where(p => p.YearMonth == portfolioMonth && p.CustomerId == customerId)
                    .Select(cp => cp.Portfolio)
                    .Single();
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
                .Where(p => p.CustomerId == customerId && string.Compare(p.YearMonth, yearMonthStr, StringComparison.Ordinal) <= 0)
                .OrderByDescending(p => p.YearMonth)
                .Select(p => p.YearMonth)
                .First();
        }

    }
}
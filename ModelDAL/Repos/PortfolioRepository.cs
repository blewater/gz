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
        public List<PortfolioReturnsDTO> GetPortfolioReturns() {

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

//        public 

        public IList<string> GetPortfolioRetLines() {

            List<string> portfolioRetLine = new List<string>();

            var portfoliosRates = GetPortfolioReturns();

            foreach (var p in portfoliosRates) {

                portfolioRetLine.Add("Portfolio Id: " + p.PortfolioId + ", Risk: " + p.RiskEnumValue.ToString() + ", Is active: " + p.IsActive + ", portfolio RoI: " + p.RoI + "%");
            }

            return portfolioRetLine;
        }
    }
}
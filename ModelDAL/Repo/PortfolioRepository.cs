using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using gzWeb.Models;

namespace gzWeb.Repo {

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

        public IList<string> GetPortfolioRetLines() {

            List<string> portfolioRetLine = new List<string>();

            foreach (var p in db.Portfolios.ToList()) {

                var r = p.PortFunds.Select(f => f.Weight * Math.Max(f.Fund.ThreeYrReturnPcnt, f.Fund.FiveYrReturnPcnt)/100).Sum();
                portfolioRetLine.Add(p.RiskTolerance.ToString() + " portfolio return " + r + "%");
            }

            return portfolioRetLine;
        }
    }
}
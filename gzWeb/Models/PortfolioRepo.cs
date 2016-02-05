using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Migrations;

namespace gzWeb.Models {
    public class PortfolioRepo {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        public void CalcPortfoliosDayValues(int year, int month, int day) {

            using (var db = new ApplicationDbContext()) {

                var fundPrices = new FundRepo().GetFundsPrices(db, year, month, day);

                var activePortfolios = db.Portfolios
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.RiskTolerance)
                    .ToList();
                foreach (var p in activePortfolios) {

                    var portfoliosFunds = db.PortFunds
                        .Where(pf => pf.PortfolioId == p.Id)
                        .Select(pf => new { Weight = pf.Weight, Symbol = pf.Fund.Symbol, WghtClosingPrice = pf.Weight * fundPrices[pf.Fund.Symbol], valueDay = new DateTime(year, month, day) })
                        .OrderBy(pf => pf.Symbol)
                        .ToList();
                    foreach (var pFundValue in portfoliosFunds) {
                    }
                }
            }
        }
    }
}
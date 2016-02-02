using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Migrations;

namespace gzWeb.Models {
    public class PortfolioRepo {

        /// <summary>
        /// GetFundsPrices for a selected day by looking at the last trade day before the requested day.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        private Dictionary<string, float> SetFundsPrices(ApplicationDbContext db, int year, int month, int day) {

            Dictionary<string, float> fundClosingPrices = null;

            // Get last trade day before our interested day
            var lastTradeDay = db.FundPrices
                .Where(f => string.Compare(f.YearMonthDay, year.ToString("0000") + month.ToString("00") + day.ToString("00")) <= 0)
                .Max(f => f.YearMonthDay);
                    
            if (lastTradeDay != null) {

                // Get Funds prices
                fundClosingPrices = db.FundPrices.Where(fp => fp.YearMonthDay == lastTradeDay)
                            .Select(p => new { p.Fund.Symbol, p.ClosingPrice })
                            .ToDictionary(p => p.Symbol, p => p.ClosingPrice);
            }

            return fundClosingPrices;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        public void CalcPortfoliosDayValues(int year, int month, int day) {

            using (var db = new ApplicationDbContext()) {

                var fundPrices = SetFundsPrices(db, year, month, day);

                //var activePortfolios = db.Portfolios
                //    .Where(p=>p.IsActive)
                //    .OrderBy(p => p.RiskTolerance)
                //    .ToList();
                //foreach (var p in activePortfolios) {

                //    var portfoliosFunds = db.PortFunds
                //        .Where(pf=>pf.PortfolioId == p.Id)
                //        .Select(pf => new { Weight = pf.Weight, Symbol = pf.Fund.Symbol, WghtClosingPrice = pf.Weight * fundPrices[pf.Fund.Symbol], valueDay=new DateTime(year, month, day) })
                //        .OrderBy(pf => pf.Symbol)
                //        .ToList();
                //    foreach (var pFundValue in portfoliosFunds) {
                //        pFundValue.
                //    }
                //}
            }
        }
    }
}
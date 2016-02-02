using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using gzWeb.Utl;

namespace gzWeb.Models {
    public class CustFundShareRepo {

        public void AddCustomerPurchasedFundShares(int customerId, IEnumerable<PortfolioFundValue> fundsShares, int year, int month, DateTime updatedOnUTC) {

            using (var db = new ApplicationDbContext()) {

                foreach (var fundShares in fundsShares) {

                    db.CustFundShares.AddOrUpdate(
                        f => new { f.CustomerId, f.FundId, f.YearMonth },
                        new CustFundShare {
                            CustomerId = customerId,
                            FundId = fundShares.FundId,
                            NumShares = fundShares.NumberofShares,
                            YearMonth = Expressions.GetStrYearMonth(year,month),
                            UpdatedOnUTC = updatedOnUTC
                        }
                    );
                }

                db.SaveChanges();
            }
        }

        /// <summary>
        /// Return type of CalcCustomerMonthlyFundShares method
        /// </summary>
        public class PortfolioFundValue {
            public int PortfolioId { get; set; }
            public int FundId { get; set; }
            public decimal NumberofShares { get; set; }
            public int Year { get; set; }
            public int Month { get; set; }
            public int TradeDay { get; set; }
        }

        /// <summary>
        /// Calculate the customer portfolio value for a given month.
        /// Phase 1 implements assumes only 1 portfolio in 100%
        /// Implementation done in 2 steps to prepare Phase 2.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cashToInvest"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="updatedOnUTC"></param>
        public List<PortfolioFundValue> CalcCustomerMonthlyFundShares(int customerId, decimal cashToInvest, int year, int month) {

            List<PortfolioFundValue> portfolioFundValues;

            using (var db = new ApplicationDbContext()) {

                // Get last selected portfolio for customer
                var lastMonthPort = db.CustPortfolios
                    .Where(p => p.CustomerId == customerId && string.Compare(p.YearMonth, Expressions.GetStrYearMonth(year, month)) <= 0)
                    .Select(p => p.YearMonth)
                    .Max();

                if ( lastMonthPort == null) {

                    throw new Exception($"No portfolio has been set for customer Id: {customerId}");
                }

                //Get Portfolio Funds rows
                var portFundWeights = db.CustPortfolios
                    .Where(p => p.CustomerId == customerId && p.YearMonth == lastMonthPort)
                    .Include(p => p.Portfolio.PortFunds)
                    .SelectMany(p => p.Portfolio.PortFunds )
                    .AsQueryable();

                portfolioFundValues = new List<PortfolioFundValue>();
                foreach (var pfund in portFundWeights) {
                    var weight = pfund.Weight;
                    var fundId = pfund.FundId;
                    var portfolioId = pfund.PortfolioId;
                    var cashPerFund = (decimal) weight * cashToInvest;
                    //var clPrice = pfund.Fund.FundPrices.Where(fp=>fp.YearMonthDay)
                    var lastTradeDay = db.FundPrices
                        .Where(fp => fp.FundId == fundId
                        && string.Compare(fp.YearMonthDay, new DateTime(year, month, 1).AddMonths(1).AddDays(-1).ToString("yyyyMMdd")) <= 0)
                        .Select(fp => fp.YearMonthDay)
                        .Max();
                    var fundPrice = db.FundPrices
                        .Where(fp => fp.FundId == fundId && fp.YearMonthDay == lastTradeDay)
                        .Single();
                    var numShares = cashPerFund / (decimal) fundPrice.ClosingPrice;

                    portfolioFundValues.Add(
                        new PortfolioFundValue() {
                            PortfolioId = portfolioId,
                            FundId = fundId,
                            NumberofShares = numShares,
                            Year = year,
                            Month = month,
                            TradeDay = int.Parse(lastTradeDay.Substring(5, 2))
                        }
                    );
                }
            }
            return portfolioFundValues;
        }

    }
}
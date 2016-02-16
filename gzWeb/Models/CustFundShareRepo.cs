using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using gzWeb.Utl;

namespace gzWeb.Models {
    public class CustFundShareRepo {

        /// <summary>
        /// Save purchased funds shares to the customer's account.
        /// 
        /// Does *not* call DBcontext.SaveChanges() for easy transaction support
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="fundsShares"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="updatedOnUTC"></param>
        public void SaveDbCustPurchasedFundShares(ApplicationDbContext db, int customerId, IEnumerable<PortfolioFundDTO> fundsShares, int year, int month, DateTime updatedOnUTC) {

            foreach (var fundShares in fundsShares) {

                try {
                    db.CustFundShares.AddOrUpdate(
                        f => new { f.CustomerId, f.FundId, f.YearMonth },
                        new CustFundShare {
                        // Key
                        CustomerId = customerId,
                            FundId = fundShares.FundId,
                            YearMonth = Expressions.GetStrYearMonth(year, month),

                        // Updated Monthly balance
                        SharesNum = fundShares.SharesNum,
                            SharesValue = fundShares.SharesValue,
                            SharesFundPriceId = fundShares.SharesFundPriceId,

                        // New Shares
                        NewSharesNum = fundShares.NewSharesNum,
                            NewSharesValue = fundShares.NewSharesValue,

                            UpdatedOnUTC = updatedOnUTC
                        }
                    );
                    // Save often to 
                    db.SaveChanges();
                } catch (Exception e) {
                    // TODO: log customer id, fundId
                    var msg = e.Message;
                    throw e;
                }
            }
        }

        /// <summary>
        /// Return type of CalcCustomerMonthlyFundShares method
        /// </summary>
        public class PortfolioFundDTO {
            public int PortfolioId { get; set; }
            public int FundId { get; set; }
            public decimal SharesNum { get; set; }
            public decimal SharesValue { get; set; }
            public string SharesTradeDay { get; set; }
            public int SharesFundPriceId { get; set; }

            // Additional positive or negative shares
            public decimal? NewSharesNum { get; set; }
            public decimal? NewSharesValue { get; set; }
            public DateTime UpdatedOnUTC { get; set; }
        }

        /// <summary>
        /// Calculate the customer portfolio value for a given month.
        /// Phase 1 implements assumes only 1 portfolio in 100%
        /// Implementation done in 2 steps to prepare Phase 2.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="netInvAmount"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="updatedOnUTC"></param>
        public List<PortfolioFundDTO> GetCalcCustMonthlyFundShares(int customerId, decimal netInvAmount, int year, int month) {

            List<PortfolioFundDTO> portfolioFundValues = null;

            using (var db = new ApplicationDbContext()) {
                if (netInvAmount > 0) {

                    portfolioFundValues = BuyShares(customerId, netInvAmount, year, month, db);

                    // Note this case for repricing (0 cash) or liquidating shares to cash
                } else if (netInvAmount <= 0) {

                    SellShares(customerId, netInvAmount, year, month, db);

                }
            }
            return portfolioFundValues;
        }

        /// <summary>
        /// Sell shares to extract cash.
        /// Big difference compared to BuyShares method aside from the cash amount being negative in this case
        /// is that we use the previous month's portfolio to assign weight to the cash amount per fund.
        /// We don't care about this month's portfolio if the customer has selected a new one.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cashToExtract">Negative Cash Amount to extract by selling shares</param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private static List<PortfolioFundDTO> SellShares(int customerId, decimal cashToExtract, int year, int month, ApplicationDbContext db) {

            List<PortfolioFundDTO> portfolioFundValues = null;

            // Skip this month's but get previous month's portfolio
            DateTime prevYearMonth = new DateTime(year, month, 1).AddMonths(-1);
            string lastMonthPort = GetCustPortfYearMonth(customerId, prevYearMonth.Year, prevYearMonth.Month, db);
            if (lastMonthPort == null) {

                return null;
            }

            portfolioFundValues = GetCalcPortfShares(customerId, cashToExtract, year, month, db, lastMonthPort);

            return portfolioFundValues;
        }

        /// <summary>
        /// Buy shares for the cashToInvest cash amount.
        /// Big difference compared to SellShares method aside from the cash amount being positive in this case
        /// is that we use the present month's portfolio to assign weight to the cash amount per fund.
        /// We care about this month's customer portfolio to assign weights.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cashToInvest">Positive amount of cash to invest by buying shares</param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private static List<PortfolioFundDTO> BuyShares(int customerId, decimal cashToInvest, int year, int month, ApplicationDbContext db) {
            List<PortfolioFundDTO> portfolioFundValues;

            string lastMonthPort = GetCustPortfYearMonth(customerId, year, month, db);

            if (lastMonthPort == null) {

                throw new Exception($"No portfolio has been set for customer Id: {customerId}");
            }

            portfolioFundValues = GetCalcPortfShares(customerId, cashToInvest, year, month, db, lastMonthPort);

            return portfolioFundValues;
        }

        /// <summary>
        /// Calculating this months funds shares when
        /// +    -- More cash is invested according to the latest portfolio weight configuration
        /// - Or -- Selling Shares to get cash
        /// 0 Or -- 0 cash proceed to next step
        /// Then Reprice shares to this month's stock prices
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cashToInvest"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="db"></param>
        /// <param name="lastMonthPort"></param>
        /// <returns></returns>
        private static List<PortfolioFundDTO> GetCalcPortfShares(int customerId, decimal cashToInvest, int year, int month, ApplicationDbContext db, string lastMonthPort) {
            List<PortfolioFundDTO> portfolioFundValues;
            //Get Portfolio Funds Weights
            List<PortFund> portFundWeights = GetFundWeights(customerId, db, lastMonthPort);

            portfolioFundValues = new List<PortfolioFundDTO>();

            // Loop through each portfolio funds
            foreach (var pfund in portFundWeights) {
                var weight = pfund.Weight;
                var fundId = pfund.FundId;

                // If cash is positive then this is the latest
                var portfolioId = pfund.PortfolioId;
                var cashPerFund = (decimal)weight/100 * cashToInvest;

                //Find last trade day 
                var lastMonthDay = new DateTime(year, month, 1).AddMonths(1).AddDays(-1).ToString("yyyyMMdd");
                var lastTradeDay = db.FundPrices
                    .Where(fp => fp.FundId == fundId
                    && string.Compare(fp.YearMonthDay, lastMonthDay) <= 0)
                    .OrderByDescending(fp => fp.YearMonthDay)
                    .Select(fp => fp.YearMonthDay)
                    .First();
                // Find latest closing price
                var fundPrice = db.FundPrices
                    .Where(fp => fp.FundId == fundId && fp.YearMonthDay == lastTradeDay)
                    .Single();

                // Get previous months shares
                DateTime prevYearMonth = new DateTime(year, month, 1).AddMonths(-1);
                var preYearMonStr = Expressions.GetStrYearMonth(prevYearMonth.Year, prevYearMonth.Month);
                var prevMonShares = db.CustFundShares
                    .Where(f => f.CustomerId == customerId
                        && f.YearMonth == preYearMonStr
                        && f.FundId == fundId)
                    .Select(f => f.SharesNum)
                    .SingleOrDefault();

                // Calculate shares values balance and new purchases
                var NewsharesNum = cashPerFund / (decimal)fundPrice.ClosingPrice;

                var thisMonthsSharesNum = prevMonShares + NewsharesNum;

                var thisMonthsSharesVal = cashPerFund + prevMonShares * (decimal)fundPrice.ClosingPrice; // thisMonthsSharesNum * (decimal)fundPrice.ClosingPrice;

                var tradeDayDateTime = Expressions.GetDtYearMonthDay(lastTradeDay);

                SaveDtoPortFundShares(portfolioFundValues, fundId, portfolioId, cashPerFund, lastTradeDay, fundPrice, NewsharesNum, thisMonthsSharesNum, thisMonthsSharesVal);
            }

            return portfolioFundValues;
        }

        private static void SaveDtoPortFundShares(List<PortfolioFundDTO> portfolioFundValues, int fundId, int portfolioId, decimal cashPerFund, string lastTradeDay, FundPrice fundPrice, decimal NewsharesNum, decimal thisMonthsSharesNum, decimal thisMonthsSharesVal) {
            portfolioFundValues.Add(
                new PortfolioFundDTO() {
                    PortfolioId = portfolioId,
                    FundId = fundId,
                    SharesNum = thisMonthsSharesNum,
                    SharesValue = thisMonthsSharesVal,
                    SharesTradeDay = lastTradeDay,
                    SharesFundPriceId = fundPrice.Id,
                    NewSharesNum = NewsharesNum,
                    NewSharesValue = cashPerFund,
                    UpdatedOnUTC = DateTime.UtcNow
                }
            );
        }

        private static List<PortFund> GetFundWeights(int customerId, ApplicationDbContext db, string lastMonthPort) {
            return db.CustPortfolios
                .Include(p => p.Portfolio.PortFunds)
                .Where(p => p.CustomerId == customerId && p.YearMonth == lastMonthPort && p.Portfolio.PortFunds.Any(pf=>pf.PortfolioId == p.PortfolioId))
                .SelectMany(pf=>pf.Portfolio.PortFunds)
                .ToList();

            /*** Equivalent in Join syntax comprehensive syntax ***
            return (from pf in db.PortFunds
            join p in db.CustPortfolios on pf.PortfolioId equals p.PortfolioId
                 where p.CustomerId == customerId && p.YearMonth == lastMonthPort
            select pf).ToList(); */

        }

        /// <summary>
        /// Get the latest YearMonth of a saved customer portfolio selection
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private static string GetCustPortfYearMonth(int customerId, int year, int month, ApplicationDbContext db) {

            string YearMonthStr = Expressions.GetStrYearMonth(year, month);

            return db.CustPortfolios
                .Where(p => p.CustomerId == customerId && string.Compare(p.YearMonth, YearMonthStr) <= 0)
                .OrderByDescending(p => p.YearMonth)
                .Select(p => p.YearMonth)
                .First();
        }
    }
}
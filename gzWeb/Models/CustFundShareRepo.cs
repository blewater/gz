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
        public void SaveDbCustPurchasedFundShares(ApplicationDbContext db, int customerId, Dictionary<int, PortfolioFundDTO> fundsShares, int year, int month, DateTime updatedOnUTC) {

            foreach (var fundShares in fundsShares) {

                try {
                    db.CustFundShares.AddOrUpdate(
                        f => new { f.CustomerId, f.FundId, f.YearMonth },
                        new CustFundShare {
                        // Key
                        CustomerId = customerId,
                            FundId = fundShares.Value.FundId,
                            YearMonth = Expressions.GetStrYearMonth(year, month),

                        // Updated Monthly balance
                        SharesNum = fundShares.Value.SharesNum,
                            SharesValue = fundShares.Value.SharesValue,
                            SharesFundPriceId = fundShares.Value.SharesFundPriceId,

                        // New Shares
                        NewSharesNum = fundShares.Value.NewSharesNum,
                            NewSharesValue = fundShares.Value.NewSharesValue,

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
            public int? PortfolioId { get; set; }
            public int FundId { get; set; }
            public float? Weight { get; set; }
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
        public Dictionary<int, PortfolioFundDTO> GetCalcCustMonthlyFundShares(int customerId, decimal netInvAmount, int year, int month) {

            Dictionary<int, PortfolioFundDTO> portfolioFundValues = null;

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
        private static Dictionary<int, PortfolioFundDTO> SellShares(int customerId, decimal cashToExtract, int year, int month, ApplicationDbContext db) {

            Dictionary<int, PortfolioFundDTO> portfolioFundValues = null;

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
        private static Dictionary<int, PortfolioFundDTO> BuyShares(int customerId, decimal cashToInvest, int year, int month, ApplicationDbContext db) {
            Dictionary<int, PortfolioFundDTO> portfolioFundValues;

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
        private static Dictionary<int, PortfolioFundDTO> GetCalcPortfShares(int customerId, decimal cashToInvest, int year, int month, ApplicationDbContext db, string lastMonthPort) {

            var prevYearMonStr = Expressions.GetPrevYearMonth(year, month);

            //Get Portfolio Funds Weights
            Dictionary<int, PortfolioFundDTO> portfolioFundValues = GetFundWeights(customerId, db, lastMonthPort, prevYearMonStr);

            // Loop through each portfolio funds
            foreach (var pfund in portfolioFundValues) {
                var weight = pfund.Value.Weight; // weight is 0 for owned funds no longer part of the current customer portfolio
                var fundId = pfund.Value.FundId;

                
                var portfolioId = pfund.Value.PortfolioId;
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

                // Get previous from the queried CustomerFund 
                decimal prevMonShares = pfund.Value.SharesNum;

                // Calculate shares values balance and new purchases
                var NewsharesNum = cashPerFund / (decimal)fundPrice.ClosingPrice;

                var thisMonthsSharesNum = prevMonShares + NewsharesNum;

                var thisMonthsSharesVal = cashPerFund + prevMonShares * (decimal)fundPrice.ClosingPrice; // thisMonthsSharesNum * (decimal)fundPrice.ClosingPrice;

                var tradeDayDateTime = Expressions.GetDtYearMonthDay(lastTradeDay);

                SaveDtoPortFundShares(pfund.Value, cashPerFund, lastTradeDay, fundPrice, NewsharesNum, thisMonthsSharesNum, thisMonthsSharesVal);
            }

            return portfolioFundValues;
        }

        /// <summary>
        /// Save calculated values in DTO buffer
        /// </summary>
        /// <param name="savePortfolioFundDTO"></param>
        /// <param name="cashPerFund"></param>
        /// <param name="lastTradeDay"></param>
        /// <param name="fundPrice"></param>
        /// <param name="NewsharesNum"></param>
        /// <param name="thisMonthsSharesNum"></param>
        /// <param name="thisMonthsSharesVal"></param>
        private static void SaveDtoPortFundShares(
            PortfolioFundDTO savePortfolioFundDTO, decimal cashPerFund, string lastTradeDay, 
            FundPrice fundPrice, decimal NewsharesNum, decimal thisMonthsSharesNum, 
            decimal thisMonthsSharesVal) {

            savePortfolioFundDTO.SharesNum = thisMonthsSharesNum;
            savePortfolioFundDTO.SharesValue = thisMonthsSharesVal;
            savePortfolioFundDTO.SharesTradeDay = lastTradeDay;
            savePortfolioFundDTO.SharesFundPriceId = fundPrice.Id;
            savePortfolioFundDTO.NewSharesNum = NewsharesNum;
            savePortfolioFundDTO.NewSharesValue = cashPerFund;
            savePortfolioFundDTO.UpdatedOnUTC = DateTime.UtcNow;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="db"></param>
        /// <param name="lastMonthPort"></param>
        /// <param name="prevYearMonStr"></param>
        /// <returns></returns>
        private static Dictionary<int, PortfolioFundDTO> GetFundWeights(int customerId, ApplicationDbContext db, string lastMonthPort, string prevYearMonStr) {

            // STEP 1: Retrieve the funds associated with the current customer portfolio
            var fundsIQry = from pf in db.PortFunds
                join p in db.CustPortfolios on pf.PortfolioId equals p.PortfolioId
                where p.CustomerId == customerId && p.YearMonth == lastMonthPort
                select new PortfolioFundDTO {
                    FundId = pf.FundId,
                    PortfolioId = pf.PortfolioId,
                    Weight = pf.Weight,
                };

            // Get the last month that we have a customer balance
            var lastBalanceMonth = db.CustFundShares
                .Where(c => c.CustomerId == customerId && string.Compare(c.YearMonth, prevYearMonStr) <= 0)
                .OrderByDescending(c => c.YearMonth)
                .Select(c => c.YearMonth)
                .FirstOrDefault();

            // STEP 2: Get the current customer fund shares balances
            // Note on the first month this will return 0 rows
            var custFundsIQry =
                from c in db.CustFundShares
                where c.CustomerId == customerId 
                    && c.YearMonth == (lastBalanceMonth??prevYearMonStr)
                select new PortfolioFundDTO {
                    FundId = c.FundId,
                    PortfolioId = 0,
                    Weight = 0,
                    SharesNum = c.SharesNum
                };

            // STEP 3: Update the customer owned fund shares that overlap with their current selected portfolio funds of Step 1
            // the weight value that will be used to allocate cash for the current month
            var custFundsDct = custFundsIQry.ToDictionary(f=>f.FundId);
            foreach (var fundItem in fundsIQry) {

                if ( custFundsDct.ContainsKey(fundItem.FundId) ) {

                    // Portfolio Id Debugging info only
                    custFundsDct[fundItem.FundId].PortfolioId = fundItem.PortfolioId; 
                    custFundsDct[fundItem.FundId].Weight = fundItem.Weight;
                
               // else part executed only on the first customer balance month
                } else {
                    custFundsDct.Add(fundItem.FundId, fundItem);
                    
                }
            }

            return custFundsDct;

        }

        /// <summary>
        /// Get the latest YearMonth of a saved customer portfolio selection
        /// By business rules there will be a globally default portfolio for all customers
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
                .FirstOrDefault();
        }
    }
}
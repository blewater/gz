using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using gzWeb.Model.Util;
using gzWeb.Repo.Interfaces;
using gzWeb.Models;

namespace gzWeb.Repo {
    public class CustFundShareRepo : ICustFundShareRepo
    {

        private readonly ApplicationDbContext db;
        public CustFundShareRepo(ApplicationDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Save purchased funds shares to the customer's account.
        /// 
        /// Call DBcontext.SaveChanges() on incoming db object for transaction support
        /// </summary>
        /// <param name="db"></param>
        /// <param name="customerId"></param>
        /// <param name="fundsShares"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="updatedOnUTC"></param>
        public void SaveDbCustPurchasedFundShares(int customerId, Dictionary<int, PortfolioFundDTO> fundsShares, int year, int month, DateTime updatedOnUTC) {

            foreach (var fundShares in fundsShares) {

                try {
                    db.CustFundShares.AddOrUpdate(
                        f => new { f.CustomerId, f.FundId, f.YearMonth },
                        new CustFundShare {
                        // Key
                        CustomerId = customerId,
                            FundId = fundShares.Value.FundId,
                            YearMonth = DbExpressions.GetStrYearMonth(year, month),

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
                    throw ;
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
                if (netInvAmount >= 0) {

                    portfolioFundValues = GetBoughtShares(customerId, netInvAmount, year, month);

                    // Note this case for repricing (0 cash) or liquidating shares to cash
                } else if (netInvAmount < 0) {

                    GetSoldShares(customerId, netInvAmount, year, month);

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
        private Dictionary<int, PortfolioFundDTO> GetSoldShares(int customerId, decimal cashToExtract, int year, int month) {

            Dictionary<int, PortfolioFundDTO> portfolioFundValues = null;

            // Skip this month's but get previous month's portfolio
            DateTime prevYearMonth = new DateTime(year, month, 1).AddMonths(-1);
            string YearMonthStr = DbExpressions.GetStrYearMonth(year, month);
            string lastMonthPort = GetCustPortfYearMonth(customerId, YearMonthStr, db);
            if (lastMonthPort == null) {

                return null;
            }

            // Get previous balance
            var prevBal = db.InvBalances
                .Where(b => b.CustomerId == customerId
                    && b.Id ==
                        (db.InvBalances
                        .Where(p => p.CustomerId == customerId 
                            && string.Compare(p.YearMonth, DbExpressions.GetStrYearMonth(prevYearMonth.Year, prevYearMonth.Month)) <=0)
                        .Select(p => p.Id)
                        .Max())
                )
                .Select(b => b.Balance)
                .SingleOrDefault();

            portfolioFundValues = GetCalcPortfShares(customerId, cashToExtract, year, month, lastMonthPort, prevBal);

            return portfolioFundValues;
        }

        /// <summary>
        /// Buy shares for the cashToInvest cash amount.
        /// Difference compared to SellShares method aside from the cash amount being positive
        /// is that we use the present month's portfolio to assign weight to the cash amount per fund.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cashToInvest">Positive amount of cash to invest by buying shares</param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private Dictionary<int, PortfolioFundDTO> GetBoughtShares(int customerId, decimal cashToInvest, int year, int month) {
            Dictionary<int, PortfolioFundDTO> portfolioFundValues;

            string lastMonthPort = GetCustPortfYearMonth(customerId, DbExpressions.GetStrYearMonth(year, month), db);

            System.Diagnostics.Trace.Assert(lastMonthPort != null, String.Format("No portfolio has been set for customer Id: {0}", customerId));

            if (lastMonthPort == null) {

                throw new Exception(String.Format("No portfolio has been set for customer Id: {0}", customerId));
            }

            portfolioFundValues = GetCalcPortfShares(customerId, cashToInvest, year, month, lastMonthPort);

            return portfolioFundValues;
        }

        /// <summary>
        /// Calculating this months funds shares when
        /// +    -- More cash is invested according to the latest portfolio weight configuration
        /// - Or -- Selling Shares to get cash
        /// 0 Or -- 0 see line below
        /// Then Re-price shares to this month's stock prices
        /// </summary>
        /// <param name="db"></param>
        /// <param name="customerId"></param>
        /// <param name="cashToInvest"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="lastMonthPort"></param>
        /// <param name="prevInvBal"></param>
        /// <returns></returns>
        private Dictionary<int, PortfolioFundDTO> GetCalcPortfShares(int customerId, decimal cashToInvest, int year, int month, string lastMonthPort, decimal prevInvBal = 0) {

            var prevYearMonStr = DbExpressions.GetPrevYearMonth(year, month);
            decimal cashToGetBySellingShares = 0;

            // Negative cash or amount to sell becomes positive amount to invest by subtracting it from the previous balance
            if (cashToInvest < 0) {

                cashToGetBySellingShares = cashToInvest;
                cashToInvest = prevInvBal - cashToInvest;
            }

            //Get Portfolio Funds Weights
            Dictionary<int, PortfolioFundDTO> portfolioFundValues = GetFundWeights(customerId, lastMonthPort, prevYearMonStr);

            GetCashtoFundsShares(portfolioFundValues, year, month, cashToInvest, cashToGetBySellingShares, prevInvBal);

            return portfolioFundValues;
        }

        /// <summary>
        /// Convert cash --> funds shares  
        /// Calculate all the funds shares metrics and save them in the input collection
        /// </summary>
        /// <param name="db"></param>
        /// <param name="portfolioFundValues"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="cashToInvest"></param>
        /// <param name="cashToGetBySellingShares"></param>
        /// <param name="prevInvBal"></param>
        private void GetCashtoFundsShares(Dictionary<int, PortfolioFundDTO> portfolioFundValues, int year, int month, 
            decimal cashToInvest, 
            decimal cashToGetBySellingShares = 0, 
            decimal prevInvBal = 0) {

            // Process the portfolio funds and any additional customer owned funds
            foreach (var pfund in portfolioFundValues) {
                // weight is 0 for owned funds no longer part of the current customer portfolio
                var weight = pfund.Value.Weight; 
                var fundId = pfund.Value.FundId;

                // Get previous from the queried CustomerFund 
                decimal prevMonShares = pfund.Value.SharesNum;

                var portfolioId = pfund.Value.PortfolioId;

                string lastTradeDay;
                FundPrice fundPrice = GetFundPrice(year, month, fundId, out lastTradeDay);
                
                decimal cashPerFund = (decimal)weight / 100 * cashToInvest, existingFundSharesVal = prevMonShares * (decimal)fundPrice.ClosingPrice;

                // Calculate shares values balance and new purchases
                var NewsharesNum = cashPerFund / (decimal)fundPrice.ClosingPrice;

                var thisMonthsSharesNum = prevMonShares + NewsharesNum;

                var thisMonthsSharesVal = cashPerFund + existingFundSharesVal;

                var tradeDayDateTime = DbExpressions.GetDtYearMonthDay(lastTradeDay);

                UpdateDtoPortFundShares(pfund.Value, cashPerFund, lastTradeDay, fundPrice, NewsharesNum, thisMonthsSharesNum, thisMonthsSharesVal);
            }
        }

        /// <summary>
        /// Get a single fund latest closing price relative to the requested year month
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="db"></param>
        /// <param name="fundId"></param>
        /// <param name="lastTradeDay"></param>
        /// <returns></returns>
        private FundPrice GetFundPrice(int year, int month, int fundId, out string lastTradeDay) {

            FundPrice fundPriceToRet = null;

            //Find last trade day 
            var lastMonthDay = new DateTime(year, month, 1).AddMonths(1).AddDays(-1).ToString("yyyyMMdd");
            lastTradeDay = db.FundPrices
                .Where(fp => fp.FundId == fundId
                && string.Compare(fp.YearMonthDay, lastMonthDay) <= 0)
                .OrderByDescending(fp => fp.YearMonthDay)
                .Select(fp => fp.YearMonthDay)
                .First();
                
            string locLastTradeDay = lastTradeDay;
            // Find latest closing price
            fundPriceToRet = db.FundPrices
                .Where(fp => fp.FundId == fundId && fp.YearMonthDay == locLastTradeDay )
                .Single();

            return fundPriceToRet;
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
        private void UpdateDtoPortFundShares(
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
        private Dictionary<int, PortfolioFundDTO> GetFundWeights(int customerId, string lastMonthPort, string prevYearMonStr) {

            // STEP 1: Retrieve the funds associated with the current customer portfolio
            var fundsIQry = from pf in db.PortFunds
                            join p in db.CustPortfolios on pf.PortfolioId equals p.PortfolioId
                            where p.CustomerId == customerId && p.YearMonth == lastMonthPort
                            select new PortfolioFundDTO {
                                FundId = pf.FundId,
                                PortfolioId = pf.PortfolioId,
                                Weight = pf.Weight,
                            };

            string lastBalanceMonth = GetLastFundSharesBalMonth(customerId, db, prevYearMonStr);

            // STEP 2: Get the current customer fund shares balances
            // Note on the first month this will return 0 rows
            var custFundsIQry =
                from c in db.CustFundShares
                where c.CustomerId == customerId
                    && c.YearMonth == (lastBalanceMonth ?? prevYearMonStr)
                select new PortfolioFundDTO {
                    FundId = c.FundId,
                    PortfolioId = 0,
                    Weight = 0,
                    SharesNum = c.SharesNum
                };

            // STEP 3: Update the customer owned fund shares that overlap with their current selected portfolio funds of Step 1
            // the weight value that will be used to allocate cash for the current month
            var custFundsDct = custFundsIQry.ToDictionary(f => f.FundId);
            foreach (var fundItem in fundsIQry) {

                if (custFundsDct.ContainsKey(fundItem.FundId)) {

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
        /// This is useful for test customer accounts when they have monthly gaps
        /// in their funds shares balances
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="db"></param>
        /// <param name="prevYearMonStr"></param>
        /// <returns></returns>
        private string GetLastFundSharesBalMonth(int customerId, ApplicationDbContext db, string prevYearMonStr) {
            // Get the last month that we have a customer balance
            return db.CustFundShares
                .Where(c => c.CustomerId == customerId && string.Compare(c.YearMonth, prevYearMonStr) <= 0)
                .OrderByDescending(c => c.YearMonth)
                .Select(c => c.YearMonth)
                .FirstOrDefault();
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
        private string GetCustPortfYearMonth(int customerId, string YearMonthStr, ApplicationDbContext db) {

            return db.CustPortfolios
                .Where(p => p.CustomerId == customerId && string.Compare(p.YearMonth, YearMonthStr) <= 0)
                .OrderByDescending(p => p.YearMonth)
                .Select(p => p.YearMonth)
                .FirstOrDefault();
        }
    }
}
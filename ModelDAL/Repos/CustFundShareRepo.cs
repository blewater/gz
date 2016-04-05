using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Migrations;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using gzDAL.Models;

namespace gzDAL.Repos {
    public class CustFundShareRepo : ICustFundShareRepo {

        private readonly ApplicationDbContext db;
        public CustFundShareRepo(ApplicationDbContext db) {
            this.db = db;
        }

        ///  <summary>
        ///
        ///  Save purchased or sold funds shares to the customer's account.
        ///
        ///  Call DBcontext.SaveChanges() on incoming db object for transaction support
        ///
        ///  </summary>
        ///  <param name="db"></param>
        ///  <param name="customerId"></param>
        ///  <param name="fundsShares"></param>
        ///  <param name="year"></param>
        ///  <param name="month"></param>
        ///  <param name="updatedOnUTC"></param>
        /// <param name="boughtShares"></param>
        public void SaveDBMonthlyCustomerFundShares(
            bool boughtShares,
            int customerId,
            Dictionary<int, PortfolioFundDTO> fundsShares,
            int year,
            int month,
            DateTime updatedOnUTC) {

            foreach (var fundShares in fundsShares) {

                var custFundShare = new CustFundShare {
                    // Key
                    CustomerId = customerId,
                    FundId = fundShares.Value.FundId,
                    YearMonth = DbExpressions.GetStrYearMonth(year, month),

                    // Updated Shares Values Monthly balance
                    // -- Buying or Selling logic
                    SharesNum = boughtShares ? fundShares.Value.SharesNum : 0,
                    SharesValue = boughtShares ? fundShares.Value.SharesValue : 0,

                    // New Shares
                    // -- Buying or Selling logic
                    NewSharesNum = boughtShares 
                                        ? fundShares.Value.NewSharesNum 
                                        : -fundShares.Value.NewSharesNum,

                    NewSharesValue = boughtShares 
                                        ? fundShares.Value.NewSharesValue 
                                        : -fundShares.Value.NewSharesValue,

                    SharesFundPriceId = fundShares.Value.SharesFundPriceId,
                    UpdatedOnUTC = updatedOnUTC
                };

                SaveDBCustFundShare(custFundShare, customerId, year, month, updatedOnUTC, fundShares);
            }
        }

        /// <summary>
        /// Save one 1 Customer Fund Share Row
        /// Rather than save all of them at once for a single customer, it saves row by row to get immediate error feedback
        /// </summary>
        /// <param name="custFundShare"></param>
        /// <param name="customerId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="updatedOnUTC"></param>
        /// <param name="fundShares"></param>
        private void SaveDBCustFundShare(CustFundShare custFundShare, int customerId, int year, int month, DateTime updatedOnUTC, KeyValuePair<int, PortfolioFundDTO> fundShares) {

            try {

                db.CustFundShares.AddOrUpdate(
                    // Keys
                    f => new {f.CustomerId, f.FundId, f.YearMonth},

                    // Row object value
                    custFundShare
                );
                db.SaveChanges();

            } catch (Exception e) {
                // TODO: log customer id, fundId
                var msg = e.Message;
                throw;
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
        /// Sell all shares to extract cash.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private Dictionary<int, PortfolioFundDTO> GetSoldShares(int customerId, decimal netInvAmount, int year, int month) {

            var customerShares = (
                from s in db.CustFundShares
                where s.CustomerId == customerId
                      && s.YearMonth == DbExpressions.GetStrYearMonth(year, month)
                select new PortfolioFundDTO {
                    FundId = s.FundId,
                    SharesNum = s.SharesNum,
                    SharesValue = 0
                })
                .ToDictionary(f => f.FundId);

            GetUpdatedFundsSharesValue(customerShares, year, month, cashToInvest: 0);

            return customerShares;
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
        /// Calculating this month's funds shares when
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

            var prevYearMonStr = DbExpressions.GetPrevMonthInYear(year, month);

            //Get Portfolio Funds Weights
            Dictionary<int, PortfolioFundDTO> portfolioFundValues = GetFundWeights(customerId, lastMonthPort, prevYearMonStr);

            GetUpdatedFundsSharesValue(portfolioFundValues, year, month, cashToInvest);

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
        private void GetUpdatedFundsSharesValue(
                Dictionary<int, PortfolioFundDTO> portfolioFundValues,
                int year,
                int month,
                decimal cashToInvest) {

            // Process the portfolio funds and any additional customer owned funds
            foreach (var pfund in portfolioFundValues) {
                // weight is 0 for owned funds no longer part of the current customer portfolio
                var weight = pfund.Value.Weight;
                var fundId = pfund.Value.FundId;

                // Get previous from the queried CustomerFund
                decimal prevMonShares = pfund.Value.SharesNum;

                // Calculate the current fund shares value
                string lastTradeDay;
                FundPrice fundPrice = GetFundPrice(year, month, fundId, out lastTradeDay);
                decimal existingFundSharesVal = prevMonShares * (decimal)fundPrice.ClosingPrice;

                // Calculate new cash --> to shares investment
                decimal cashPerFund = (decimal)weight / 100 * cashToInvest;
                var newsharesNum = cashPerFund / (decimal)fundPrice.ClosingPrice;

                // Calculate this month current fund value including the new cash investment
                var thisMonthsSharesNum = prevMonShares + newsharesNum;
                var thisMonthsSharesVal = cashPerFund + existingFundSharesVal;

                UpdateDtoPortFundShares(pfund.Value, cashPerFund, lastTradeDay, fundPrice, newsharesNum, thisMonthsSharesNum, thisMonthsSharesVal);
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
                .Where(fp => fp.FundId == fundId && fp.YearMonthDay == locLastTradeDay)
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
        /// <param name="lastMonthPort">The month associated with current portfolio</param>
        /// <param name="prevYearMonStr">One (-1) month before the current one</param>
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
            var custFundsDct = (
                from c in db.CustFundShares
                where c.CustomerId == customerId
                      && c.YearMonth == (lastBalanceMonth ?? prevYearMonStr)

                select new PortfolioFundDTO {
                    FundId = c.FundId,
                    PortfolioId = 0,
                    Weight = 0,
                    SharesNum = c.SharesNum

                })
                 .ToDictionary(f => f.FundId);

            // STEP 3: Update the customer owned fund shares that overlap with their current selected portfolio funds of Step 1
            // the weight of which will be used to allocate cash for the current month
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
                .Where(p => p.CustomerId == customerId && string.Compare(p.YearMonth, YearMonthStr, StringComparison.Ordinal) <= 0)
                .OrderByDescending(p => p.YearMonth)
                .Select(p => p.YearMonth)
                .FirstOrDefault();
        }
    }
}
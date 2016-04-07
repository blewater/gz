using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Migrations;
using gzDAL.DTO;
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
        public void SaveDbMonthlyCustomerFundShares(
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
                    SharesNum = boughtShares 
                                    ? fundShares.Value.SharesNum 
                                    : -fundShares.Value.SharesNum,

                    SharesValue = boughtShares 
                                    ? fundShares.Value.SharesValue 
                                    : -fundShares.Value.SharesValue,

                    // New Shares
                    // -- Buying or Selling logic
                    NewSharesNum = boughtShares 
                                        ? fundShares.Value.NewSharesNum 
                                        : 0,

                    NewSharesValue = boughtShares 
                                        ? fundShares.Value.NewSharesValue 
                                        : 0,

                    SharesFundPriceId = fundShares.Value.SharesFundPriceId,
                    UpdatedOnUTC = updatedOnUTC
                };

                SaveDbCustFundShare(custFundShare, customerId, year, month, updatedOnUTC, fundShares);
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
        private void SaveDbCustFundShare(CustFundShare custFundShare, int customerId, int year, int month, DateTime updatedOnUTC, KeyValuePair<int, PortfolioFundDTO> fundShares) {

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
        /// Calculate the customer portfolio value for a given month.
        /// Phase 1 implements assumes only 1 portfolio in 100%
        /// Implementation done in 2 steps to prepare Phase 2.
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="netInvAmount"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="updatedOnUTC"></param>
        public Dictionary<int, PortfolioFundDTO> GetCalcCustomerMonthlyFundShares(int customerId, decimal netInvAmount, int year, int month) {

            Dictionary<int, PortfolioFundDTO> portfolioFundValues = null;

            if (netInvAmount >= 0) {

                portfolioFundValues = GetOwnedFundSharesPortfolioWeights(customerId, netInvAmount, year, month);

                // Note this case for repricing (0 cash) or liquidating shares to cash
            } else if (netInvAmount < 0) {

                portfolioFundValues = GetPortfolioSharesValue(customerId, netInvAmount, year, month, currentPortfolioMonth: null);

            }

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
        private Dictionary<int, PortfolioFundDTO> GetOwnedFundSharesPortfolioWeights(int customerId, decimal cashToInvest, int year, int month) {

            string currentPortfolioMonth = GetCustPortfYearMonth(customerId, DbExpressions.GetStrYearMonth(year, month), db);

            System.Diagnostics.Trace.Assert(currentPortfolioMonth != null, string.Format("No portfolio has been set for customer Id: {0}", customerId));

            if (currentPortfolioMonth == null) {

                throw new Exception(string.Format("No portfolio has been set for customer Id: {0}", customerId));
            }

            var portfolioFundValues = GetPortfolioSharesValue(customerId, cashToInvest, year, month, currentPortfolioMonth);

            return portfolioFundValues;
        }

        /// <summary>
        /// 
        /// Calculating this month's funds shares value in $ when
        /// 
        /// + : More cash is invested according to the latest portfolio weight configuration
        /// - : Selling Shares to get cash
        /// 0 : Move to next step
        /// 
        /// Then Re-price shares to this month's stock prices
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="customerId"></param>
        /// <param name="cashToInvest"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <param name="currentPortfolioMonth"></param>
        /// <returns>Dictionary of Portfolio DTO</returns>
        private Dictionary<int, PortfolioFundDTO> GetPortfolioSharesValue(
            int customerId, 
            decimal cashToInvest, 
            int yearCurrent, 
            int monthCurrent, 
            string currentPortfolioMonth) {

            if (cashToInvest > 0 && string.IsNullOrEmpty(currentPortfolioMonth)) {
                
                throw new Exception("Cannot invest new cash without a valid portfolio");

            }

            // Get Customer Owned Funds
            var customerShares = GetOwnedCustomerFunds(customerId, yearCurrent, monthCurrent);

            if (customerShares.Count > 0) {

                //Get Portfolio Funds Weights only about to buy shares
                if (cashToInvest > 0 && !string.IsNullOrEmpty(currentPortfolioMonth)) {

                    SetPortfolioFundWeights(customerId, customerShares, currentPortfolioMonth);
                }

                SetFundsSharesValue(customerShares, yearCurrent, monthCurrent, cashToInvest);
            }

            return customerShares;
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
        private void SetFundsSharesValue(
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
                var cashPerFund = (decimal)weight/ 100 * cashToInvest;
                var newsharesNum = cashPerFund / (decimal)fundPrice.ClosingPrice;

                // Calculate this month current fund value including the new cash investment
                var thisMonthsSharesNum = prevMonShares + newsharesNum;
                var thisMonthsSharesVal = cashPerFund + existingFundSharesVal;

                SaveDtoPortFundShares(pfund.Value, cashPerFund, lastTradeDay, fundPrice, newsharesNum, thisMonthsSharesNum, thisMonthsSharesVal);
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
                && string.Compare(fp.YearMonthDay, lastMonthDay, StringComparison.Ordinal) <= 0)
                .OrderByDescending(fp => fp.YearMonthDay)
                .Select(fp => fp.YearMonthDay)
                .First();

            string locLastTradeDay = lastTradeDay;
            // Find latest closing price
            fundPriceToRet = db.FundPrices
                .Single(fp => fp.FundId == fundId && fp.YearMonthDay == locLastTradeDay);

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
        private void SaveDtoPortFundShares(
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
        /// Retrieve all the Customer funds with their weights so we can allocate cash this month.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="customerOwnedFundsDct">The owned customer fund shares if any. Note is 0 on first customer month.</param>
        /// <param name="currentPortfolioMonth">The month associated with current portfolio</param>
        /// <returns>
        /// 
        /// Updated funds collection has updated these values:
        /// 1. their portfolio funds weights 
        /// 2. the number of shares of Owned Customer funds.
        /// 
        /// </returns>
        private void SetPortfolioFundWeights(
            int customerId,
            Dictionary<int, PortfolioFundDTO> customerOwnedFundsDct,
            string currentPortfolioMonth) {

            var fundsIQry = GetPortfolioFunds(customerId, currentPortfolioMonth);

            // Match Portfolio Funds with --> Already Owned Customer Funds of In parameter 
            // Update the customer owned fund shares that overlap with their current selected portfolio funds of Step 1
            // the weight of which will be used to allocate cash for the current month
            foreach (var fundItem in fundsIQry) {

                if (customerOwnedFundsDct.ContainsKey(fundItem.FundId)) {

                    // Portfolio Id here is set on the customer owned funds that match 
                    // the current Customer portfolio
                    customerOwnedFundsDct[fundItem.FundId].PortfolioId = fundItem.PortfolioId;
                    customerOwnedFundsDct[fundItem.FundId].Weight = fundItem.Weight;

                    // Else part executed only on the first customer balance month
                } else {
                    customerOwnedFundsDct.Add(fundItem.FundId, fundItem);

                }
            }
        }

        /// <summary>
        /// 
        /// Retrieve the funds from the currently selected customer Portfolio 
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="currentPortfolioMonth">The </param>
        /// <returns>The funds IQueryable holding PortfolioDTOs</returns>
        private IQueryable<PortfolioFundDTO> GetPortfolioFunds(int customerId, string currentPortfolioMonth) {

            return from pf in db.PortFunds
                join p in db.CustPortfolios on pf.PortfolioId equals p.PortfolioId
                where p.CustomerId == customerId && p.YearMonth == currentPortfolioMonth
                select new PortfolioFundDTO {
                    FundId = pf.FundId,
                    PortfolioId = pf.PortfolioId,
                    Weight = pf.Weight,
                };
        }

        /// <summary>
        /// 
        /// Get the current customer Owned fund shares balances
        /// 
        /// Note on the first month this will return 0 rows
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="monthCurrent"></param>
        /// <param name="lastBalanceMonth"></param>
        /// <param name="yearCurrent"></param>
        /// <returns></returns>
        private Dictionary<int, PortfolioFundDTO> GetOwnedCustomerFunds(int customerId, int yearCurrent, int monthCurrent) {

            string lastFundsHoldingMonth = GetLastFundSharesHoldingMonth(customerId, db,
                    DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent));

            var prevYearMonth = DbExpressions.GetPrevMonthInYear(yearCurrent, monthCurrent);

            var ownedFunds = (
                from c in db.CustFundShares
                where c.CustomerId == customerId
                    && c.SharesNum > 0
                    && c.YearMonth == (lastFundsHoldingMonth ?? prevYearMonth)
                select new PortfolioFundDTO {
                    FundId = c.FundId,
                    PortfolioId = 0,
                    Weight = 0,
                    SharesNum = c.SharesNum
                })
                .ToDictionary(f => f.FundId);

            return ownedFunds;
        }

        /// <summary>
        /// 
        /// Get the last funds holding month in CustFundShares less or equal
        /// than the month in value of in parameter currentYearMonStr.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="db"></param>
        /// <param name="currentYearMonStr"></param>
        /// <returns></returns>
        private string GetLastFundSharesHoldingMonth(int customerId, ApplicationDbContext db, string currentYearMonStr) {

            return db.CustFundShares
                .Where(c => c.CustomerId == customerId 
                    && string.Compare(c.YearMonth, currentYearMonStr, StringComparison.Ordinal) <= 0)
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
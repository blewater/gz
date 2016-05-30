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
        private readonly ICustPortfolioRepo _custPortfolioRepo;

        public CustFundShareRepo(ApplicationDbContext db, ICustPortfolioRepo custPortfolioRepo) {

            this.db = db;
            this._custPortfolioRepo = custPortfolioRepo;

        }

        ///  <summary>
        ///
        ///  Save purchased or sold funds shares to the customer's account.
        ///
        ///  </summary>
        ///  <param name="db"></param>
        ///  <param name="customerId"></param>
        ///  <param name="fundsShares"></param>
        ///  <param name="year"></param>
        ///  <param name="month"></param>
        ///  <param name="updatedOnUtc"></param>
        /// <param name="boughtShares"></param>
        public void SaveDbMonthlyCustomerFundShares(
            bool boughtShares,
            int customerId,
            Dictionary<int, PortfolioFundDTO> fundsShares,
            int year,
            int month,
            DateTime updatedOnUtc) {

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
                    UpdatedOnUtc = updatedOnUtc
                };

                SaveDbCustFundShare(custFundShare, customerId, year, month, updatedOnUtc, fundShares);
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
        /// <param name="updatedOnUtc"></param>
        /// <param name="fundShares"></param>
        private void SaveDbCustFundShare(CustFundShare custFundShare, int customerId, int year, int month, DateTime updatedOnUtc, KeyValuePair<int, PortfolioFundDTO> fundShares) {

            try {

                db.CustFundShares.AddOrUpdate(
                    // Keys
                    f => new {f.CustomerId, f.FundId, f.YearMonth},

                    // Row object value
                    custFundShare
                );

            } catch (Exception e) {
                // TODO: log customer id, fundId
                var msg = e.Message;
                throw;
            }
        }

        /// <summary>
        /// Calculate the customer portfolio value for a given month.
        /// Phase 1 launch assumes only 1 portfolio possession in 100%
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="netInvAmount"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="updatedOnUtc"></param>
        public Dictionary<int, PortfolioFundDTO> GetMonthlyFundSharesAfterBuyingSelling(int customerId, decimal netInvAmount, int year, int month) {

            Dictionary<int, PortfolioFundDTO> portfolioFundValues = null;

            if (netInvAmount >= 0) {

                portfolioFundValues = GetOwnedFundSharesPortfolioWeights(customerId, netInvAmount, year, month);

                // Note this case for repricing (0 cash) or liquidating shares to cash
            } 
            //else if (netInvAmount < 0) {

            //    portfolioFundValues = GetMonthsBoughtFundsValue(customerId, year, month);
            //}

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
        /// <returns></returns>
        private Dictionary<int, PortfolioFundDTO> GetOwnedFundSharesPortfolioWeights(int customerId, decimal cashToInvest, int year, int month) {

            var portfolio = _custPortfolioRepo.GetCustomerPortfolioForMonth(customerId, DbExpressions.GetStrYearMonth(year, month));

            System.Diagnostics.Trace.Assert(portfolio != null, string.Format("No portfolio has been set for customer Id: {0}", customerId));

            if (portfolio == null) {

                throw new Exception(string.Format("No portfolio has been set for customer Id: {0}", customerId));
            }

            var portfolioFundValues = GetPortfolioSharesValue(customerId, cashToInvest, year, month, portfolio);

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
        /// <param name="customerPortfolio"></param>
        /// <returns>Dictionary of Portfolio DTO</returns>
        private Dictionary<int, PortfolioFundDTO> GetPortfolioSharesValue(
            int customerId, 
            decimal cashToInvest, 
            int yearCurrent, 
            int monthCurrent, 
            Portfolio customerPortfolio) {

            if (cashToInvest > 0 && customerPortfolio == null) {
                
                throw new Exception("Cannot invest new cash without a valid portfolio");

            }

            // Get Customer Owned Funds
            var customerShares = GetOwnedCustomerFunds(customerId, yearCurrent, monthCurrent);

            //Get Portfolio Funds Weights if buying shares
            if (cashToInvest > 0 && customerPortfolio != null) {

                SetPortfolioFundWeights(customerId, customerShares, customerPortfolio);
            }

            SetFundsSharesBalance(customerShares, yearCurrent, monthCurrent, cashToInvest);

            return customerShares;
        }

        /// <summary>
        /// 
        /// Calculate the value of a month's bought (not owned) funds shares.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <returns></returns>
        public IEnumerable<CustFundShare> GetMonthsBoughtFundsValue(int customerId, int yearCurrent, int monthCurrent) {

            var customerShares = GetMonthsBoughtFundsShares(customerId, yearCurrent, monthCurrent);

            SetFundsSharesLatestValue(customerShares);

            return customerShares;
        }

        /// <summary>
        /// Convert cash --> funds shares within the month's market prices
        /// 1. Convert previous funds balance in this month's market prices
        /// 2. Convert the new monthly cash investment to fund shares
        /// 3. Add 1+2 = for this month's balance
        /// Save the collection of total fund shares data in a dictionary.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="portfolioFundValues"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="cashToInvest"></param>
        private void SetFundsSharesBalance(
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
                FundPrice fundPrice = GetFundPriceForCurrentMonth(year, month, fundId);
                decimal existingFundSharesVal = prevMonShares * (decimal)fundPrice.ClosingPrice;

                // Calculate new cash --> to shares investment
                var cashPerFund = (decimal)weight/ 100 * cashToInvest;
                var newsharesNum = cashPerFund / (decimal)fundPrice.ClosingPrice;

                // Calculate this month current fund value including the new cash investment
                var thisMonthsSharesNum = prevMonShares + newsharesNum;
                var thisMonthsSharesVal = cashPerFund + existingFundSharesVal;

                SaveDtoPortFundShares(pfund.Value, cashPerFund, fundPrice.YearMonthDay, fundPrice, newsharesNum, thisMonthsSharesNum, thisMonthsSharesVal);
            }
        }

        /// <summary>
        /// 
        /// Calculate the latest funds prices of the In parameter shares collection.
        /// 
        /// To be used for selling shares not for calculating balances.
        /// 
        /// </summary>
        /// <param name="soldCustFundShares"></param>
        private void SetFundsSharesLatestValue(
                IEnumerable<CustFundShare> soldCustFundShares) {

            // Process the portfolio funds and any additional customer owned funds
            foreach (var custFundShare in soldCustFundShares) {

                var fundId = custFundShare.FundId;

                // Get previous from the queried CustomerFund
                decimal thisMonShares = custFundShare.SharesNum;

                // Calculate the current fund shares value
                FundPrice fundPrice = GetLatestFundPrice(fundId);
                decimal existingFundSharesVal = thisMonShares * (decimal)fundPrice.ClosingPrice;

                // Calculate this month current fund value including the new cash investment
                custFundShare.SharesValue = existingFundSharesVal;
            }
        }

        /// <summary>
        /// Get a single fund latest closing price relative to the requested year month
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="fundId"></param>
        /// <returns></returns>
        private FundPrice GetFundPriceForCurrentMonth(int year, int month, int fundId) {

            FundPrice fundPriceToRet = null;

            //Find last trade day
            var lastMonthDay = new DateTime(year, month, 1).AddMonths(1).AddDays(-1).ToString("yyyyMMdd");
            var lastTradeDay = db.FundPrices
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
        /// 
        /// Gets the latest stored fund price for a traded fund share.
        /// 
        /// </summary>
        /// <param name="fundId"></param>
        /// <returns></returns>
        private FundPrice GetLatestFundPrice(int fundId) {

            FundPrice fundPriceToRet = null;

            // Find latest closing price
            fundPriceToRet = db.FundPrices
                .Where(f => f.FundId == fundId)
                .OrderByDescending(f => f.YearMonthDay)
                .First();

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
        /// <param name="customerPortfolio"></param>
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
            Portfolio customerPortfolio) {

            var fundsIQry = GetPortfolioFunds(customerId, customerPortfolio);

            // Match Portfolio Funds with --> Already Owned Customer Funds of In parameter 
            // Update the customer owned fund shares that overlap with their currently selected portfolio funds
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
        /// <param name="customerPortfolio"></param>
        /// <returns>The funds IQueryable holding PortfolioDTOs</returns>
        private IQueryable<PortfolioFundDTO> GetPortfolioFunds(int customerId, Portfolio customerPortfolio) {

            return from pf in db.PortFunds
                join p in db.CustPortfolios on pf.PortfolioId equals p.PortfolioId
                where p.CustomerId == customerId && p.PortfolioId == customerPortfolio.Id
                select new PortfolioFundDTO {
                    FundId = pf.FundId,
                    PortfolioId = pf.PortfolioId,
                    Weight = pf.Weight,
                };
        }

        /// <summary>
        /// 
        /// Get the current customer Owned fund shares balances from the greatest previous month (not present).
        /// 
        /// Note on the first month this will return 0 rows
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="monthCurrent"></param>
        /// <param name="yearCurrent"></param>
        /// <returns></returns>
        private Dictionary<int, PortfolioFundDTO> GetOwnedCustomerFunds(int customerId, int yearCurrent, int monthCurrent) {

            string currentYearMonthStr = DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent);
            string lastFundsHoldingMonth =
                GetFundSharesFromLastPurchase(customerId, db, currentYearMonthStr) ?? "";

            var ownedFunds = (
                from c in db.CustFundShares
                where c.CustomerId == customerId
                    && c.SharesNum > 0
                    && c.YearMonth == lastFundsHoldingMonth 
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
        /// Get the *Purchased* (NewShares) for a month.
        /// 
        /// Used when selling a month's vintage.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="yearCurrent"></param>
        /// <param name="monthCurrent"></param>
        /// <returns></returns>
        private IEnumerable<CustFundShare> GetMonthsBoughtFundsShares(
            int customerId, 
            int yearCurrent, 
            int monthCurrent) {

            string lastFundsHoldingMonth = GetMonthsFundShares(customerId, db,
                                                DbExpressions.GetStrYearMonth(yearCurrent, monthCurrent));
            var ownedFunds =
                (from c in db.CustFundShares
                    where c.CustomerId == customerId
                          && c.YearMonth == lastFundsHoldingMonth
                    select c).AsEnumerable();

            return ownedFunds;
        }

        /// <summary>
        /// 
        /// Get the previous month from CustFundShares
        /// than currentYearMonStr.
        /// 
        /// Used when buying new shares in currentYearMonStr
        /// 
        /// </summary>
        /// <param name="customerId"></param>*
        /// <param name="db"></param>
        /// <param name="currentYearMonStr"></param>
        /// <returns></returns>
        private string GetFundSharesFromLastPurchase(int customerId, ApplicationDbContext db, string currentYearMonStr) {

            return db.CustFundShares
                .Where(c => c.CustomerId == customerId 
                                /* Before */
                    && string.Compare(c.YearMonth, currentYearMonStr, StringComparison.Ordinal) < 0)
                .OrderByDescending(c => c.YearMonth)
                .Select(c => c.YearMonth)
                .FirstOrDefault();
        }

        /// <summary>
        /// 
        /// Get the last completed month from CustFundShares
        /// before currentYearMonStr.
        /// 
        /// Used when selling the requested month's shares or past vintage investment shares.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="db"></param>
        /// <param name="currentYearMonStr"></param>
        /// <returns></returns>
        private string GetMonthsFundShares(int customerId, ApplicationDbContext db, string currentYearMonStr) {

            string lastCompletedFundSharesMonth = null;

            if (currentYearMonStr != DateTime.UtcNow.ToStringYearMonth()) {

                // Typical case
                lastCompletedFundSharesMonth = db.CustFundShares
                    .Where(c => c.CustomerId == customerId
                                /* Equals */
                                && c.YearMonth == currentYearMonStr)
                    .OrderByDescending(c => c.YearMonth)
                    .Select(c => c.YearMonth)
                    .FirstOrDefault();
            }
            else {

                // Rare case if ever used. Get the previous investment month from present.
                lastCompletedFundSharesMonth = GetFundSharesFromLastPurchase(customerId, db, currentYearMonStr);
            }

            return lastCompletedFundSharesMonth;
        }

    }
}
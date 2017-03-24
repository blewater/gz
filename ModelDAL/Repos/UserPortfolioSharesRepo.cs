using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Threading.Tasks;
using gzDAL.DTO;
using gzDAL.ModelUtil;
using gzDAL.Repos.Interfaces;
using gzDAL.Models;
using NLog;
using Z.EntityFramework.Plus;

namespace gzDAL.Repos {
    class PortfolioPricesDto
    {
        public double ConservativePortfolioPrice;
        public double MediumPortfolioPrice;
        public double HighPortfolioPrice;
        public string YearMonthDay;
    }

    public class UserPortfolioSharesRepo : IUserPortfolioSharesRepo {

        private readonly ApplicationDbContext db;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public UserPortfolioSharesRepo(ApplicationDbContext db) {

            this.db = db;
        }

        /// <summary>
        /// 
        /// ** Unit Test Helper **
        /// 
        /// Calculate the value of a vintage shares on a given month in the past to support Unit testing.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintageYearMonthStr"></param>
        /// <param name="sellOnThisYearMonth"></param>
        /// <returns></returns>
        public VintageSharesDto GetVintageSharesMarketValueOn(int customerId, string vintageYearMonthStr, string sellOnThisYearMonth)
        {
            var vintageShares = GetVintagePortfolioSharesDto(customerId, vintageYearMonthStr);

            SetPortfolioSharesValueOn(vintageShares, sellOnThisYearMonth);

            return vintageShares;
        }

        /// <summary>
        /// 
        /// Get a month's (vintage) portfolio shares.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintageYearMonthStr"></param>
        /// <returns></returns>
        private VintageSharesDto GetVintagePortfolioSharesDto(int customerId, string vintageYearMonthStr) {

            var vintageShares =
                db.InvBalances
                    .Where(c => c.CustomerId == customerId && c.YearMonth == vintageYearMonthStr)
                .Select(b => new VintageSharesDto {
                    LowRiskShares = b.LowRiskShares,
                    MediumRiskShares = b.MediumRiskShares,
                    HighRiskShares = b.HighRiskShares
                })
                .SingleOrDefault();

            return vintageShares;
        }

        /// <summary>
        /// 
        /// Calculate the present value of a vintage shares.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="vintageYearMonthStr"></param>
        /// <returns></returns>
        public VintageSharesDto GetVintageSharesMarketValue(int customerId, string vintageYearMonthStr) {

            var vintageShares = GetVintagePortfolioSharesDto(customerId, vintageYearMonthStr);

            SetPortfolioSharesLatestValue(vintageShares);

            return vintageShares;
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
        /// ** Unit Test Helper **
        /// 
        /// Calculate the latest portfolio prices of the In parameter shares collection.
        /// 
        /// To be used for selling shares not for calculating balances.
        /// 
        /// </summary>
        /// <param name="vintageShares"></param>
        /// <param name="sellOnThisYearMonth"></param>
        private void SetPortfolioSharesValueOn(VintageSharesDto vintageShares, string sellOnThisYearMonth) {
            var latestPortfoliosPrices = GetPortfolioSharePriceOn(sellOnThisYearMonth);

            SetVintageMarketPricing(vintageShares, latestPortfoliosPrices);
        }

        /// <summary>
        /// 
        /// Set vintage market pricing by in argument portfoliosPrices
        /// 
        /// </summary>
        /// <param name="vintageShares"></param>
        /// <param name="portfoliosPricesDto"></param>
        private void SetVintageMarketPricing(VintageSharesDto vintageShares, PortfolioPricesDto portfoliosPricesDto) {

            vintageShares.MarketPrice = vintageShares.LowRiskShares*(decimal) portfoliosPricesDto.ConservativePortfolioPrice +
                                        vintageShares.MediumRiskShares*(decimal) portfoliosPricesDto.MediumPortfolioPrice +
                                        vintageShares.HighRiskShares*(decimal) portfoliosPricesDto.HighPortfolioPrice;
            vintageShares.TradingDay = DbExpressions.GetDtYearMonthDay(portfoliosPricesDto.YearMonthDay);
        }

        /// <summary>
        /// 
        /// Calculate the latest portfolio prices of the In parameter shares collection.
        /// 
        /// To be used for selling shares not for calculating balances.
        /// 
        /// </summary>
        /// <param name="vintageShares"></param>
        private void SetPortfolioSharesLatestValue(VintageSharesDto vintageShares) {

            var latestPortfoliosPrices = GetCachedLatestPortfolioSharePrice();

            SetVintageMarketPricing(vintageShares, latestPortfoliosPrices);
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

            //Find last trade day and include +1 month when the awarding occurs
            var lastMonthDay = new DateTime(year, month, 1).AddMonths(2).AddDays(-1).ToString("yyyyMMdd");
            var lastTradeDay = db.FundPrices
                .Where(fp => fp.FundId == fundId
                             && string.Compare(fp.YearMonthDay, lastMonthDay, StringComparison.Ordinal) <= 0)
                .Select(fp => fp.YearMonthDay)
                .Max();

            string locLastTradeDay = lastTradeDay;
            // Find latest closing price
            fundPriceToRet = db.FundPrices
                .Single(fp => fp.FundId == fundId && fp.YearMonthDay == locLastTradeDay);

            return fundPriceToRet;
        }

        /// <summary>
        /// 
        /// ** Unit Test Helper **
        /// 
        /// Gets the stored portfolio share price on a given month. Supports unit testing.
        /// 
        /// </summary>
        /// <param name="onThisYearMonth"></param>
        /// <returns></returns>
        private PortfolioPricesDto GetPortfolioSharePriceOn(string onThisYearMonth) {
            var onThisYearMonthDay = 
                DbExpressions.GetStrYearEndofMonthDay
                (
                    int.Parse( onThisYearMonth.Substring(0, 4)), 
                    int.Parse( onThisYearMonth.Substring(4, 2))
                );
            // Find latest closing price
            var latestPortfoliosPrices =
                db.PortfolioPrices
                    .Where(pp => String.Compare(pp.YearMonthDay, onThisYearMonthDay, StringComparison.Ordinal) <= 0)
                    .OrderByDescending(p => p.YearMonthDay)
                    .Select(p => new PortfolioPricesDto {
                        ConservativePortfolioPrice = p.PortfolioLowPrice,
                        MediumPortfolioPrice = p.PortfolioMediumPrice,
                        HighPortfolioPrice = p.PortfolioHighPrice,
                        YearMonthDay = p.YearMonthDay
                    })
                    .First();

            return latestPortfoliosPrices;
        }

        /// <summary>
        /// 
        /// Gets the latest stored portfolio share price. Edited from fund shares equivalent.
        /// 
        /// Cached for 2 hours.
        /// 
        /// </summary>
        /// <returns></returns>
        private PortfolioPricesDto GetCachedLatestPortfolioSharePrice() {

            // Find latest closing price
            var latestPortfoliosPrices =
                db.PortfolioPrices
                    .Where(p => p.YearMonthDay == db.PortfolioPrices.Select(pm => pm.YearMonthDay).Max())
                    .Select(p => new PortfolioPricesDto {
                        ConservativePortfolioPrice = p.PortfolioLowPrice,
                        MediumPortfolioPrice = p.PortfolioMediumPrice,
                        HighPortfolioPrice = p.PortfolioHighPrice,
                        YearMonthDay = p.YearMonthDay
                    })
                    .Single();

            return latestPortfoliosPrices;
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
        /// <param name="customerPortfolioId"></param>
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
            int customerPortfolioId) {

            var fundsIQry = GetPortfolioFunds(customerId, customerPortfolioId);

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
        /// <param name="customerPortfolioId"></param>
        /// <returns>The funds IQueryable holding PortfolioDTOs</returns>
        private IQueryable<PortfolioFundDTO> GetPortfolioFunds(int customerId, int customerPortfolioId) {

            return from pf in db.PortFunds
                   //join p in _db.CustPortfolios on pf.PortfolioId equals p.PortfolioId
                   //where p.CustomerId == customerId && p.PortfolioId == customerPortfolioId
                where pf.PortfolioId == customerPortfolioId
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

            var portfolioFundDtos = (
                from c in db.CustFundShares
                where c.CustomerId == customerId
                      && c.SharesNum > 0
                      && c.YearMonth == lastFundsHoldingMonth
                select new PortfolioFundDTO() {
                    FundId = c.FundId,
                    PortfolioId = 0,
                    Weight = 0,
                    SharesNum = c.SharesNum
                })
                .ToDictionary(c => c.FundId);

            SetShareValuesBySoldVintagesOffset(customerId, currentYearMonthStr, portfolioFundDtos);

            return portfolioFundDtos;
        }

        /// <summary>
        /// 
        /// Offset (Decrease) the PortfolioFundDtos Shares number by the shares already sold during this month 
        /// of previous vintages.
        /// 
        /// Post condition: portfolioFundDtos.SharesNum has been decreased by the vintage shares that have been sold.
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="currentYearMonthStr"></param>
        /// <param name="portfolioFundDtos"></param>
        private void SetShareValuesBySoldVintagesOffset(int customerId, string currentYearMonthStr, Dictionary<int, PortfolioFundDTO> portfolioFundDtos) {

            var soldVintageYearMonths = db.InvBalances
                .Where(sv => sv.CustomerId == customerId
                             && sv.SoldYearMonth == currentYearMonthStr)
                .Select(sv => sv.YearMonth).ToList();

            if (soldVintageYearMonths.Count > 0) {

                var vintageFunds = db.CustFundShares
                    .Where(c => c.CustomerId == customerId
                                && c.SharesNum > 0
                                && soldVintageYearMonths.Contains(c.YearMonth))
                    .GroupBy(c => c.FundId)
                    .Select(g => new {FundId = g.Key, NewSharesNum = g.Sum(c => c.NewSharesNum)})
                    .ToDictionary(g => g.FundId);

                // Combine owned with vintage shares
                foreach (var ownedFund in portfolioFundDtos) {
                    var fundId = ownedFund.Key;
                    Trace.Assert(vintageFunds.ContainsKey(fundId));
                    if (vintageFunds.ContainsKey(fundId)) {
                        // Reduce this months total shares by the new shares of the vintage month
                        ownedFund.Value.SharesNum -= vintageFunds[fundId].NewSharesNum ?? 0;
                    }
                }
            }
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
                    && string.Compare(c.YearMonth, currentYearMonStr, StringComparison.Ordinal) 
                        < 0)
                .Max(c => c.YearMonth);
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
                    .Max(c => c.YearMonth);
            }
            else {

                // Rare case if ever used. Get the previous investment month from present.
                lastCompletedFundSharesMonth = GetFundSharesFromLastPurchase(customerId, db, currentYearMonStr);
            }

            return lastCompletedFundSharesMonth;
        }

    }
}
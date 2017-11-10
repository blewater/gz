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

            vintageShares.PresentMarketPrice = vintageShares.LowRiskShares*(decimal) portfoliosPricesDto.ConservativePortfolioPrice +
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
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gzDAL.DTO;
using gzDAL.Models;

namespace gzDAL.Repos.Interfaces {
    public interface IInvBalanceRepo {

        Task<IEnumerable<InvBalance>> CacheLatestBalance(int customerId);
        Task<Tuple<decimal, DateTime?>> GetCachedLatestBalanceTimestampAsync(Task<IEnumerable<InvBalance>> lastBalanceRowTask);
        Task<Decimal> CacheInvestmentReturns(int customerId);
        Task<decimal> GetCachedInvestmentReturnsAsync(Task<decimal> invGainSumTask);
        void SetVintagesMarketPrices(int customerId, IEnumerable<VintageDto> vintages);
        Dictionary<int, PortfolioFundDTO> GetCustomerSharesBalancesForMonth(
            int customerId, 
            int yearCurrent, 
            int monthCurrent, 
            decimal cashToInvest, 
            out decimal monthlyBalance, 
            out decimal invGainLoss,
            out RiskToleranceEnum monthsPortfolioRisk);

        ICollection<VintageDto> GetCustomerVintages(int customerId);

        ICollection<VintageDto> GetCustomerVintagesSellingValue(int customerId);
        ICollection<VintageDto> GetCustomerVintagesSellingValue(int customerId, List<VintageDto> customerVintages);
        void SaveDbSellVintages(int customerId, ICollection<VintageDto> vintages);

        bool SaveDbSellAllCustomerFundsShares(
            int customerId, 
            DateTime updatedDateTimeUtc, 
            out RiskToleranceEnum monthsPortfolioRisk, 
            int yearCurrent = 0,
            int monthCurrent = 0);

        void SaveDbCustomerAllMonthlyBalances(int customerId, string startYearMonthStr = null, string endYearMonthStr = null);
        void SaveDbAllCustomersMonthlyBalances(string startYearMonthStr = null, string endYearMonthStr = null);
        void SaveDbCustomerMonthlyBalance(int customerId, string thisYearMonth);
        void SaveDbCustomerMonthlyBalancesByTrx(int customerId, string[] monthsToProc);
    }
}
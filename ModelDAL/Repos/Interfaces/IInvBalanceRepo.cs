using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gzDAL.DTO;
using gzDAL.Models;

namespace gzDAL.Repos.Interfaces {
    public interface IInvBalanceRepo {

        Task<IEnumerable<InvBalance>> CacheLatestBalanceAsync(int customerId);
        Task<InvBalAmountsRow> GetCachedLatestBalanceTimestampAsync(Task<IEnumerable<InvBalance>> lastBalanceRowTask);
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
            int yearCurrent = 0,
            int monthCurrent = 0);

        void SaveDbCustomerAllMonthlyBalances(int customerId, string startYearMonthStr = null, string endYearMonthStr = null);
        void SaveDbAllCustomersMonthlyBalances(string startYearMonthStr = null, string endYearMonthStr = null);
        void SaveDbCustomerMonthlyBalance(int customerId, string thisYearMonth);
    }
}
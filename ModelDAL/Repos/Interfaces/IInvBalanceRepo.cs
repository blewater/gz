using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gzDAL.DTO;
using gzDAL.Models;

namespace gzDAL.Repos.Interfaces {
    public interface IInvBalanceRepo {

        Task<IEnumerable<InvBalance>> CacheLatestBalanceAsync(int customerId);
        Task<IEnumerable<InvBalance>> CacheLatestBalanceAsyncByMonth(int customerId, string yyyyMm);
        Task<InvBalAmountsRow> GetCachedLatestBalanceTimestampAsync(Task<IEnumerable<InvBalance>> lastBalanceRowTask);
        Task<WithdrawEligibilityDTO> GetWithdrawEligibilityDataAsync(int customerId);
        Task<bool> GetEnabledWithdraw(int customerId);
        void SetVintagesMarketPrices(int customerId, IEnumerable<VintageDto> vintages);
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

        void UpsInvBalance(
            int customerId,
            RiskToleranceEnum userPortfolioRiskSelection,
            int yearCurrent,
            int monthCurrent,
            decimal cashToInvest,
            decimal monthlyBalance,
            decimal invGainLoss,
            decimal lowRiskShares,
            decimal mediumRiskShares,
            decimal highRiskShares,
            decimal begGmBalance,
            decimal deposits,
            decimal withdrawals,
            decimal gamingGainLoss,
            decimal endGmBalance,
            decimal totalCashInvInHold,
            decimal totalCashInvestments,
            decimal totalSoldVintagesValue,
            DateTime updatedOnUtc);
    }
}
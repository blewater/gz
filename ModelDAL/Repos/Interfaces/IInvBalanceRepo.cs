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
        int SetVintagesPresentMarketValue(int customerId, IEnumerable<VintageDto> vintages);
        Task<List<VintageDto>> GetCustomerVintagesAsync(int customerId);
        Task<List<VintageDto>> GetCustomerVintagesSellingValue(int customerId);
        List<VintageDto> GetCustomerVintagesSellingValueNow(int customerId, List<VintageDto> customerVintages);
        ICollection<VintageDto> GetUserVintagesSellingValueOn(int customerId, List<VintageDto> customerVintages,
            string sellOnThisYearMonth);
        void SaveDbSellVintages(int customerId, ICollection<VintageDto> vintages, string sellOnThisYearMonth = "");
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
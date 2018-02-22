using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gzDAL.DTO;
using gzDAL.Models;

namespace gzDAL.Repos.Interfaces {
    public interface IInvBalanceRepo {
        Task<InvBalance> GetCachedLatestBalanceAsync(int customerId);
        Task<InvBalance> GetCachedLatestBalanceAsyncByMonth(int customerId, string yyyyMm);
        InvBalAmountsRow GetLatestBalanceDto(InvBalance lastBalanceRow);
        WithdrawEligibilityDTO GetWithdrawEligibilityData(int customerId);
        Task<Tuple<UserSummaryDTO, ApplicationUser>> GetSummaryDataAsync(int userId);
        int SetAllSelectedVintagesPresentMarketValue(int customerId, IEnumerable<VintageDto> vintages);
        List<VintageDto> GetCustomerVintages(int customerId);
        /// ** Unit Test Helper **
        List<VintageDto> GetCustomerVintagesSellingValueUnitTestHelper(int customerId);
        VintagesWithSellingValues GetCustomerVintagesSellingValueNow(int customerId, List<VintageDto> customerVintages);
        SoldVintagesAmounts GetSoldVintagesAmounts(int userId);
        ICollection<VintageDto> GetUserVintagesSellingValueOn(int customerId, List<VintageDto> customerVintages,
            string sellOnThisYearMonth);
        ICollection<VintageDto> SaveDbSellAllSelectedVintagesInTransRetry(
            int customerId, 
            ICollection<VintageDto> vintages, 
            bool sendEmail2Admins, 
            string queueAzureConnString,
            string queueName,
            string emailAdmins, 
            string gmailUser, 
            string gmailPwd, 
            string sellOnThisYearMonth = "");
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
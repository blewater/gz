using System;
using System.Collections.Generic;
using gzDAL.DTO;

namespace gzDAL.Repos.Interfaces
{
    public interface IInvBalanceRepo {
        DateTime GetLastUpdatedDateTime(int customerId);
        Dictionary<int, PortfolioFundDTO> GetCustomerSharesBalancesForMonth(int customerId, int yearCurrent, int monthCurrent, decimal cashToInvest, out decimal monthlyBalance, out decimal invGainLoss);

        ICollection<VintageDto> GetCustomerVintages(int customerId);

        ICollection<VintageDto> GetCustomerVintagesSellingValue(int customerId);

        ICollection<VintageDto> SaveDbSellVintages(int customerId, ICollection<VintageDto> vintages);

        bool SaveDbSellAllCustomerFundsShares(int customerId, DateTime updatedDateTimeUtc, int yearCurrent = 0,
            int monthCurrent = 0);

        void SaveDbCustomerAllMonthlyBalances(int customerId, string startYearMonthStr = null, string endYearMonthStr = null);
        void SaveDbAllCustomersMonthlyBalances(string startYearMonthStr = null, string endYearMonthStr = null);
        void SaveDbCustomerMonthlyBalance(int customerId, string thisYearMonth);
        void SaveDbCustomerMonthlyBalancesByTrx(int customerId, string[] monthsToProc);
    }
}
using gzDAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gzDAL.DTO;

namespace gzDAL.Repos.Interfaces
{
    public interface IGzTransactionRepo {

        decimal GetTotalDeposit(int customerId);

        decimal LastInvestmentAmount(int customerId, string yearMonthStr);

        Task<decimal> GetLastInvestmentAmountAsync(int userId);

        Task<decimal> GetTotalInvestmentsAmountAsync(int userId);

        Task<decimal> GetTotalWithdrawalsAmountAsync(int userId);

        IEnumerable<int> GetActiveCustomers(string startYearMonthStr, string endYearMonthStr);

        bool GetLiquidationTrxCount(int customerId, int yearCurrent, int monthCurrent);

        DateTime GetSoldPortfolioTimestamp(int customerId, int yearCurrent, int monthCurrent);

        decimal GetWithdrawnFees(decimal liquidationAmount);

        Task<WithdrawEligibilityDTO> GetWithdrawEligibilityDataAsync(int customerId);

        Task<bool> GetEnabledWithdraw(int customerId);

        void SaveDbGmTransaction(int customerId, GmTransactionTypeEnum gzTransactionType, decimal amount,
            DateTime createdOnUtc);

        void SaveDbGzTransaction(int customerId, GzTransactionTypeEnum gzTransactionType, decimal amount,
            DateTime createdOnUtc);

        void SaveDbPlayingLoss(int customerId, decimal totPlayinLossAmount, DateTime createdOnUtc);

        void SaveDbTransferToGamingAmount(int customerId, decimal investmentAmount, DateTime createdOnUtc);

        decimal SaveDbLiquidatedPortfolioWithFees(int customerId, decimal liquidationAmount, GzTransactionTypeEnum sellingJournalTypeReason, DateTime createdOnUtcout, out decimal lastInvestmentCredit);

        void SaveDbSellVintages(int customerId, ICollection<VintageDto> vintages);
    }
}
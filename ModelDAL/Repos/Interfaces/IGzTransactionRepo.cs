using gzDAL.Models;
using System;
using System.Collections.Generic;

namespace gzDAL.Repos.Interfaces
{
    public interface IGzTransactionRepo {

        IEnumerable<int> GetActiveCustomers(string thisYearMonth);

        bool GetLiquidationTrxCount(int customerId, int yearCurrent, int monthCurrent);

        DateTime GetSoldPortfolioTimestamp(int customerId, int yearCurrent, int monthCurrent);

        decimal GetWithdrawnFees(decimal liquidationAmount);

        void SaveDbGzTransaction(int customerId, GzTransactionJournalTypeEnum gzTransactionType, decimal amount, DateTime createdOnUtc);

        void SaveDbInvWithdrawalAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUtc);

        void SaveDbPlayingLoss(int customerId, decimal totPlayinLossAmount, decimal creditPcnt, DateTime createdOnUtc);

        void SaveDbTransferToGamingAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUtc);

        decimal SaveDbLiquidatedPortfolioWithFees(int customerId, decimal liquidationAmount, GzTransactionJournalTypeEnum sellingJournalTypeReason, DateTime createdOnUtc);
    }
}
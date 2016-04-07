using gzDAL.Models;
using System;

namespace gzDAL.Repos.Interfaces
{
    public interface IGzTransactionRepo {

        decimal GetWithdrawnFees(decimal liquidationAmount);

        void SaveDbGzTransaction(int customerId, GzTransactionJournalTypeEnum gzTransactionType, decimal amount, DateTime createdOnUtc);

        void SaveDbInvWithdrawalAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUtc);

        void SaveDbPlayingLoss(int customerId, decimal totPlayinLossAmount, decimal creditPcnt, DateTime createdOnUtc);

        void SaveDbTransferToGamingAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUtc);

        decimal SaveDbLiquidatedPortfolioWithFees(int customerId, decimal liquidationAmount, GzTransactionJournalTypeEnum sellingJournalTypeReason, DateTime createdOnUtc);
    }
}
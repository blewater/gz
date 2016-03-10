using gzWeb.Models;
using System;

namespace gzWeb.Repo.Interfaces
{
    public interface IGzTransactionRepo
    {
        void SaveDBGzTransaction(int customerId, TransferTypeEnum gzTransactionType, decimal amount, DateTime createdOnUTC);
        void SaveDBInvWithdrawalAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUTC);
        void SaveDBPlayingLoss(int customerId, decimal totPlayinLossAmount, decimal creditPcnt, DateTime createdOnUTC);
        void SaveDBTransferToGamingAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUTC);
    }
}
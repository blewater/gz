using gzDAL.Models;
using System;

namespace gzDAL.Repos.Interfaces
{
    public interface IGzTransactionRepo
    {
        void SaveDBGzTransaction(int customerId, TransferTypeEnum gzTransactionType, decimal amount, DateTime createdOnUTC);
        void SaveDBInvWithdrawalAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUTC);
        void SaveDBPlayingLoss(int customerId, decimal totPlayinLossAmount, decimal creditPcnt, DateTime createdOnUTC);
        void SaveDBTransferToGamingAmount(int customerId, decimal withdrawnAmount, DateTime createdOnUTC);
    }
}
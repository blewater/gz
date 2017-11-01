using gzDAL.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using gzDAL.DTO;

namespace gzDAL.Repos.Interfaces
{
    public interface IGzTransactionRepo {

        decimal LastInvestmentAmount(int customerId, string yearMonthStr);

        DateTime GetSoldPortfolioTimestamp(int customerId, int yearCurrent, int monthCurrent);

        FeesDto GetWithdrawnFees(decimal vintageCashInvestment,string vintageYearMonthStr, decimal liquidationAmount);

        void SaveDbGzTransaction(int customerId, GzTransactionTypeEnum gzTransactionType, decimal amount, string trxYearMonth, DateTime createdOnUtc);

        void SaveDbPlayingLoss(int customerId, decimal totPlayinLossAmount, string trxYearMonth, DateTime createdOnUtc, decimal begGmBalance, decimal deposits, decimal withdrawals, decimal gainLoss, decimal endGmbalance);
    }
}
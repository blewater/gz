using System.Collections.Generic;
using gzDAL.DTO;

namespace gzDAL.Repos.Interfaces
{
    public interface IInvBalanceRepo
    {
        Dictionary<int, PortfolioFundDTO> GetCalcMonthlyBalancesForCustomer(int custId, int yearCurrent, int monthCurrent, decimal cashToInvest, out decimal monthlyBalance, out decimal invGainLoss);
        void SaveDBCustomerMonthlyBalancesByTrx(int customerId, string[] monthsToProc);
        void SaveDBCustomerMonthBalanceByCashInv(int customerId, int year, int month, decimal cashToInvest);
    }
}
using System.Collections.Generic;
using gzDAL.DTO;

namespace gzDAL.Repos.Interfaces
{
    public interface IInvBalanceRepo
    {
        Dictionary<int, PortfolioFundDTO> GetCalcMonthlyBalancesForCustomer(int customerId, int yearCurrent, int monthCurrent, decimal cashToInvest, out decimal monthlyBalance, out decimal invGainLoss);
        void SaveDbCustomerMonthlyBalancesByTrx(int customerId, string[] monthsToProc);
        void SaveDbCustomerMonthBalanceByCashInv(int customerId, int year, int month, decimal cashToInvest);
    }
}
using System.Collections.Generic;

namespace gzDAL.Repos.Interfaces
{
    public interface IInvBalanceRepo
    {
        Dictionary<int, CustFundShareRepo.PortfolioFundDTO> GetCalcMonthlyBalancesForCustomer(int custId, int year, int month, decimal cashToInvest, out decimal monthlyBalance, out decimal invGainLoss);
        void SaveCustomerTrxsBalances(int customerId);
        void SaveCustomerTrxsBalances(int custId, uint[] monthsToProc);
        void SaveDBbyTrxMonthlyBalanceForCustomer(int custId, int year, int month, decimal cashToInvest);
    }
}
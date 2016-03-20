using System.Collections.Generic;

namespace gzDAL.Repos.Interfaces
{
    public interface IInvBalanceRepo
    {
        Dictionary<int, CustFundShareRepo.PortfolioFundDTO> GetCalcMonthlyBalancesForCustomer(int custId, int year, int month, decimal cashToInvest, out decimal monthlyBalance, out decimal invGainLoss);
        void SaveCustTrxsBalances(int custId);
        void SaveDBbyTrxMonthlyBalanceForCustomer(int custId, int year, int month, decimal cashToInvest);
    }
}
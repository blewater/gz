﻿using System.Collections.Generic;

namespace gzDAL.Repos.Interfaces
{
    public interface IInvBalanceRepo
    {
        Dictionary<int, CustFundShareRepo.PortfolioFundDTO> GetCalcMonthlyBalancesForCustomer(int custId, int year, int month, decimal cashToInvest, out decimal monthlyBalance, out decimal invGainLoss);
        void SaveDBCustomerMonthlyBalancesByTrx(int customerId, string[] monthsToProc);
        void SaveDBCustomerMonthBalanceByCashInv(int customerId, int year, int month, decimal cashToInvest);
    }
}
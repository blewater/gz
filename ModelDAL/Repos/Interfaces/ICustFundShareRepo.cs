using gzDAL.Models;
using System;
using System.Collections.Generic;

namespace gzDAL.Repos.Interfaces
{
    public interface ICustFundShareRepo
    {
        Dictionary<int, CustFundShareRepo.PortfolioFundDTO> GetCalcCustMonthlyFundShares(int customerId, decimal netInvAmount, int year, int month);
        void SaveDBCustomerPurchasedFundShares(int customerId, Dictionary<int, CustFundShareRepo.PortfolioFundDTO> fundsShares, int year, int month, DateTime updatedOnUTC);
    }
}
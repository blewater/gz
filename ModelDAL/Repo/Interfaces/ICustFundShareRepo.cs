using gzWeb.Models;
using System;
using System.Collections.Generic;

namespace gzWeb.Repo.Interfaces
{
    public interface ICustFundShareRepo
    {
        Dictionary<int, CustFundShareRepo.PortfolioFundDTO> GetCalcCustMonthlyFundShares(int customerId, decimal netInvAmount, int year, int month);
        void SaveDbCustPurchasedFundShares(ApplicationDbContext db, int customerId, Dictionary<int, CustFundShareRepo.PortfolioFundDTO> fundsShares, int year, int month, DateTime updatedOnUTC);
    }
}
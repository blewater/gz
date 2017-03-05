﻿using gzDAL.Models;
using System;
using System.Collections.Generic;
using gzDAL.DTO;

namespace gzDAL.Repos.Interfaces
{
    public interface ICustFundShareRepo {
        VintageSharesDto GetVintageSharesMarketValue(int customerId, string yearMonthStr);

        Dictionary<int, PortfolioFundDTO> GetMonthlyFundSharesAfterBuyingSelling(
            int customerId, 
            decimal cashInvestmentAmount, 
            int year, 
            int month, 
            out RiskToleranceEnum monthsPortfolioRisk);

        void SaveDbMonthlyCustomerFundShares(bool boughtShares, int customerId, 
            Dictionary<int, PortfolioFundDTO> fundsShares, int year, int month, DateTime updatedOnUtc);
    }
}
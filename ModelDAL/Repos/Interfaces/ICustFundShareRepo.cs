﻿using gzDAL.Models;
using System;
using System.Collections.Generic;
using gzDAL.DTO;

namespace gzDAL.Repos.Interfaces
{
    public interface ICustFundShareRepo {
        IEnumerable<CustFundShareDto> GetMonthsBoughtFundsValue(int customerId, int yearCurrent, int monthCurrent);

        Dictionary<int, PortfolioFundDTO> GetMonthlyFundSharesAfterBuyingSelling(int customerId, decimal cashInvestmentAmount, int year, int month);

        void SaveDbMonthlyCustomerFundShares(bool boughtShares, int customerId, 
            Dictionary<int, PortfolioFundDTO> fundsShares, int year, int month, DateTime updatedOnUtc);
    }
}
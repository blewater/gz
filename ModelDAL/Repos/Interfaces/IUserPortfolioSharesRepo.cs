using gzDAL.Models;
using System;
using System.Collections.Generic;
using gzDAL.DTO;

namespace gzDAL.Repos.Interfaces
{
    public interface IUserPortfolioSharesRepo {

        VintageSharesDto GetVintageSharesMarketValueOn(
            int customerId, 
            string vintageYearMonthStr,
            string sellOnThisYearMonth);

        VintageSharesDto GetVintageSharesMarketValue(
            int customerId, 
            string vintageYearMonthStr,
            PortfolioPricesDto portfolioPrices);

        PortfolioPricesDto GetCachedLatestPortfolioSharePrice();
    }
}
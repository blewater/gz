using System.Collections.Generic;
using gzDAL.DTO;
using gzDAL.Models;

namespace gzDAL.Repos.Interfaces
{
    public interface IPortfolioRepository {

        IEnumerable<PortfolioReturnsDTO> GetPortfolioReturns();

        IList<string> GetPortfolioRetLines();

        Portfolio GetCurrentCustomerPortfolio(int customerId);

        Portfolio GetNextMonthsCustomerPortfolio(int customerId);
    }
}
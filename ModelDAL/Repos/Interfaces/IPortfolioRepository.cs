using System.Collections.Generic;
using gzDAL.DTO;
using gzDAL.Models;

namespace gzDAL.Repos.Interfaces
{
    public interface IPortfolioRepository {

        List<PortfolioReturnsDTO> GetPortfolioReturns();

        IList<string> GetPortfolioRetLines();
    }
}
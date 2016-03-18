using System.Collections.Generic;

namespace gzDAL.Repos.Interfaces
{
    public interface IPortfolioRepository
    {
        IList<string> GetPortfolioRetLines();
    }
}
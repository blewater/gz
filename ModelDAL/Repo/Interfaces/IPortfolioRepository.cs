using System.Collections.Generic;

namespace gzWeb.Repo
{
    public interface IPortfolioRepository
    {
        IList<string> GetPortfolioRetLines();
    }
}
using System.Collections.Generic;
using gzDAL.ModelsUtil;
using gzDAL.Models;

namespace gzDAL.Repos.Interfaces
{
    public interface IFundRepo
    {
        List<FundQuote> SaveDbDailyFundClosingPrices();
    }
}
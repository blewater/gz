using System.Collections.Generic;
using gzDAL.ModelsUtil;
using gzDAL.Models;

namespace gzDAL.Repos.Interfaces
{
    public interface IFundRepo
    {
        Dictionary<string, float> GetFundsPrices(int year, int month, int day);
        List<FundQuote> SaveDbDailyFundClosingPrices();
    }
}
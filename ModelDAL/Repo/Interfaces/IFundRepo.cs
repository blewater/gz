using System.Collections.Generic;
using gzWeb.Models.Util;
using gzWeb.Models;

namespace gzWeb.Repo.Interfaces
{
    public interface IFundRepo
    {
        Dictionary<string, float> GetFundsPrices(ApplicationDbContext db, int year, int month, int day);
        List<FundQuote> SaveDBDailyFundClosingPrices();
    }
}
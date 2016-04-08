using System.Collections.Generic;
using gzDAL.ModelsUtil;

namespace gzDAL.Repos.Interfaces
{
    public interface ICurrencyRateRepo
    {
        List<CurrencyQuote> SaveDbDailyCurrenciesRates();
    }
}
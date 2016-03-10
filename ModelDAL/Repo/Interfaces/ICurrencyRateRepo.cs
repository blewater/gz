using System.Collections.Generic;
using gzWeb.Models.Util;

namespace gzWeb.Repo.Interfaces
{
    public interface ICurrencyRateRepo
    {
        List<CurrencyQuote> SaveDBDailyCurrenciesRates();
    }
}
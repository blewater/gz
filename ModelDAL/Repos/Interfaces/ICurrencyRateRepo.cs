using System.Collections.Generic;
using gzDAL.Models;
using gzDAL.ModelsUtil;

namespace gzDAL.Repos.Interfaces
{
    public interface ICurrencyRateRepo
    {
        List<CurrencyQuote> SaveDbDailyCurrenciesRates();
        CurrencyRate GetLastCurrencyRate(string currency);
    }
}
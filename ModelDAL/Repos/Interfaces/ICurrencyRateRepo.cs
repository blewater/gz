using System;
using System.Collections.Generic;
using gzDAL.Models;
using gzDAL.ModelsUtil;

namespace gzDAL.Repos.Interfaces
{
    public interface ICurrencyRateRepo
    {
        List<CurrencyQuote> SaveDbDailyCurrenciesRates();
        decimal GetLastCurrencyRateFromUSD(string currencyCodeTo);
        decimal GetLastCurrencyRateToUSD(string currencyCodeFrom);
    }
}
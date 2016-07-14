using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using gzDAL.ModelsUtil;
using System.Data.Entity.Migrations;
using System.Runtime.Caching;
using gzDAL.Models;
using gzDAL.Repos.Interfaces;
using Z.EntityFramework.Plus;


namespace gzDAL.Repos
{
    public class CurrencyRateRepo : ICurrencyRateRepo
    {
        private readonly ApplicationDbContext db;

        public CurrencyRateRepo(ApplicationDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// 
        /// Get latest currency exchange rate from USD to input parameter CurrencyCodeTo.
        /// 
        /// </summary>
        /// <param name="currencyCodeTo"></param>
        /// <returns></returns>
        public decimal GetLastCurrencyRateFromUSD(string currencyCodeTo)
        {
            var key = $"USD{currencyCodeTo.ToUpperInvariant()}";

            var rate = db.CurrencyRates
                .Where(x => x.FromTo == key
                            && x.TradeDateTime == db.CurrencyRates
                                .Where(r => r.FromTo == x.FromTo)
                                .Select(r => r.TradeDateTime).Max())
                .Select(r => r.rate)
                .DeferredSingle()
                .FromCacheAsync()
                .Result;

            return rate;
        }

        /// <summary>
        /// 
        /// Get latest currency exchange rate to convert from input parameter CurrencyCodeFrom to USD.
        /// 
        /// </summary>
        /// <param name="currencyCodeFrom"></param>
        /// <returns></returns>
        public decimal GetLastCurrencyRateToUSD(string currencyCodeFrom) {
            var code = String.Format("{0}USD", currencyCodeFrom.ToUpperInvariant());
            return db.CurrencyRates
                .Where(x => x.FromTo == code)
                .OrderByDescending(x => x.TradeDateTime)
                .Select(r => r.rate)
                .FirstOrDefault();
        }

        public List<CurrencyQuote> SaveDbDailyCurrenciesRates() {

            var currenciesList = db.CurrenciesListX.Select(c => c.From + c.To).ToList();

            var quotes = currenciesList.Select(c => new CurrencyQuote { CurrFromTo = c }).ToList();

            List<CurrencyQuote> retVal = YQLFundCurrencyInq.FetchCurrencyRates(quotes);
                
            foreach (var q in retVal) {

                db.CurrencyRates.AddOrUpdate(
                    c => new { c.FromTo, c.TradeDateTime },
                    new CurrencyRate {
                        rate = q.Rate,
                        FromTo = q.CurrFromTo,
                        TradeDateTime = q.TradeDateTime.Value,
                        UpdatedOnUTC = DateTime.UtcNow,
                    });
            }

            db.SaveChanges();

            return retVal;
        }
    }
}

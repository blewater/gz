using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using gzWeb.Models.Util;
using System.Data.Entity.Migrations;
using gzWeb.Repo.Interfaces;
using gzWeb.Models;

namespace gzWeb.Repo {
    public class CurrencyRateRepo : ICurrencyRateRepo
    {
        private readonly ApplicationDbContext db;

        public CurrencyRateRepo(ApplicationDbContext db)
        {
            this.db = db;
        }

        public List<CurrencyQuote> SaveDBDailyCurrenciesRates() {

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

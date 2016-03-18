using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using gzDAL.ModelsUtil;
using System.Data.Entity.Migrations;
using gzDAL.Models;
using gzDAL.Repos.Interfaces;

namespace gzDAL.Repos
{
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

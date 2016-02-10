using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using gzWeb.Helpers;
using System.Data.Entity.Migrations;

namespace gzWeb.Models {
    public class CurrencyRateRepo {

        public List<CurrencyQuote> AddDailyCurrenciesRates() {

            List<CurrencyQuote> retVal = null;

            using (var db = new ApplicationDbContext()) {

                var currenciesList = db.CurrenciesListX.Select(c => c.From + c.To).ToList();

                var quotes = currenciesList.Select(c => new CurrencyQuote { CurrFromTo = c }).ToList();

                retVal = YQLFundCurrencyInq.FetchCurrencyRates(quotes);

                try {
                    foreach (var q in quotes) {

                        db.CurrencyRates.AddOrUpdate(
                            c => new { c.FromTo, c.TradeDateTime },
                            new CurrencyRate {
                                rate = q.Rate,
                                FromTo = q.CurrFromTo,
                                TradeDateTime = q.TradeDateTime.Value,
                                UpdatedOnUTC = DateTime.UtcNow,
                            }
                            );
                    }

                    db.SaveChanges();

                } catch (Exception ex) {
                    var exception = ex.Message;
                    throw ex;
                }
            }
            return retVal;
        }
    }
}

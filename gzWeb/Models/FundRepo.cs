using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gzWeb.Helpers;
using System.Data.Entity.Migrations;

namespace gzWeb.Models {
    public class FundRepo {

        public void AddDailyFundClosingPrices() {

            using (var db = new ApplicationDbContext()) {

                var fundsList = db.Funds.Select(f => f.Symbol).ToList();

                var quotes = fundsList.Select(f => new PQuote { Symbol = f }).ToList();

                YahooStockEngine.Fetch(quotes);

                using (var dbContextTransaction = db.Database.BeginTransaction()) {

                    try {
                        foreach (var q in quotes) {

                            db.FundPrices.AddOrUpdate(
                                f => new { f.FundId, f.YearMonthDay },
                                new FundPrice {
                                    FundId = db.Funds.Where(f => f.Symbol == q.Symbol).Select(f => f.Id).FirstOrDefault(),
                                    ClosingPrice = q.LastTradePrice.Value,
                                    YearMonthDay = q.LastTradeDate.Value.Year.ToString("0000")
                                        + q.LastTradeDate.Value.Month.ToString("00")
                                        + q.LastTradeDate.Value.Day.ToString("00"),
                                    UpdatedOnUTC = q.UpdatedOnUTC
                                }
                                );
                        }

                        db.SaveChanges();
                        dbContextTransaction.Commit();

                    } catch (Exception ex) {
                        var exception = ex.Message;
                        dbContextTransaction.Rollback();
                        throw ex;
                    }
                }
            }
        }
    }
}
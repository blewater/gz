using System.Collections.Generic;
using System.Linq;
using gzDAL.ModelsUtil;
using System.Data.Entity.Migrations;
using gzDAL.Repos.Interfaces;
using gzDAL.Models;

namespace gzDAL.Repos
{
    public class FundRepo : IFundRepo
    {
        private readonly ApplicationDbContext db;
        public FundRepo(ApplicationDbContext db)
        {
            this.db = db;
        }

        /// <summary>
        /// Retrieve the last completed day's closing price for our funds.
        /// If it's a weekday before 4pm (US Eastern Time?) then we get the closing price of the previous weekday.
        /// For example on Monday 12pm, we go to Friday's closing price if Friday was not a western holiday.
        /// </summary>
        /// <returns></returns>
        public List<FundQuote> SaveDbDailyFundClosingPrices() {

            var fundsList = db.Funds.Select(f => f.Symbol).ToList();

            var quotes = fundsList.Select(f => new FundQuote { Symbol = f }).ToList();

            List<FundQuote> fundQuotes = YQLFundCurrencyInq.FetchFundsQuoteInfo(quotes);

            foreach (var q in fundQuotes) {

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
            
            return fundQuotes;
        }
    }
}
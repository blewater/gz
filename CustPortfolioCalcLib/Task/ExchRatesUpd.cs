using gzDAL.Models;
using gzDAL.Repos;

namespace gzCpcLib.Task {

    /// <summary>
    /// 
    /// Request latest currency rates from YApi and save to Database
    /// 
    /// </summary>
    public class ExchRatesUpd : CpcTask {

        public override void DoTask() {

            var currencyRateRepo = new CurrencyRateRepo(new ApplicationDbContext());
            var quotes = currencyRateRepo.SaveDBDailyCurrenciesRates();

        }

    }
}

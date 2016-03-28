using gzDAL.Models;
using gzDAL.Repos;

namespace gzCpcLib.Task {
    public class CurrencyRatesUpdDb : CpcTask {

        public override void DoTask() {

            var currencyRateRepo = new CurrencyRateRepo(new ApplicationDbContext());
            var quotes = currencyRateRepo.SaveDBDailyCurrenciesRates();

        }

    }
}

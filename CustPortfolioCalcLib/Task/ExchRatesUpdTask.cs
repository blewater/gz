using gzDAL.Models;
using gzDAL.Repos;

namespace gzCpcLib.Task {

    /// <summary>
    /// 
    /// Request latest currency rates from YApi and save to Database
    /// 
    /// </summary>
    public class ExchRatesUpdTask : CpcTask {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override void DoTask() {

            var currencyRateRepo = new CurrencyRateRepo(new ApplicationDbContext());
            var quotes = currencyRateRepo.SaveDbDailyCurrenciesRates();

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using gzDAL.Models;
using gzDAL.Repos;
using System.Reactive.Linq;

namespace gzCpcLib {
    public class CurrencyRatesUpdDb : CpcTask {

        public override void DoTask() {

            var currencyRateRepo = new CurrencyRateRepo(new ApplicationDbContext());
            var quotes = currencyRateRepo.SaveDBDailyCurrenciesRates();

        }

    }
}

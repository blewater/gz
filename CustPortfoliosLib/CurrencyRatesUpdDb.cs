using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using gzDAL.Models;
using gzDAL.Repos;
using System.Reactive.Linq;

namespace cpc {
    public class CurrencyRatesUpdDb : ObservableTask {

        public override void DoWork() {

            var currencyRateRepo = new CurrencyRateRepo(new ApplicationDbContext());
            var quotes = currencyRateRepo.SaveDBDailyCurrenciesRates();

        }

    }
}

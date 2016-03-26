using System;
using System.Reactive;
using System.Reactive.Linq;
using gzDAL.Models;
using gzDAL.Repos;

namespace cpc {
    public class FundMarketUpdDb : ObservableTask {

        public override void DoWork() {

            var fundRepo = new FundRepo(new ApplicationDbContext());
            fundRepo.SaveDBDailyFundClosingPrices();
        }
    }
}
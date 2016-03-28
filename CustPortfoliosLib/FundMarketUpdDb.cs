using System;
using System.Reactive;
using System.Reactive.Linq;
using gzDAL.Models;
using gzDAL.Repos;

namespace gzCpcLib {
    public class FundMarketUpdDb : CpcTask {

        public override void DoTask() {

            var fundRepo = new FundRepo(new ApplicationDbContext());
            fundRepo.SaveDBDailyFundClosingPrices();
        }
    }
}
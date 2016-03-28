using gzDAL.Models;
using gzDAL.Repos;

namespace gzCpcLib.Task {

    public class FundMarketUpdDb : CpcTask {

        public override void DoTask() {

            var fundRepo = new FundRepo(new ApplicationDbContext());
            fundRepo.SaveDBDailyFundClosingPrices();
        }
    }
}
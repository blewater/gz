using gzDAL.Models;
using gzDAL.Repos;

namespace gzCpcLib.Task {

    /// <summary>
    /// 
    /// Request latest stock market pricing info for the interested Funds
    /// 
    /// </summary>
    public class FundsUpd : CpcTask {

        public override void DoTask() {

            var fundRepo = new FundRepo(new ApplicationDbContext());
            fundRepo.SaveDBDailyFundClosingPrices();
        }
    }
}
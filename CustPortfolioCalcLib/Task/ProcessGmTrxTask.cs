using gzDAL.Models;
using gzDAL.Repos;

namespace gzCpcLib.Task {
    /// <summary>
    /// 
    /// Request latest stock market pricing info for the interested Funds
    /// 
    /// </summary>
    public class ProcessGmTrxTask : CpcTask {

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public override void DoTask() {

            _logger.Info("Entered ProcessGmTrxTask.DoTask()");

            //var fundRepo = new FundRepo(new ApplicationDbContext());
            //fundRepo.SaveDbDailyFundClosingPrices();
        }
    }
}
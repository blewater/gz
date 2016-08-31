using System.Data.Entity;
using gzDAL.Models;
using gzDAL.Repos;
using System.Configuration;
using System.Linq;
using NLog;

namespace gzCpcLib.Task {

    /// <summary>
    /// 
    /// Request latest stock market pricing info for the interested Funds
    /// 
    /// </summary>
    public class FundsUpdTask : CpcTask {

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public override void DoTask() {

            _logger.Info("Entered FundsUpdTask.DoTask()");

            // Kill any code first migration checks
            Database.SetInitializer<ApplicationDbContext>(null);

            var fundRepo = new FundRepo(new ApplicationDbContext());
            fundRepo.SaveDbDailyFundClosingPrices();
        }
    }
}
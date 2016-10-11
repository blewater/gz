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
        private ApplicationDbContext _db;

        public FundsUpdTask() {
            _logger.Trace("Entered FundsUpdTask()");

            this._db = new ApplicationDbContext();
        }

        public FundsUpdTask(bool isProd) {
            _logger.Trace("Entered FundsUpdTask(isProd:{0}) ", isProd);

            if (isProd) {
                this._db = new ApplicationDbContext("gzProdDb");
            }
            else {
                this._db = new ApplicationDbContext("gzDevDb");

                // Kill any code first migration checks on Dev which tracks migrations
                Database.SetInitializer<ApplicationDbContext>(null);
            }
        }

        public override void DoTask() {

            _logger.Info("Retrieving last day's stock prices...");

            new FundRepo(_db).SaveDbDailyFundClosingPrices();
        }
    }
}
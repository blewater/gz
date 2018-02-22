using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;
using FluentScheduler;
using gzDAL.Models;
using gzDAL.Repos;
using NLog;
using Z.EntityFramework.Plus;

namespace gzWeb.Utilities  {
    /// <summary>
    /// 
    /// https://github.com/fluentscheduler/FluentScheduler
    /// 
    /// Employed technique for caching global data applicable to all users.
    /// 
    /// </summary>
    public class GlobalCacheScheduledJob : IJob, IRegisteredObject {

        private static Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly object _lock = new object();

        private bool _shuttingDown;

        public GlobalCacheScheduledJob() {

            _logger.Trace("Registering GlobalCacheScheduledJob.");

            // Register this job with the hosting environment.
            // Allows for a more graceful stop of the job, in the case of Azure shutting down.
            HostingEnvironment.RegisterObject(this);
        }

        public void Execute() {
            lock (_lock) {
                if (_shuttingDown)
                    return;

                _logger.Trace("Execute() of GlobalCacheScheduledJob.");

                CacheGlobalData();
            }
        }

        public void Stop(bool immediate) {
            // Locking here will wait for the lock in Execute to be released until this code can continue.
            lock (_lock) {
                _shuttingDown = true;
            }

            HostingEnvironment.UnregisterObject(this);
        }

        /// <summary>
        /// 
        /// Cache global database data applicable to all customers
        /// 
        /// </summary>
        private void CacheGlobalData() {

            try {

                //-------- Global Caching Configuration of Single GzConfiguration Row
                var db = new ApplicationDbContext();

                // Cache configuration object
                var _ = new ConfRepo(db).GetConfRow();

                //-------- Cache Fund prices for 2 hours
                var __ = db.PortfolioPrices
                    .Where(p => p.YearMonthDay == db.PortfolioPrices.Select(pm => pm.YearMonthDay).Max())
                    .Select(p => new {
                        p.PortfolioLowPrice,
                        p.PortfolioMediumPrice,
                        p.PortfolioHighPrice,
                        p.YearMonthDay
                    })
                    .FromCacheAsync(DateTime.UtcNow.AddHours(2))
                    .ConfigureAwait(false);
            }
            catch (Exception ex) {
                _logger.Error(ex, "CacheGlobalData()");
            }
        }


    }
}
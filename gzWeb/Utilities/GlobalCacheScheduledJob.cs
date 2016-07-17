using System;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Hosting;
using FluentScheduler;
using gzDAL.Models;
using Z.EntityFramework.Plus;

namespace gzWeb.Utilities  {
    /// <summary>
    /// 
    /// https://github.com/fluentscheduler/FluentScheduler
    /// 
    /// </summary>
    public class GlobalCacheScheduledJob : IJob, IRegisteredObject {
        private readonly object _lock = new object();

        private bool _shuttingDown;

        public GlobalCacheScheduledJob() {

            // Register this job with the hosting environment.
            // Allows for a more graceful stop of the job, in the case of Azure shutting down.
            HostingEnvironment.RegisterObject(this);
        }

        public void Execute() {
            lock (_lock) {
                if (_shuttingDown)
                    return;

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

            //-------- Global Caching Configuration of Single GzConfiguration Row
            var db = new ApplicationDbContext();
            db.GzConfigurations

                // 7 days cache
                .FromCacheAsync(DateTime.UtcNow.AddDays(7));

            //-------- Cache Fund prices for 2 hours
            db.FundPrices
                .Where(f => f.YearMonthDay == db.FundPrices.Select(d => f.YearMonthDay).Max())
                .Select(f => new { f.Id, f.ClosingPrice })
                .FromCacheAsync(DateTime.UtcNow.AddHours(2));

            //-------- Cache Currencies for 2 hours

            db.CurrencyRates
                .Where(x => x.TradeDateTime == db.CurrencyRates.Select(r => r.TradeDateTime).Max())
                .Select(x => new {x.FromTo, x.rate})
                .FromCacheAsync(DateTime.UtcNow.AddHours(2));
        }


    }
}
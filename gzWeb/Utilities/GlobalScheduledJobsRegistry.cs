using FluentScheduler;

namespace gzWeb.Utilities {

    /// <summary>
    /// 
    /// Schedule to run every 1 hour
    /// 
    /// </summary>
    public class GlobalScheduledJobsRegistry : Registry {

        public GlobalScheduledJobsRegistry() {

            // Run every 1 hour + 7 minutes: don't fall within cache limits
            Schedule<GlobalCacheScheduledJob>().ToRunNow().AndEvery(67).Minutes();
        }
        
    }
}
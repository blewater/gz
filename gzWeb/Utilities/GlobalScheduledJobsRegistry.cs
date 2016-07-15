using FluentScheduler;

namespace gzWeb.Utilities {

    /// <summary>
    /// 
    /// Schedule to run every 1 hour
    /// 
    /// </summary>
    public class GlobalScheduledJobsRegistry : Registry {

        public GlobalScheduledJobsRegistry() {

            // Run every 1 hour
            Schedule<GlobalCacheScheduledJob>().ToRunNow().AndEvery(1).Hours();
        }
        
    }
}
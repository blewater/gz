using System;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using NLog;

namespace gzCpcLib.Task {
    /// <summary>
    /// 
    /// A Customer Portfolio Calculation Task
    /// 
    /// </summary>
    public abstract class CpcTask : ICpcTask {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 
        /// Return an observable for the declared task
        /// 
        /// </summary>
        public IObservable<Unit> TaskObservable {
            get {
                return System.Threading.Tasks.Task.Factory
                    .StartNew(DoTask)
                    .ToObservable();
            }
        }

        /// <summary>
        /// 
        /// The task definition: Cpc biz logic
        /// 
        /// </summary>
        public abstract void DoTask();

    }
}
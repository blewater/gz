using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using gzDAL.Models;
using gzDAL.Repos;

namespace gzCpcLib {
    /// <summary>
    /// 
    /// A Customer Portfolio Calculation Task
    /// 
    /// </summary>
    public abstract class CpcTask : ICpcTask {

        /// <summary>
        /// 
        /// Return an observable for the declared task
        /// 
        /// </summary>
        public IObservable<Unit> TaskObservable {
            get {
                return Task.Factory
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
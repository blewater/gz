using System;
using System.Reactive;
using System.Reactive.Linq;
using gzDAL.Models;
using gzDAL.Repos;

namespace cpc {
    public abstract class ObservableTask : IObservableTask {

        /// <summary>
        /// Create an observable from the work
        /// </summary>
        public IObservable<Unit> WorkObservable {
            get {
                return Observable.Start(DoWork);
            }
        }

        public abstract void DoWork();

    }
}
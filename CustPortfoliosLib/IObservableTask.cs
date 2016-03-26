using System;
using System.Reactive;

namespace cpc {
    public interface IObservableTask {
        /// <summary>
        /// Create an observable from the work task
        /// </summary>
        IObservable<Unit> WorkObservable { get; }

        void DoWork();
    }
}
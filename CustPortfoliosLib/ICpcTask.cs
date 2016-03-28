using System;
using System.Reactive;

namespace gzCpcLib {
    public interface ICpcTask {
        /// <summary>
        /// Create an observable from the work task
        /// </summary>
        IObservable<Unit> TaskObservable { get; }

        void DoTask();
    }
}
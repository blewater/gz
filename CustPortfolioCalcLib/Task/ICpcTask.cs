using System;
using System.Reactive;

namespace gzCpcLib.Task {

    public interface ICpcTask {

        IObservable<Unit> TaskObservable { get; }

        void DoTask();

    }

}
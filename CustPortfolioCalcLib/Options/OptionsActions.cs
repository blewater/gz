using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;
using gzCpcLib.Task;

namespace gzCpcLib.Options {

    /// <summary>
    /// 
    /// Execute the tasks associated to the parsed options.
    /// 
    /// </summary>
    public class OptionsActions {

        private readonly CpcOptions cpcOptions;
        private ExchRatesUpd exchRatesUpd;
        private FundsUpd fundsUpd;
        private CustInvestmentBalUpd custInvestBalUpd;

        public bool IsProcessing { get; private set; } = false;

        public OptionsActions(CpcOptions inCpcOptions
            , ExchRatesUpd inExchRatesUpd
            , FundsUpd inFundsUpd
            , CustInvestmentBalUpd inCustInvestBalUpd) {

            this.cpcOptions = inCpcOptions;
            this.exchRatesUpd = inExchRatesUpd;
            this.fundsUpd = inFundsUpd;
            this.custInvestBalUpd = inCustInvestBalUpd;
        }

        public void ProcessOptions() {

            if (!cpcOptions.ParsingSuccess) {
                // Did not process
                return ;
            }

            // Starting to process
            IsProcessing = true;

            if (cpcOptions.CurrenciesMarketUpdOnly) {

                SubscribeToObs(exchRatesUpd, "Currencies updated.", indicateWhenCompleteProcessing : true);
                        
            } else if (cpcOptions.StockMarketUpdOnly) {

                SubscribeToObs(fundsUpd, "Funds stock values updated.", indicateWhenCompleteProcessing: true);

            } else if (cpcOptions.FinancialValuesUpd) {

                MergeObs(exchRatesUpd, fundsUpd, "Financial Values Updated.");

            } else if (cpcOptions.CustomersToProc != null) {

                custInvestBalUpd.CustomerIds = cpcOptions.CustomersToProc;
                MergeReduceObs(exchRatesUpd, fundsUpd, custInvestBalUpd, "Customers Balances Processed");

            } else {
                Console.WriteLine("No action taken!");
                IsProcessing = false;
            }

            return ;
        }

        /// <summary>
        /// Subscribe to a task observable
        /// </summary>
        /// <param name="cpcTask"></param>
        /// <param name="logCompletionMsg"></param>
        /// <param name="indicateWhenCompleteProcessing"></param>
        public void SubscribeToObs(CpcTask cpcTask, string logCompletionMsg, bool indicateWhenCompleteProcessing = false) {

            cpcTask.TaskObservable.Subscribe(
                _ => {

                    Console.WriteLine(logCompletionMsg);
                    if (indicateWhenCompleteProcessing) {
                        IsProcessing = false;
                    }

               }
            );
        }

        /// <summary>
        /// Merge 2 parallel task observables
        /// </summary>
        /// <param name="cpcTask1"></param>
        /// <param name="cpcTask2"></param>
        /// <param name="logCompletionMsg"></param>
        public void MergeObs(CpcTask cpcTask1, CpcTask cpcTask2, string logCompletionMsg) {

            cpcTask1.TaskObservable
                .Merge(cpcTask2.TaskObservable)
                .Subscribe(
                    // OnNext
                    w =>
                        Console.WriteLine("OnNext"),

                    // OnCompleted
                    () => {
                        Console.WriteLine(logCompletionMsg);
                        IsProcessing = false;
                    }
                );
        }

        /// <summary>
        /// Merge 2 parallel task observable, wait their completion,
        /// subscribe to last Task observable.
        /// </summary>
        /// <param name="cpcTask1"></param>
        /// <param name="cpcTask2"></param>
        /// <param name="cpcFinalTask"></param>
        /// <param name="logCompletionMsg"></param>
        public void MergeReduceObs(CpcTask cpcTask1, CpcTask cpcTask2, CpcTask cpcFinalTask, string logCompletionMsg) {

            cpcTask1.TaskObservable
                
                .Merge(cpcTask2.TaskObservable)
            
                // Wait to complete before final task
                .TakeLast(1)
                .Merge(cpcFinalTask.TaskObservable)
                .Subscribe(
                    // OnNext
                    w =>
                        Console.WriteLine("OnNext Final Task"),

                    // OnCompleted
                    () => {
                        Console.WriteLine(logCompletionMsg);
                        IsProcessing = false;
                    }
                );
        }
    }
}

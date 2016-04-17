using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;
using gzCpcLib.Task;
using NLog;

namespace gzCpcLib.Options {

    /// <summary>
    /// 
    /// Execute the tasks associated to the parsed options.
    /// 
    /// </summary>
    public class OptionsActions {

        private readonly CpcOptions cpcOptions;
        private readonly ExchRatesUpdTask exchRatesUpd;
        private readonly FundsUpdTask fundsUpd;
        private readonly CustomerBalanceUpdTask customerBalUpd;
        private readonly Logger logger;

        public bool IsProcessing { get; private set; }

        public OptionsActions(CpcOptions inCpcOptions,
            ExchRatesUpdTask inExchRatesUpd,
            FundsUpdTask inFundsUpd,
            CustomerBalanceUpdTask inCustomerBalUpd,
            Logger inLogger) {

            this.cpcOptions = inCpcOptions;
            this.exchRatesUpd = inExchRatesUpd;
            this.fundsUpd = inFundsUpd;
            this.customerBalUpd = inCustomerBalUpd;
            this.logger = inLogger;
        }

        public void ProcessOptions() {

            if (!cpcOptions.ParsingSuccess) {

                logger.Trace("Exiting ProcessOptions cpcOptions.ParsingSucces is false");

                // Did not process
                return;
            }

            // Starting to process
            IsProcessing = true;

            if (cpcOptions.CurrenciesMarketUpdOnly) {

                SubscribeToObs(exchRatesUpd, "Currencies updated.", indicateWhenCompleteProcessing: true);

            }
            else if (cpcOptions.StockMarketUpdOnly) {

                SubscribeToObs(fundsUpd, "Funds stock values updated.", indicateWhenCompleteProcessing: true);

            }
            else if (cpcOptions.FinancialValuesUpd) {

                MergeObs(exchRatesUpd, fundsUpd, "Financial Values Updated.");

            }
            else if (cpcOptions.ProcessEverything || cpcOptions.CustomersToProc.Length > 0 || cpcOptions.YearMonthsToProc.Length > 0) {

                customerBalUpd.CustomerIds = cpcOptions.CustomersToProc;
                customerBalUpd.YearMonthsToProc = cpcOptions.YearMonthsToProc;

                // Wait for both to complete before moving on: merge
                MergeReduceObs(exchRatesUpd, fundsUpd, customerBalUpd, "Customers Balances Processed");

            } else if (cpcOptions.ConsoleOutOnly) {
                
            }
            else {
                logger.Trace("No action taken!");
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

                    logger.Info(logCompletionMsg);
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
                        logger.Trace("OnNext"),

                    // OnCompleted
                    () => {
                        logger.Info(logCompletionMsg);
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
                        logger.Trace("OnNext Final Task"),

                    // OnCompleted
                    () => {
                        logger.Info(logCompletionMsg);
                        IsProcessing = false;
                    }
                );
        }
    }
}

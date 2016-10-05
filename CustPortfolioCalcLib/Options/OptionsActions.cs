using System;
using System.Linq;
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

        private readonly CpcOptions _cpcOptions;
        private readonly ExchRatesUpdTask _exchRatesUpd;
        private readonly FundsUpdTask _fundsUpd;
        private readonly CustomerBalanceUpdTask _customerBalUpd;
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public bool IsProcessing { get; private set; }

        public OptionsActions(CpcOptions inCpcOptions,
            ExchRatesUpdTask inExchRatesUpd,
            FundsUpdTask inFundsUpd,
            CustomerBalanceUpdTask inCustomerBalUpd) {

            this._cpcOptions = inCpcOptions;
            this._exchRatesUpd = inExchRatesUpd;
            this._fundsUpd = inFundsUpd;
            this._customerBalUpd = inCustomerBalUpd;
        }

        public void ProcessOptions() {

            if (!_cpcOptions.ParsingSuccess) {

                _logger.Trace("Exiting ProcessOptions cpcOptions.ParsingSucces is false");

                // Did not process
                return;
            }

            // Starting to process
            IsProcessing = true;

            if (_cpcOptions.CurrenciesMarketUpdOnly) {

                _logger.Info("In _cpcOptions.CurrenciesMarketUpdOnly");
                SubscribeToObs(_exchRatesUpd, "Currencies updated.", indicateWhenCompleteProcessing: true);

            }
            else if (_cpcOptions.StockMarketUpdOnly) {

                _logger.Info("In _cpcOptions.StockMarketUpdOnly");
                SubscribeToObs(_fundsUpd, "Funds stock values updated.", indicateWhenCompleteProcessing: true);

            }
            else if (_cpcOptions.FinancialValuesUpd) {

                _logger.Info("In _cpcOptions.FinancialValuesUpd");
                MergeObs(_exchRatesUpd, _fundsUpd, "Financial Values Updated.");

            }
            else if (_cpcOptions.ProcessEverything || _cpcOptions.CustomersToProc.Length > 0 || _cpcOptions.YearMonthsToProc.Length > 0) {

                _logger.Info("In _cpcOptions.ProcessEverything");
                _customerBalUpd.CustomerIds = _cpcOptions.CustomersToProc.ToList();
                _customerBalUpd.YearMonthsToProc = _cpcOptions.YearMonthsToProc.ToList();

                // Wait for both to complete before moving on: merge
                MergeReduceObs(_exchRatesUpd, _fundsUpd, _customerBalUpd, "Customers Balances Processed");

            } else if (_cpcOptions.ConsoleOutOnly) {
                _logger.Info("In _cpcOptions.ConsoleOutOnly");

            } else {
                _logger.Trace("No action taken!");
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

                    _logger.Info(logCompletionMsg);
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
                        _logger.Trace("OnNext"),

                    // OnCompleted
                    () => {
                        _logger.Info(logCompletionMsg);
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
                        _logger.Trace("OnNext Final Task"),

                    // OnCompleted
                    () => {
                        _logger.Info(logCompletionMsg);
                        IsProcessing = false;
                    }
                );
        }
    }
}

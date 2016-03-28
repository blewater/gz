using System;
using System.Reactive;
using System.Reactive.Linq;
using gzCpcLib.Task;

namespace gzCpcLib.Options {

    public class OptionsActions {

        private readonly CpcOptions _cpcOptions;
        private CurrencyRatesUpdDb currencyRatesUpdDb;
        private FundMarketUpdDb fundMarketUpdDb;

        public bool IsProcessing { get; private set; } = false;

        public OptionsActions(CpcOptions inCpcOptions, CurrencyRatesUpdDb inCurrencyRatesUpdDb, FundMarketUpdDb inFundMarketUpdDb) {

            this._cpcOptions = inCpcOptions;
            this.currencyRatesUpdDb = inCurrencyRatesUpdDb;
            this.fundMarketUpdDb = inFundMarketUpdDb;
        }

        public void ProcOptions() {

            if (!_cpcOptions.ParsingSuccess) {
                // Did not process
                return ;
            }

            // Starting to process
            IsProcessing = true;

            if (_cpcOptions.CurrenciesMarketUpdOnly) {

                SubscribeToObs(currencyRatesUpdDb, "Currencies updated.", indicateWhenCompleteProcessing : true);
                        
            } else if (_cpcOptions.StockMarketUpdOnly) {

                SubscribeToObs(fundMarketUpdDb, "Funds stock values updated.", indicateWhenCompleteProcessing: true);

            } else if (_cpcOptions.FinancialValuesUpd) {

                MergeObs(currencyRatesUpdDb, fundMarketUpdDb, "Financial Values Updated.");

            } else {
                Console.WriteLine("No action taken!");
                IsProcessing = false;
            }

            return ;
        }

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

        public void SyncSubscribe(IObservable<Unit> observable) {
            observable.Wait();
        }

    }
}

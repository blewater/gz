﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cpc {
    public class OptionsActions {

        private readonly Options options;
        private CurrencyRatesUpdDb currencyRatesUpdDb;
        private FundMarketUpdDb fundMarketUpdDb;

        public bool IsProcessing { get; private set; } = false;

        public OptionsActions(Options inOptions, CurrencyRatesUpdDb inCurrencyRatesUpdDb, FundMarketUpdDb inFundMarketUpdDb) {

            this.options = inOptions;
            this.currencyRatesUpdDb = inCurrencyRatesUpdDb;
            this.fundMarketUpdDb = inFundMarketUpdDb;
        }

        public void ProcOptions() {

            if (!options.ParsingSuccess) {
                // Did not process
                return ;
            }

            // Starting to process
            IsProcessing = true;

            if (options.CurrenciesMarketUpdOnly) {

                SubscribeToObs(currencyRatesUpdDb, "Currencies updated.", indicateWhenCompleteProcessing : true);
                        
            } else if (options.StockMarketUpdOnly) {

                SubscribeToObs(fundMarketUpdDb, "Funds stock values updated.", indicateWhenCompleteProcessing: true);

            } else if (options.FinancialValuesUpd) {

                MergeObs(currencyRatesUpdDb, fundMarketUpdDb, "Financial Values Updated.");

            } else {
                Console.WriteLine("No action taken!");
                IsProcessing = false;
            }

            return ;
        }

        public void SubscribeToObs(ObservableTask cpcTask, string logCompletionMsg, bool indicateWhenCompleteProcessing = false) {

            cpcTask.WorkObservable.Subscribe(
                _ => {

                    Console.WriteLine(logCompletionMsg);
                    if (indicateWhenCompleteProcessing) {
                        IsProcessing = false;
                    }

               }
            );
        }

        public void MergeObs(ObservableTask cpcTask1, ObservableTask cpcTask2, string logCompletionMsg) {

            cpcTask1.WorkObservable
                .Merge(cpcTask2.WorkObservable)
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

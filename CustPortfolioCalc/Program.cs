using System;
using System.Threading;
using gzCpcLib.Options;
using gzCpcLib.Task;

namespace CustPortfoliosCalc {

    class Program {

        const int SleepIntervalMillis = 250;

        static void Main(string[] args) {

            try {
                var options = CpcOptions.ProcArgs(args);
                var optionsActions = new OptionsActions(options, new CurrencyRatesUpdDb(), new FundMarketUpdDb());
                optionsActions.ProcOptions();

                while (optionsActions.IsProcessing) {
                    Thread.Sleep(SleepIntervalMillis);
                }
            }
            catch (Exception ex) {
                var exMsg = ex.Message;
            }
        }
    }
}

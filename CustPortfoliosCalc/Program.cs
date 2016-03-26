using System;
using System.Threading;
using cpc;

namespace CustPortfoliosCalc {

    class Program {

        const int SleepIntervalMillis = 250;

        static void Main(string[] args) {

            try {
                var options = Options.ProcArgs(args);
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

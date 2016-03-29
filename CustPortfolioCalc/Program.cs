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

                if (options.ParsingSuccess) {

                    ProcessParsedOptions(options);

                }
            }
            catch (Exception ex) {
                var exMsg = ex.Message;
            }
        }

        private static void ProcessParsedOptions(CpcOptions options) {

            var optionsActions = new OptionsActions(options
                , new ExchRatesUpd()
                , new FundsUpd()
                , new CustInvestmentBalUpd());

            optionsActions.ProcessOptions();

            while (optionsActions.IsProcessing) {
                Thread.Sleep(SleepIntervalMillis);
            }
        }
    }
}

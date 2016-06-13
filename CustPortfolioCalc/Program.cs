using System;
using System.Threading;
using gzCpcLib.Options;
using gzCpcLib.Task;
using gzDAL.Models;
using gzDAL.Repos;
using NLog;
using PostSharp.Patterns.Diagnostics;
using PostSharp.Extensibility;

namespace CustPortfoliosCalc {

    class Program {

        const int SleepIntervalMillis = 250;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [Log]
        [LogException]
        public static void Main(string[] args) {

            try {
                var options = CpcOptions.ProcArgs(args);

                if (options.ParsingSuccess) {

                    ProcessParsedOptions(options);

                }
            }
            catch (Exception ex) {
                throw;
            }
        }

        [Log]
        private static void ProcessParsedOptions(CpcOptions options) {

            var db = new ApplicationDbContext();

            var optionsActions = new OptionsActions(
                options,
                new ExchRatesUpdTask(),
                new FundsUpdTask(),
                new CustomerBalanceUpdTask(
                    db,
                    new InvBalanceRepo(
                        db,
                        new CustFundShareRepo(db, new CustPortfolioRepo(db)),
                        new GzTransactionRepo(db))));

            optionsActions.ProcessOptions();

            while (optionsActions.IsProcessing) {
                Thread.Sleep(SleepIntervalMillis);
            }
        }
    }
}

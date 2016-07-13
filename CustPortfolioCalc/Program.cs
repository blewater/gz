﻿using System;
using System.Threading;
using gzCpcLib.Options;
using gzCpcLib.Task;
using gzDAL.Models;
using gzDAL.Repos;
using NLog;

namespace CustPortfoliosCalc {

    class Program {

        const int SleepIntervalMillis = 250;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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

        private static void ProcessParsedOptions(CpcOptions options) {

            var db = new ApplicationDbContext();

            var custPortfolioRepo = new CustPortfolioRepo(db);
            var optionsActions = new OptionsActions(
                options,
                new ExchRatesUpdTask(),
                new FundsUpdTask(),
                new CustomerBalanceUpdTask(
                    db,
                    new InvBalanceRepo(
                        db,
                        new CustFundShareRepo(db, custPortfolioRepo),
                        new GzTransactionRepo(db), 
                        custPortfolioRepo)));

            optionsActions.ProcessOptions();

            while (optionsActions.IsProcessing) {
                Thread.Sleep(SleepIntervalMillis);
            }
        }
    }
}

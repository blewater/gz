using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using gzCpcLib;

namespace CustPortfoliosCalcTest {
    [TestClass]
    public class CmdOptionsTest {
        [TestMethod]
        public void ConsoleOnlyOption() {

            var options = Options.ProcArgs(new string[] { "-c" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.ConsoleOutOnly);
        }

        [TestMethod]
        public void CustomerIdOption() {

            var options = Options.ProcArgs(new string[] { "-i 1" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.CustomersToProc[0] == 1 && options.CustomersToProc.Length == 1);
        }

        [TestMethod]
        public void StockMarketOnlyOption() {

            var options = Options.ProcArgs(new string[] { "-s" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.StockMarketUpdOnly);
        }

        [TestMethod]
        public void FinancialOption() {

            var options = Options.ProcArgs(new string[] { "-f" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.FinancialValuesUpd);
        }

        [TestMethod]
        public void CurrencyRatesOnlyOption() {

            var options = Options.ProcArgs(new string[] { "-r" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.CurrenciesMarketUpdOnly);
        }

        [TestMethod]
        public void ConflictingMarketUpdOption() {

            var options = Options.ProcArgs(new string[] { "-r", "-f" });
            Assert.IsFalse(options.ParsingSuccess);

            Assert.IsTrue(options.CurrenciesMarketUpdOnly && options.FinancialValuesUpd);
        }

        [TestMethod]
        public void MonthOption() {

            var options = Options.ProcArgs(new string[] { "-m", "201505","201504" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.YearMonthsToProc[0] == 201505);
        }

        [TestMethod]
        public void MixedOptions() {

            var options = Options.ProcArgs(new string[] { "-c", "-i", "30", "1", "-m", "201601", "201502", "201501" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.ConsoleOutOnly);
            Assert.IsTrue(options.CustomersToProc[0] == 30);
            Assert.IsTrue(options.CustomersToProc[1] == 1);
            Assert.IsTrue(options.CustomersToProc.Length == 2);
            Assert.IsTrue(options.YearMonthsToProc[0] == 201601);
            Assert.IsTrue(options.YearMonthsToProc[1] == 201502);
            Assert.IsTrue(options.YearMonthsToProc[2]== 201501);
            Assert.IsTrue(options.YearMonthsToProc.Length == 3);
        }

    }
}

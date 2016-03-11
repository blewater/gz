using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using cpc;

namespace CustPortfoliosCalcTest {
    [TestClass]
    public class CmdOptionsTest {
        [TestMethod]
        public void ConsoleOnlyOption() {

            var options = CpcOptions.ProcArgs(new string[] { "-c" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.ConsoleOutOnly);
        }

        [TestMethod]
        public void CustomerIdOption() {

            var options = CpcOptions.ProcArgs(new string[] { "-i 1" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.CustomersToProc[0] == 1 && options.CustomersToProc.Length == 1);
        }

        [TestMethod]
        public void StockMarketOnlyOption() {

            var options = CpcOptions.ProcArgs(new string[] { "-s" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.StockMarketUpdOnly);
        }

        [TestMethod]
        public void MarketOnlyOption() {

            var options = CpcOptions.ProcArgs(new string[] { "-f" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.MarketUpdOnly);
        }

        [TestMethod]
        public void CurrencyRatesOnlyOption() {

            var options = CpcOptions.ProcArgs(new string[] { "-r" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.CurrenciesMarketUpdOnly);
        }

        [TestMethod]
        public void ConflictingMarketUpdOption() {

            var options = CpcOptions.ProcArgs(new string[] { "-r", "-f" });
            Assert.IsFalse(options.ParsingSuccess);

            Assert.IsTrue(options.CurrenciesMarketUpdOnly && options.MarketUpdOnly);
        }

        [TestMethod]
        public void MonthOption() {

            var options = CpcOptions.ProcArgs(new string[] { "-m", "201505","201504" });
            Assert.IsTrue(options.ParsingSuccess);

            Assert.IsTrue(options.YearMonthsToProc[0] == 201505);
        }

        [TestMethod]
        public void MixedOptions() {

            var options = CpcOptions.ProcArgs(new string[] { "-c", "-i", "30", "1", "-m", "201601", "201502", "201501" });
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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using cpc;

namespace CustPortfoliosCalcTest {
    [TestClass]
    public class CmdOptionsTest {
        [TestMethod]
        public void ConsoleOnlyOption() {

            CpcOptions.ProcArgs(new string[] { "-c" });

            Assert.IsFalse(CpcOptions.SaveToDb);
        }

        [TestMethod]
        public void CustomerIdOption() {

            CpcOptions.ProcArgs(new string[] { "-i=1" });

            Assert.IsTrue(CpcOptions.CustomersToProc[0] == 1 && CpcOptions.CustomersToProc.Count == 1);
        }

        [TestMethod]
        public void MarketOnlyOption() {

            CpcOptions.ProcArgs(new string[] { "-s" });

            Assert.IsTrue(CpcOptions.MarketUpdOnly);
        }

        [TestMethod]
        public void MonthOption() {

            CpcOptions.ProcArgs(new string[] { "-c", "-m=201505" });

            Assert.IsTrue(CpcOptions.YearMonnthsToProc[0] == "201505");
        }

        [TestMethod]
        public void MixedOptions() {

            CpcOptions.ProcArgs(new string[] { "-c", "/i 30", "--i=1" });

            Assert.IsFalse(CpcOptions.SaveToDb);
            Assert.IsTrue(CpcOptions.CustomersToProc[0] == 30);
            Assert.IsTrue(CpcOptions.CustomersToProc[1] == 1);
            Assert.IsTrue(CpcOptions.CustomersToProc.Count == 2);
        }

    }
}

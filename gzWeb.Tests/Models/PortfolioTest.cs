using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gzWeb.Models;
using System.Diagnostics;

namespace gzWeb.Tests.Models {
    [TestClass]
    public class PortfolioTest {
        [TestMethod]
        public void PortfolioReturns() {

            var portfolios = new PortfolioRepository().GetAllPortfolios();

            foreach (var p in portfolios) {
                Trace.Write(p.RiskTolerance.ToString() + " portfolio return ");
                var r = p.PortFunds.Select(f => f.Weight * Math.Max(f.Fund.ThreeYrReturnPcnt, f.Fund.FiveYrReturnPcnt)).Sum();
                Trace.WriteLine(r + "%");
            }
        }

    }
}

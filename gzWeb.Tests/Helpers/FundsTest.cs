using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gzWeb.Models.Util;
using System.Diagnostics;
using System.ComponentModel;

namespace gzWeb.Tests.Helpers {
    [TestClass]
    public class FundsTest {

        [TestMethod]
        public void GetYahooQuotes() {

            List<FundFullQuote> quotes = new List<FundFullQuote>() {
                new FundFullQuote() { Symbol = "VTI" },
                new FundFullQuote() { Symbol = "XLE" },
            };

            YQLFundCurrencyInq.FetchFundsFullQuoteInfo(quotes);

            foreach (var q in quotes) {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(q)) {
                    string name = descriptor.Name;
                    object value = descriptor.GetValue(q);
                    Debug.Print("{0}={1}", name, value);
                }
            }

            Assert.IsNotNull(quotes[0].BookValue);
        }
    }
}

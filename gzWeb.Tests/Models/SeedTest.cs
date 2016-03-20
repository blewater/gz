using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gzDAL.Models;
using gzDAL.Conf;

namespace gzWeb.Tests.Models {
    [TestClass]
    public class SeedTest {
        [TestMethod]
        public void SeedData() {

            // Null means no exception
            Assert.IsNull(Seed.GenData());
        }
    }
}


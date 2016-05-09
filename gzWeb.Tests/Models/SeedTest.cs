using System.Data.Entity;
using NUnit.Framework;
using gzDAL.Conf;
using gzDAL.Models;

namespace gzWeb.Tests.Models {
    [TestFixture]
    public class SeedTest {

        [OneTimeSetUp]
        public void OneTimeSetUp() {
            Database.SetInitializer<ApplicationDbContext>(null);
        }
        
        [Test]
        public void SeedData() {

            // Null means no exception
            Assert.IsNull(Seed.GenData());
        }
    }
}


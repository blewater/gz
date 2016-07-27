using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using gzWeb.Areas.Mvc.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gzWeb;

namespace gzWeb.Tests.Controllers {
    [TestClass]
    public class HomeControllerTest {
        [TestMethod]
        public void Index() {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            PartialViewResult result = controller.Index() as PartialViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}

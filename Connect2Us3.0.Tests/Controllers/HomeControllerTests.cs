using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using book2us.Controllers;

namespace Connect2Us3._0.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTests
    {
        [TestMethod]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
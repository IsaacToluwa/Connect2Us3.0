using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using book2us.Controllers;
using book2us.Models;
using Moq;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Security.Claims;
using Microsoft.AspNet.Identity;

namespace Connect2Us3._0.Tests.Controllers
{
    [TestClass]
    public class WalletControllerTests
    {
        [TestMethod]
        public void Index_UserWithNoWallet_CreatesWalletAndReturnsView()
        {
            // Arrange
            var data = new List<Wallet>().AsQueryable();
            var mockSet = new Mock<DbSet<Wallet>>();
            mockSet.As<IQueryable<Wallet>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Wallet>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Wallet>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Wallet>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<Book2UsContext>();
            mockContext.Setup(c => c.Wallets).Returns(mockSet.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            }));

            var controller = new WalletController();
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock }.Object
            };
            controller.ControllerContext.HttpContext.User = user;

            // Act
            var result = controller.Index() as ViewResult;
            var model = result.Model as Wallet;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(0, model.Balance);
        }

        [TestMethod]
        public void AddFunds_PositiveAmount_UpdatesBalanceAndRedirects()
        {
            // Arrange
            var wallet = new Wallet { UserId = "test-user-id", Balance = 100 };
            var data = new List<Wallet> { wallet }.AsQueryable();

            var mockSet = new Mock<DbSet<Wallet>>();
            mockSet.As<IQueryable<Wallet>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Wallet>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Wallet>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Wallet>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<Book2UsContext>();
            mockContext.Setup(c => c.Wallets).Returns(mockSet.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            }));

            var controller = new WalletController();
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock }.Object
            };
            controller.ControllerContext.HttpContext.User = user;

            // Act
            var result = controller.AddFunds(50) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(150, wallet.Balance);
        }
    }
}
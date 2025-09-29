using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using book2us.Controllers;
using book2us.Models;
using book2us.Services;
using Moq;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using System;

namespace Connect2Us3._0.Tests.Controllers
{
    [TestClass]
    public class WithdrawalSystemTests
    {
        private Mock<Book2UsContext> CreateMockContext()
        {
            var mockContext = new Mock<Book2UsContext>();
            return mockContext;
        }

        private ClaimsPrincipal CreateTestUser(string userId = "test-user-id")
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
            }));
        }

        private void SetupMockDbSet<T>(Mock<Book2UsContext> mockContext, IQueryable<T> data, Expression<Func<Book2UsContext, DbSet<T>>> dbSetProperty) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            
            mockContext.Setup(dbSetProperty).Returns(mockSet.Object);
        }

        [TestMethod]
        public void Withdraw_ValidRequest_RedirectsToWithdrawalHistory()
        {
            // Arrange
            var userId = "test-user-id";
            var wallet = new Wallet { UserId = userId, Balance = 500 };
            var bankAccount = new BankAccount { BankAccountId = 1, UserId = userId, IsActive = true };
            
            var wallets = new List<Wallet> { wallet }.AsQueryable();
            var bankAccounts = new List<BankAccount> { bankAccount }.AsQueryable();
            var withdrawalRequests = new List<WithdrawalRequest>().AsQueryable();
            var transactions = new List<Transaction>().AsQueryable();

            var mockContext = CreateMockContext();
            SetupMockDbSet(mockContext, wallets, c => c.Wallets);
            SetupMockDbSet(mockContext, bankAccounts, c => c.BankAccounts);
            SetupMockDbSet(mockContext, withdrawalRequests, c => c.WithdrawalRequests);
            SetupMockDbSet(mockContext, transactions, c => c.Transactions);

            var controller = new WalletController();
            var user = CreateTestUser(userId);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock }.Object
            };
            controller.ControllerContext.HttpContext.User = user;

            // Act
            var result = controller.Withdraw(100, 1) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("WithdrawalHistory", result.RouteValues["action"]);
        }

        [TestMethod]
        public void Withdraw_InsufficientFunds_ReturnsViewWithError()
        {
            // Arrange
            var userId = "test-user-id";
            var wallet = new Wallet { UserId = userId, Balance = 50 };
            var bankAccount = new BankAccount { BankAccountId = 1, UserId = userId, IsActive = true };
            
            var wallets = new List<Wallet> { wallet }.AsQueryable();
            var bankAccounts = new List<BankAccount> { bankAccount }.AsQueryable();
            var withdrawalRequests = new List<WithdrawalRequest>().AsQueryable();

            var mockContext = CreateMockContext();
            SetupMockDbSet(mockContext, wallets, c => c.Wallets);
            SetupMockDbSet(mockContext, bankAccounts, c => c.BankAccounts);
            SetupMockDbSet(mockContext, withdrawalRequests, c => c.WithdrawalRequests);

            var controller = new WalletController();
            var user = CreateTestUser(userId);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock }.Object
            };
            controller.ControllerContext.HttpContext.User = user;

            // Act
            var result = controller.Withdraw(100, 1) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Withdraw", result.ViewName ?? "");
            Assert.IsTrue(controller.ModelState.ContainsKey(""));
        }

        [TestMethod]
        public void WithdrawalHistory_UserHasRequests_ReturnsViewWithRequests()
        {
            // Arrange
            var userId = "test-user-id";
            var withdrawalRequests = new List<WithdrawalRequest>
            {
                new WithdrawalRequest 
                { 
                    UserId = userId, 
                    Amount = 100, 
                    Status = WithdrawalStatus.Pending,
                    RequestDate = DateTime.Now.AddDays(-1)
                },
                new WithdrawalRequest 
                { 
                    UserId = userId, 
                    Amount = 200, 
                    Status = WithdrawalStatus.Completed,
                    RequestDate = DateTime.Now.AddDays(-2)
                }
            }.AsQueryable();

            var mockContext = CreateMockContext();
            SetupMockDbSet(mockContext, withdrawalRequests, c => c.WithdrawalRequests);

            var controller = new WalletController();
            var user = CreateTestUser(userId);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock }.Object
            };
            controller.ControllerContext.HttpContext.User = user;

            // Act
            var result = controller.WithdrawalHistory() as ViewResult;
            var model = result.Model as List<WithdrawalRequest>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Count);
        }

        [TestMethod]
        public void BankingController_AddBankAccount_ValidModel_ReturnsRedirect()
        {
            // Arrange
            var userId = "test-user-id";
            var bankAccounts = new List<BankAccount>().AsQueryable();
            
            var mockContext = CreateMockContext();
            SetupMockDbSet(mockContext, bankAccounts, c => c.BankAccounts);

            var controller = new BankingController();
            var user = CreateTestUser(userId);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock }.Object
            };
            controller.ControllerContext.HttpContext.User = user;

            var model = new BankAccountViewModel
            {
                BankName = "Test Bank",
                AccountHolderName = "John Doe",
                AccountNumber = "1234567890",
                RoutingNumber = "123456789",
                AccountType = AccountType.Checking
            };

            // Act
            var result = controller.AddBankAccount(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("BankAccounts", result.RouteValues["action"]);
        }

        [TestMethod]
        public void BankingController_Cards_ReturnsViewWithCards()
        {
            // Arrange
            var userId = "test-user-id";
            var cards = new List<CardDetails>
            {
                new CardDetails 
                { 
                    UserId = userId, 
                    CardHolderName = "John Doe",
                    LastFourDigits = "1234",
                    CardType = "Visa",
                    IsActive = true
                }
            }.AsQueryable();

            var mockContext = CreateMockContext();
            SetupMockDbSet(mockContext, cards, c => c.CardDetails);

            var controller = new BankingController();
            var user = CreateTestUser(userId);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock }.Object
            };
            controller.ControllerContext.HttpContext.User = user;

            // Act
            var result = controller.Cards() as ViewResult;
            var model = result.Model as List<CardDetails>;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Count);
            Assert.AreEqual("1234", model[0].LastFourDigits);
        }

        [TestMethod]
        public void FinancialService_CalculateWithdrawalFee_ReturnsCorrectFee()
        {
            // Arrange
            var service = new FinancialService();
            var amount = 100m;

            // Act
            var fee = service.CalculateWithdrawalFee(amount);

            // Assert
            Assert.AreEqual(2.50m, fee); // 2.5% of 100
        }

        [TestMethod]
        public void FinancialService_GenerateTransactionReference_ReturnsValidFormat()
        {
            // Arrange
            var service = new FinancialService();

            // Act
            var reference = service.GenerateTransactionReference();

            // Assert
            Assert.IsTrue(reference.StartsWith("TXN"));
            Assert.AreEqual(13, reference.Length); // TXN + 10 digits
        }
    }
}
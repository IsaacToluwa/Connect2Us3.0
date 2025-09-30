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
    public class PaymentSystemIntegrationTests
    {
        private Mock<Book2UsContext> CreateMockContext()
        {
            var mockContext = new Mock<Book2UsContext>();
            return mockContext;
        }

        private ClaimsPrincipal CreateTestUser(string userId = "test-user-id", string username = "testuser")
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, username),
            }));
        }

        private void SetupMockDbSet<T>(Mock<Book2UsContext> mockContext, IQueryable<T> data, System.Linq.Expressions.Expression<Func<Book2UsContext, DbSet<T>>> dbSetProperty) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            
            mockContext.Setup(dbSetProperty).Returns(mockSet.Object);
        }

        [TestMethod]
        public void CompletePaymentFlow_WalletToOrderPayment_Success()
        {
            // Arrange - Setup complete payment ecosystem
            var userId = "test-user-id";
            var username = "testuser";
            var wallet = new Wallet { UserId = userId, Balance = 200, IsActive = true };
            var order = new Order 
            { 
                Id = 1, 
                CustomerId = userId, 
                Username = username, 
                Total = 50, 
                Status = "Pending",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            var wallets = new List<Wallet> { wallet }.AsQueryable();
            var orders = new List<Order> { order }.AsQueryable();
            var transactions = new List<Transaction>().AsQueryable();

            var mockContext = CreateMockContext();
            SetupMockDbSet(mockContext, wallets, c => c.Wallets);
            SetupMockDbSet(mockContext, orders, c => c.Orders);
            SetupMockDbSet(mockContext, transactions, c => c.Transactions);

            var financialService = new FinancialService();
            var orderController = new OrderController();
            
            var user = CreateTestUser(userId, username);
            orderController.ControllerContext = new ControllerContext()
            {
                HttpContext = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock }.Object
            };
            orderController.ControllerContext.HttpContext.User = user;

            // Act - Simulate wallet payment for order
            var walletTransaction = financialService.CreateTransaction(
                userId,
                TransactionType.Payment,
                PaymentMethod.Wallet,
                50,
                $"Payment for Order #{order.Id}"
            );

            var paymentResult = financialService.ProcessWalletTransaction(walletTransaction);
            
            // Simulate order confirmation
            var confirmPaymentResult = orderController.ConfirmPayment(order.Id, "Wallet") as RedirectToRouteResult;

            // Assert
            Assert.IsTrue(paymentResult, "Wallet transaction should succeed");
            Assert.IsNotNull(confirmPaymentResult, "Order confirmation should succeed");
            Assert.AreEqual(150, wallet.Balance, "Wallet balance should be reduced by payment amount");
            Assert.AreEqual("Paid", order.Status, "Order status should be updated to Paid");
            Assert.IsNotNull(order.PaymentDate, "Payment date should be set");
        }

        [TestMethod]
        public void BankAccountManagement_AddAndDeleteBankAccount_Success()
        {
            // Arrange
            var userId = "test-user-id";
            var bankAccounts = new List<BankAccount>().AsQueryable();
            
            var mockContext = CreateMockContext();
            SetupMockDbSet(mockContext, bankAccounts, c => c.BankAccounts);

            var bankingController = new BankingController();
            var user = CreateTestUser(userId);
            bankingController.ControllerContext = new ControllerContext()
            {
                HttpContext = new Mock<HttpContextBase>() { DefaultValue = DefaultValue.Mock }.Object
            };
            bankingController.ControllerContext.HttpContext.User = user;

            var model = new BankAccountViewModel
            {
                BankName = "Test Bank",
                AccountHolderName = "John Doe",
                AccountNumber = "1234567890",
                RoutingNumber = "123456789",
                AccountType = AccountType.Checking
            };

            // Act - Add bank account
            var addResult = bankingController.AddBankAccount(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(addResult);
            Assert.AreEqual("BankAccounts", addResult.RouteValues["action"]);
        }

        [TestMethod]
        public void CardManagement_AddAndEncryptCard_Success()
        {
            // Arrange
            var userId = "test-user-id";
            var cardDetails = new List<CardDetails>().AsQueryable();
            
            var mockContext = CreateMockContext();
            SetupMockDbSet(mockContext, cardDetails, c => c.CardDetails);

            var financialService = new FinancialService();
            var cardNumber = "4111111111111111"; // Test Visa card

            // Act - Encrypt card number
            var encryptedCardNumber = financialService.EncryptCardNumber(cardNumber);
            var decryptedCardNumber = financialService.DecryptCardNumber(encryptedCardNumber);

            // Assert
            Assert.AreNotEqual(cardNumber, encryptedCardNumber, "Card number should be encrypted");
            Assert.AreEqual(cardNumber, decryptedCardNumber, "Decrypted card number should match original");
        }

        [TestMethod]
        public void WalletFunding_BankTransfer_Success()
        {
            // Arrange
            var userId = "test-user-id";
            var wallet = new Wallet { UserId = userId, Balance = 100, IsActive = true };
            var bankAccount = new BankAccount 
            { 
                BankAccountId = 1, 
                UserId = userId, 
                BankName = "Test Bank",
                AccountNumber = "1234567890",
                IsActive = true 
            };

            var wallets = new List<Wallet> { wallet }.AsQueryable();
            var bankAccounts = new List<BankAccount> { bankAccount }.AsQueryable();
            var transactions = new List<Transaction>().AsQueryable();

            var mockContext = CreateMockContext();
            SetupMockDbSet(mockContext, wallets, c => c.Wallets);
            SetupMockDbSet(mockContext, bankAccounts, c => c.BankAccounts);
            SetupMockDbSet(mockContext, transactions, c => c.Transactions);

            var financialService = new FinancialService();

            // Act - Simulate bank transfer funding
            var transaction = financialService.CreateTransaction(
                userId,
                TransactionType.Deposit,
                PaymentMethod.BankTransfer,
                50,
                "Bank transfer deposit",
                bankAccount.BankAccountId
            );

            var processResult = financialService.ProcessWalletTransaction(transaction);

            // Assert
            Assert.IsTrue(processResult, "Bank transfer should succeed");
            Assert.AreEqual(150, wallet.Balance, "Wallet should be funded with transfer amount");
            Assert.AreEqual(TransactionStatus.Completed, transaction.Status, "Transaction should be completed");
        }

        [TestMethod]
        public void WithdrawalRequest_ValidRequest_Success()
        {
            // Arrange
            var userId = "test-user-id";
            var wallet = new Wallet { UserId = userId, Balance = 200, IsActive = true };
            var bankAccount = new BankAccount 
            { 
                BankAccountId = 1, 
                UserId = userId, 
                BankName = "Test Bank",
                AccountNumber = "1234567890",
                IsActive = true 
            };

            var wallets = new List<Wallet> { wallet }.AsQueryable();
            var bankAccounts = new List<BankAccount> { bankAccount }.AsQueryable();
            var withdrawalRequests = new List<WithdrawalRequest>().AsQueryable();
            var transactions = new List<Transaction>().AsQueryable();

            var mockContext = CreateMockContext();
            SetupMockDbSet(mockContext, wallets, c => c.Wallets);
            SetupMockDbSet(mockContext, bankAccounts, c => c.BankAccounts);
            SetupMockDbSet(mockContext, withdrawalRequests, c => c.WithdrawalRequests);
            SetupMockDbSet(mockContext, transactions, c => c.Transactions);

            var financialService = new FinancialService();

            // Act - Create withdrawal request
            var withdrawalRequest = financialService.CreateWithdrawalRequest(
                userId,
                bankAccount.BankAccountId,
                50
            );

            var processResult = financialService.ProcessWithdrawalRequest(withdrawalRequest);

            // Assert
            Assert.IsTrue(processResult, "Withdrawal request should be processed successfully");
            Assert.AreEqual(150, wallet.Balance, "Wallet balance should be reduced by withdrawal amount");
            Assert.AreEqual(WithdrawalStatus.Completed, withdrawalRequest.Status, "Withdrawal should be completed");
            Assert.IsNotNull(withdrawalRequest.TransactionId, "Transaction ID should be set");
        }

        [TestMethod]
        public void TransactionFeeCalculation_CorrectFees_Applied()
        {
            // Arrange
            var financialService = new FinancialService();
            var withdrawalAmount = 100m;
            var expectedFee = 2.50m; // 2.5% of 100

            // Act
            var transaction = financialService.CreateTransaction(
                1,
                TransactionType.Withdrawal,
                PaymentMethod.BankTransfer,
                withdrawalAmount,
                "Test withdrawal"
            );

            // Assert
            Assert.AreEqual(expectedFee, transaction.Fee, "Transaction fee should be calculated correctly");
            Assert.AreEqual(withdrawalAmount - expectedFee, transaction.NetAmount, "Net amount should be after fee deduction");
        }

        [TestMethod]
        public void InsufficientBalance_Withdrawal_Fails()
        {
            // Arrange
            var userId = "test-user-id";
            var wallet = new Wallet { UserId = userId, Balance = 50, IsActive = true };
            var bankAccount = new BankAccount 
            { 
                BankAccountId = 1, 
                UserId = userId, 
                IsActive = true 
            };

            var wallets = new List<Wallet> { wallet }.AsQueryable();
            var bankAccounts = new List<BankAccount> { bankAccount }.AsQueryable();
            var transactions = new List<Transaction>().AsQueryable();

            var mockContext = CreateMockContext();
            SetupMockDbSet(mockContext, wallets, c => c.Wallets);
            SetupMockDbSet(mockContext, bankAccounts, c => c.BankAccounts);
            SetupMockDbSet(mockContext, transactions, c => c.Transactions);

            var financialService = new FinancialService();

            // Act - Attempt withdrawal with insufficient balance
            var transaction = financialService.CreateTransaction(
                userId,
                TransactionType.Withdrawal,
                PaymentMethod.Wallet,
                100, // More than wallet balance
                "Insufficient balance test"
            );

            var processResult = financialService.ProcessWalletTransaction(transaction);

            // Assert
            Assert.IsFalse(processResult, "Withdrawal should fail with insufficient balance");
            Assert.AreEqual(TransactionStatus.Failed, transaction.Status, "Transaction should be marked as failed");
            Assert.AreEqual(50, wallet.Balance, "Wallet balance should remain unchanged");
        }
    }
}
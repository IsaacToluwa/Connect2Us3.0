using System;
using System.Linq;
using System.Web.Mvc;
using book2us.Controllers;
using book2us.Models;
using book2us.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace book2us.Tests.Controllers
{
    [TestClass]
    public class BankingManagementTests
    {
        private Book2UsContext db;
        private BankingController controller;
        private FinancialService financialService;

        [TestInitialize]
        public void Setup()
        {
            db = new Book2UsContext();
            controller = new BankingController();
            financialService = new FinancialService(db);
        }

        [TestMethod]
        public void BankingController_Index_ReturnsViewWithBankingViewModel()
        {
            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Model, typeof(BankingViewModel));
        }

        [TestMethod]
        public void BankingController_AddBankAccount_Get_ReturnsView()
        {
            // Act
            var result = controller.AddBankAccount() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void BankingController_AddBankAccount_Post_ValidModel_AddsBankAccount()
        {
            // Arrange
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
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void BankingController_AddCard_Get_ReturnsView()
        {
            // Act
            var result = controller.AddCard() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void BankingController_AddCard_Post_ValidModel_AddsCard()
        {
            // Arrange
            var model = new CardViewModel
            {
                CardHolderName = "John Doe",
                CardNumber = "4111111111111111", // Valid test Visa card
                ExpiryMonth = 12,
                ExpiryYear = 2025,
                CVV = "123"
            };

            // Act
            var result = controller.AddCard(model) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void BankingController_DeleteBankAccount_DeactivatesAccount()
        {
            // Arrange - Create a test bank account
            var username = "testuser"; // This would normally come from the logged-in user
            var user = db.ApplicationUsers.FirstOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                Assert.Inconclusive("Test user not found. This test requires a logged-in user context.");
            }

            var bankAccount = new BankAccount
            {
                UserId = user.Id,
                BankName = "Test Bank",
                AccountHolderName = "John Doe",
                AccountNumber = "1234567890",
                RoutingNumber = "123456789",
                AccountType = AccountType.Checking,
                IsActive = true,
                CreatedDate = DateTime.Now,
                LastUsedDate = DateTime.Now
            };

            db.BankAccounts.Add(bankAccount);
            db.SaveChanges();

            // Act
            var result = controller.DeleteBankAccount(bankAccount.BankAccountId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);

            // Verify the account was deactivated
            var deactivatedAccount = db.BankAccounts.Find(bankAccount.BankAccountId);
            Assert.IsFalse(deactivatedAccount.IsActive);

            // Cleanup
            db.BankAccounts.Remove(deactivatedAccount);
            db.SaveChanges();
        }

        [TestMethod]
        public void BankingController_DeleteCard_DeactivatesCard()
        {
            // Arrange - Create a test card
            var username = "testuser"; // This would normally come from the logged-in user
            var user = db.ApplicationUsers.FirstOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                Assert.Inconclusive("Test user not found. This test requires a logged-in user context.");
            }

            var cardDetails = new CardDetails
            {
                UserId = user.Id,
                CardHolderName = "John Doe",
                CardNumber = "encrypted_card_number",
                LastFourDigits = "1111",
                ExpiryMonth = "12",
                ExpiryYear = "2025",
                CardType = "Visa",
                IsActive = true,
                IsDefault = false,
                CreatedDate = DateTime.Now,
                LastUsedDate = DateTime.Now
            };

            db.CardDetails.Add(cardDetails);
            db.SaveChanges();

            // Act
            var result = controller.DeleteCard(cardDetails.CardId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);

            // Verify the card was deactivated
            var deactivatedCard = db.CardDetails.Find(cardDetails.CardId);
            Assert.IsFalse(deactivatedCard.IsActive);

            // Cleanup
            db.CardDetails.Remove(deactivatedCard);
            db.SaveChanges();
        }

        [TestMethod]
        public void BankingController_SetDefaultCard_SetsNewDefaultCard()
        {
            // Arrange - Create test cards
            var username = "testuser"; // This would normally come from the logged-in user
            var user = db.ApplicationUsers.FirstOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                Assert.Inconclusive("Test user not found. This test requires a logged-in user context.");
            }

            var card1 = new CardDetails
            {
                UserId = user.Id,
                CardHolderName = "John Doe",
                CardNumber = "encrypted_card_number_1",
                LastFourDigits = "1111",
                ExpiryMonth = "12",
                ExpiryYear = "2025",
                CardType = "Visa",
                IsActive = true,
                IsDefault = true,
                CreatedDate = DateTime.Now,
                LastUsedDate = DateTime.Now
            };

            var card2 = new CardDetails
            {
                UserId = user.Id,
                CardHolderName = "Jane Doe",
                CardNumber = "encrypted_card_number_2",
                LastFourDigits = "2222",
                ExpiryMonth = "11",
                ExpiryYear = "2025",
                CardType = "MasterCard",
                IsActive = true,
                IsDefault = false,
                CreatedDate = DateTime.Now,
                LastUsedDate = DateTime.Now
            };

            db.CardDetails.AddRange(card1, card2);
            db.SaveChanges();

            // Act
            var result = controller.SetDefaultCard(card2.CardId) as RedirectToRouteResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Index", result.RouteValues["action"]);

            // Verify the default card was changed
            var updatedCard1 = db.CardDetails.Find(card1.CardId);
            var updatedCard2 = db.CardDetails.Find(card2.CardId);
            
            Assert.IsFalse(updatedCard1.IsDefault);
            Assert.IsTrue(updatedCard2.IsDefault);

            // Cleanup
            db.CardDetails.Remove(card1);
            db.CardDetails.Remove(card2);
            db.SaveChanges();
        }

        [TestMethod]
        public void BankingController_GetCardType_ReturnsCorrectCardType()
        {
            // Arrange
            var controller = new BankingController();
            var visaCard = "4111111111111111";
            var mastercardCard = "5555555555554444";
            var amexCard = "378282246310005";
            var discoverCard = "6011111111111117";

            // Act & Assert
            Assert.AreEqual("Visa", controller.GetCardType(visaCard));
            Assert.AreEqual("MasterCard", controller.GetCardType(mastercardCard));
            Assert.AreEqual("American Express", controller.GetCardType(amexCard));
            Assert.AreEqual("Discover", controller.GetCardType(discoverCard));
        }

        [TestCleanup]
        public void Cleanup()
        {
            db.Dispose();
            controller.Dispose();
        }
    }
}
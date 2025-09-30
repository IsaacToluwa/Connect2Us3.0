using book2us.Models;
using book2us.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace book2us.Controllers
{
    public class BankingController : Controller
    {
        private Book2UsContext db = new Book2UsContext();
        private FinancialService financialService;

        public BankingController()
        {
            financialService = new FinancialService(db);
        }

        // GET: Banking
        public ActionResult Index()
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new BankingViewModel
            {
                BankAccounts = db.BankAccounts.Where(b => b.UserId == user.Id).ToList(),
                CardDetails = db.CardDetails.Where(c => c.UserId == user.Id).ToList()
            };

            return View(model);
        }

        // GET: Banking/AddBankAccount
        public ActionResult AddBankAccount()
        {
            return View();
        }

        // POST: Banking/AddBankAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddBankAccount(BankAccountViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var bankAccount = new BankAccount
                {
                    UserId = user.Id,
                    BankName = model.BankName,
                    AccountHolderName = model.AccountHolderName,
                    AccountNumber = model.AccountNumber,
                    RoutingNumber = model.RoutingNumber,
                    AccountType = model.AccountType,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    LastUsedDate = DateTime.Now
                };

                db.BankAccounts.Add(bankAccount);
                db.SaveChanges();

                TempData["Success"] = "Bank account added successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while adding the bank account.");
            }

            return View(model);
        }

        // GET: Banking/AddCard
        public ActionResult AddCard()
        {
            return View();
        }

        // POST: Banking/AddCard
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCard(CardViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var encryptedCardNumber = financialService.EncryptCardNumber(model.CardNumber);
                var lastFourDigits = model.CardNumber.Substring(Math.Max(0, model.CardNumber.Length - 4));

                var cardDetails = new CardDetails
                {
                    UserId = user.Id,
                    CardHolderName = model.CardHolderName,
                    CardNumber = encryptedCardNumber,
                    LastFourDigits = lastFourDigits,
                    ExpiryMonth = model.ExpiryMonth.ToString("D2"),
                    ExpiryYear = model.ExpiryYear.ToString(),
                    CardType = GetCardType(model.CardNumber),
                    IsActive = true,
                    IsDefault = !db.CardDetails.Any(c => c.UserId == user.Id && c.IsActive),
                    CreatedDate = DateTime.Now,
                    LastUsedDate = DateTime.Now
                };

                db.CardDetails.Add(cardDetails);
                db.SaveChanges();

                TempData["Success"] = "Card added successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while adding the card.");
            }

            return View(model);
        }

        // POST: Banking/DeleteBankAccount/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBankAccount(int id)
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var bankAccount = db.BankAccounts.Find(id);
            if (bankAccount == null || bankAccount.UserId != user.Id)
            {
                return HttpNotFound();
            }

            try
            {
                bankAccount.IsActive = false;
                db.SaveChanges();
                TempData["Success"] = "Bank account removed successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while removing the bank account.";
            }

            return RedirectToAction("Index");
        }

        // POST: Banking/DeleteCard/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCard(int id)
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var card = db.CardDetails.Find(id);
            if (card == null || card.UserId != user.Id)
            {
                return HttpNotFound();
            }

            try
            {
                card.IsActive = false;
                
                // If this was the default card, set another card as default
                if (card.IsDefault)
                {
                    var otherCard = db.CardDetails
                        .Where(c => c.UserId == user.Id && c.IsActive && c.CardId != id)
                        .FirstOrDefault();
                    
                    if (otherCard != null)
                    {
                        otherCard.IsDefault = true;
                    }
                }

                db.SaveChanges();
                TempData["Success"] = "Card removed successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while removing the card.";
            }

            return RedirectToAction("Index");
        }

        // GET: Banking/BankAccounts
        public ActionResult BankAccounts()
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var bankAccounts = db.BankAccounts.Where(b => b.UserId == user.Id).ToList();
            return View(bankAccounts);
        }

        // GET: Banking/Cards
        public ActionResult Cards()
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new BankingViewModel
            {
                BankAccounts = db.BankAccounts.Where(b => b.UserId == user.Id).ToList(),
                CardDetails = db.CardDetails.Where(c => c.UserId == user.Id).ToList()
            };

            return View(model);
        }

        // POST: Banking/SetDefaultCard/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetDefaultCard(int id)
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Remove default status from all cards
                var userCards = db.CardDetails.Where(c => c.UserId == user.Id);
                foreach (var card in userCards)
                {
                    card.IsDefault = false;
                }

                // Set the selected card as default
                var selectedCard = db.CardDetails.Find(id);
                if (selectedCard != null && selectedCard.UserId == user.Id)
                {
                    selectedCard.IsDefault = true;
                }

                db.SaveChanges();
                TempData["Success"] = "Default card updated successfully";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while updating the default card.";
            }

            return RedirectToAction("Index");
        }

        private string GetCardType(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber)) return "Unknown";

            if (cardNumber.StartsWith("4")) return "Visa";
            if (cardNumber.StartsWith("5")) return "MasterCard";
            if (cardNumber.StartsWith("3")) return "American Express";
            if (cardNumber.StartsWith("6")) return "Discover";
            
            return "Other";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
using book2us.Models;
using book2us.Services;
using System;
using System.Linq;
using System.Web.Mvc;

namespace book2us.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private Book2UsContext db = new Book2UsContext();
        private FinancialService financialService;

        public WalletController()
        {
            financialService = new FinancialService(db);
        }

        // GET: Wallet
        public ActionResult Index()
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var wallet = db.Wallets.SingleOrDefault(w => w.UserId == user.Id);
            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = user.Id,
                    Balance = 0,
                    CreatedDate = DateTime.Now
                };
                db.Wallets.Add(wallet);
                db.SaveChanges();
            }

            ViewBag.BankAccounts = db.BankAccounts.Where(b => b.UserId == user.Id && b.IsActive).ToList();
            ViewBag.CardDetails = db.CardDetails.Where(c => c.UserId == user.Id && c.IsActive).ToList();
            
            return View(wallet);
        }

        // GET: Wallet/AddFunds
        public ActionResult AddFunds()
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.BankAccounts = db.BankAccounts.Where(b => b.UserId == user.Id && b.IsActive).ToList();
            ViewBag.CardDetails = db.CardDetails.Where(c => c.UserId == user.Id && c.IsActive).ToList();
            
            return View();
        }

        // POST: Wallet/AddFunds
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddFunds(decimal amount, PaymentMethod paymentMethod, int? bankAccountId, int? cardId)
        {
            if (amount <= 0)
            {
                ModelState.AddModelError("amount", "Amount must be greater than zero");
                return View();
            }

            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var transaction = financialService.CreateTransaction(
                    user.Id,
                    TransactionType.Deposit,
                    paymentMethod,
                    amount,
                    $"Wallet top-up via {paymentMethod}"
                );

                if (financialService.ProcessWalletTransaction(transaction))
                {
                    TempData["Success"] = $"Successfully added ${amount} to your wallet";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to process payment. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your payment.");
            }

            ViewBag.BankAccounts = db.BankAccounts.Where(b => b.UserId == user.Id && b.IsActive).ToList();
            ViewBag.CardDetails = db.CardDetails.Where(c => c.UserId == user.Id && c.IsActive).ToList();
            
            return View();
        }

        // GET: Wallet/Withdraw
        public ActionResult Withdraw()
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var wallet = db.Wallets.SingleOrDefault(w => w.UserId == user.Id);
            if (wallet == null || wallet.Balance <= 0)
            {
                TempData["Error"] = "Your wallet balance is insufficient for withdrawal";
                return RedirectToAction("Index");
            }

            ViewBag.BankAccounts = db.BankAccounts.Where(b => b.UserId == user.Id && b.IsActive).ToList();
            
            return View();
        }

        // POST: Wallet/Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Withdraw(decimal amount, int bankAccountId)
        {
            if (amount <= 0)
            {
                ModelState.AddModelError("amount", "Amount must be greater than zero");
                return View();
            }

            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var wallet = db.Wallets.SingleOrDefault(w => w.UserId == user.Id);
            if (wallet == null || wallet.Balance < amount)
            {
                ModelState.AddModelError("", "Insufficient balance for this withdrawal");
                ViewBag.BankAccounts = db.BankAccounts.Where(b => b.UserId == user.Id && b.IsActive).ToList();
                return View();
            }

            try
            {
                var withdrawalRequest = financialService.CreateWithdrawalRequest(user.Id, bankAccountId, amount);
                
                if (financialService.ProcessWithdrawalRequest(withdrawalRequest))
                {
                    TempData["Success"] = $"Withdrawal request of ${amount} has been processed successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to process withdrawal. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your withdrawal.");
            }

            ViewBag.BankAccounts = db.BankAccounts.Where(b => b.UserId == user.Id && b.IsActive).ToList();
            return View();
        }

        // GET: Wallet/TransactionHistory
        public ActionResult TransactionHistory()
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var transactions = db.Transactions
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.CreatedDate)
                .Take(50)
                .ToList();

            return View(transactions);
        }

        // GET: Wallet/WithdrawalHistory
        public ActionResult WithdrawalHistory()
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var withdrawals = db.WithdrawalRequests
                .Where(w => w.UserId == user.Id)
                .OrderByDescending(w => w.RequestedDate)
                .Take(50)
                .ToList();

            return View(withdrawals);
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
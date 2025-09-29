using book2us.Models;
using System.Linq;
using System.Web.Mvc;

namespace book2us.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private Book2UsContext db = new Book2UsContext();

        // GET: Wallet
        public ActionResult Index()
        {
            var currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = currentUser.Id.ToString();
            var wallet = db.Wallets.SingleOrDefault(w => w.UserId == userId);
            if (wallet == null)
            {
                // Create a wallet for the user if it doesn't exist
                wallet = new Wallet { UserId = userId, Balance = 0 };
                db.Wallets.Add(wallet);
                db.SaveChanges();
            }
            return View(wallet);
        }

        // GET: Wallet/AddFunds
        public ActionResult AddFunds()
        {
            return View();
        }

        // POST: Wallet/AddFunds
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddFunds(decimal amount)
        {
            if (amount > 0)
            {
                var currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                var userId = currentUser.Id.ToString();
                var wallet = db.Wallets.SingleOrDefault(w => w.UserId == userId);
                if (wallet != null)
                {
                    wallet.Balance += amount;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
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
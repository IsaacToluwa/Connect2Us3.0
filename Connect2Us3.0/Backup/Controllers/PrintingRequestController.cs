using book2us.Models;
using System.Linq;
using System.Web.Mvc;

namespace book2us.Controllers
{
    [Authorize(Roles = "Seller")]
    public class PrintingRequestController : Controller
    {
        private Book2UsContext db = new Book2UsContext();

        // GET: PrintingRequest
        public ActionResult Index()
        {
            var currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var printingRequests = db.PrintingRequests.Where(pr => pr.UserId == currentUser.Id.ToString()).ToList();
            return View(printingRequests);
        }

        // GET: PrintingRequest/Create
        public ActionResult Create()
        {
            ViewBag.BookId = new SelectList(db.Books.Where(b => b.SellerUserName == User.Identity.Name), "Id", "Title");
            return View();
        }

        // POST: PrintingRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookId,Quantity")] PrintingRequest printingRequest)
        {
            if (ModelState.IsValid)
            {
                var currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                printingRequest.UserId = currentUser.Id.ToString();
                printingRequest.RequestDate = System.DateTime.Now;
                printingRequest.Status = "Pending";
                db.PrintingRequests.Add(printingRequest);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookId = new SelectList(db.Books.Where(b => b.SellerUserName == User.Identity.Name), "Id", "Title", printingRequest.BookId);
            return View(printingRequest);
        }
    }
}
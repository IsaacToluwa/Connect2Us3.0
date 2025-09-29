using book2us.Models;
using Microsoft.AspNet.Identity;
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
            var userId = User.Identity.GetUserId();
            var printingRequests = db.PrintingRequests.Where(pr => pr.UserId == userId).ToList();
            return View(printingRequests);
        }

        // GET: PrintingRequest/Create
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
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
                printingRequest.UserId = User.Identity.GetUserId();
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
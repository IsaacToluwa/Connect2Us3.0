using book2us.Models;
using book2us.Services;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace book2us.Controllers
{
    [Authorize(Roles = "Seller")]
    public class PrintingRequestController : Controller
    {
        private Book2UsContext db = new Book2UsContext();
        private readonly EmailService _emailService = new EmailService();

        // GET: PrintingRequest
        public ActionResult Index()
        {
            var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var printingRequests = db.PrintingRequests.Where(pr => pr.UserId == currentUser.Id).ToList();
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
                var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                printingRequest.UserId = currentUser.Id;
                printingRequest.RequestDate = System.DateTime.Now;
                printingRequest.Status = "Pending";
                db.PrintingRequests.Add(printingRequest);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BookId = new SelectList(db.Books.Where(b => b.SellerUserName == User.Identity.Name), "Id", "Title", printingRequest.BookId);
            return View(printingRequest);
        }

        // GET: PrintingRequest/UpdateStatus/5
        public ActionResult UpdateStatus(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            
            var printingRequest = db.PrintingRequests.Find(id);
            if (printingRequest == null)
            {
                return HttpNotFound();
            }
            
            ViewBag.StatusList = new SelectList(new[] { "Pending", "Processing", "Completed", "Cancelled" });
            return View(printingRequest);
        }

        // POST: PrintingRequest/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status, string additionalInfo = "")
        {
            var printingRequest = db.PrintingRequests
                .Include(pr => pr.User)
                .FirstOrDefault(pr => pr.PrintingRequestId == id);
                
            if (printingRequest == null)
            {
                return HttpNotFound();
            }

            var oldStatus = printingRequest.Status;
            printingRequest.Status = status;
            
            db.SaveChanges();

            // Send status update email to the customer
            if (printingRequest.User != null)
            {
                _emailService.SendPrintingStatusUpdate(
                    printingRequest.User.Email,
                    printingRequest.User.UserName,
                    printingRequest.PrintingRequestId,
                    status,
                    additionalInfo
                );
            }

            TempData["SuccessMessage"] = $"Printing request status updated from '{oldStatus}' to '{status}'.";
            return RedirectToAction("Index");
        }
    }
}
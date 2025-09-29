using book2us.Models;
using book2us.Services;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace book2us.Controllers
{
    [Authorize]
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
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user is a customer
            var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Only customers can create printing requests
            if (currentUser.Role != "Customer")
            {
                ViewBag.CurrentRole = currentUser.Role;
                return View("CustomerOnly");
            }

            // No book selection needed - customers upload their own PDFs
            return View();
        }

        // POST: PrintingRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Quantity")] PrintingRequest printingRequest, HttpPostedFileBase pdfFile)
        {
            // Security check: verify user is a customer
            var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Only customers can create printing requests
            if (currentUser.Role != "Customer")
            {
                ViewBag.CurrentRole = currentUser.Role;
                return View("CustomerOnly");
            }

            if (ModelState.IsValid)
            {
                // Validate PDF file
                if (pdfFile == null || pdfFile.ContentLength == 0)
                {
                    ModelState.AddModelError("pdfFile", "Please select a PDF file to upload.");
                    return View(printingRequest);
                }

                if (!pdfFile.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) && 
                    !pdfFile.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("pdfFile", "Please upload a valid PDF file.");
                    return View(printingRequest);
                }

                // Save PDF file
                string fileName = Guid.NewGuid().ToString() + "_" + pdfFile.FileName;
                string uploadPath = Server.MapPath("~/Uploads/PrintingRequests/");
                
                // Create directory if it doesn't exist
                if (!System.IO.Directory.Exists(uploadPath))
                {
                    System.IO.Directory.CreateDirectory(uploadPath);
                }
                
                string filePath = System.IO.Path.Combine(uploadPath, fileName);
                pdfFile.SaveAs(filePath);

                // Create printing request
                printingRequest.UserId = currentUser.Id;
                printingRequest.RequestDate = System.DateTime.Now;
                printingRequest.Status = "Pending";
                db.PrintingRequests.Add(printingRequest);
                db.SaveChanges();

                // Store the PDF file path and printing request info in session for checkout
                Session["PrintingRequestPdfPath"] = filePath;
                Session["PrintingRequestId"] = printingRequest.PrintingRequestId;
                Session["IsPrintingService"] = true;

                // Redirect to checkout - this will handle authentication if needed
                return RedirectToAction("Checkout", "ShoppingCart");
            }

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

        // GET: PrintingRequest/TestRedirect
        public ActionResult TestRedirect()
        {
            return View();
        }
    }
}
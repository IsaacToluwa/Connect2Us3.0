using book2us.Models;
using book2us.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace book2us.Controllers
{
    [Authorize]
    public class CustomerPrintingController : Controller
    {
        private Book2UsContext db = new Book2UsContext();
        private readonly EmailService _emailService = new EmailService();

        // GET: CustomerPrinting/Test
        [AllowAnonymous]
        public ActionResult Test()
        {
            return View();
        }

        // GET: CustomerPrinting
        public ActionResult Index()
        {
            var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var printingRequests = db.PrintingRequests
                .Include(pr => pr.Book)
                .Where(pr => pr.UserId == currentUser.Id)
                .OrderByDescending(pr => pr.RequestDate)
                .ToList();
                
            return View(printingRequests);
        }

        // GET: CustomerPrinting/Request/5
        public ActionResult Request(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }

            var model = new PrintingRequest
            {
                BookId = book.Id,
                Quantity = 1,
                TotalPages = 1,
                FulfillmentMethod = "Pickup"
            };

            ViewBag.BookTitle = book.Title;
            ViewBag.BookAuthor = book.Author;
            ViewBag.BookPrice = book.Price;

            return View(model);
        }

        // POST: CustomerPrinting/Request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Request([Bind(Include = "BookId,Quantity,TotalPages,FulfillmentMethod,DeliveryAddress,CustomerNotes")] PrintingRequest printingRequest)
        {
            if (ModelState.IsValid)
            {
                var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var book = db.Books.Find(printingRequest.BookId);
                if (book == null)
                {
                    ModelState.AddModelError("BookId", "Book not found.");
                    return View(printingRequest);
                }

                printingRequest.UserId = currentUser.Id;
                printingRequest.RequestDate = DateTime.Now;
                printingRequest.Status = "Pending";
                printingRequest.TotalPages = printingRequest.TotalPages > 0 ? printingRequest.TotalPages : 1;
                
                // Calculate initial costs
                printingRequest.CalculatePrintingCost();
                printingRequest.CalculateDeliveryCommission();

                db.PrintingRequests.Add(printingRequest);
                db.SaveChanges();

                // Assign to nearest seller or employee
                AssignPrintRequestToNearest(printingRequest);

                TempData["SuccessMessage"] = "Your printing request has been submitted successfully! You will receive an email confirmation shortly.";
                return RedirectToAction("Index");
            }

            // If we got this far, something failed, redisplay form
            var bookForView = db.Books.Find(printingRequest.BookId);
            if (bookForView != null)
            {
                ViewBag.BookTitle = bookForView.Title;
                ViewBag.BookAuthor = bookForView.Author;
                ViewBag.BookPrice = bookForView.Price;
            }

            return View(printingRequest);
        }

        // GET: CustomerPrinting/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var printingRequest = db.PrintingRequests
                .Include(pr => pr.Book)
                .Include(pr => pr.AssignedEmployee)
                .Include(pr => pr.AssignedSeller)
                .FirstOrDefault(pr => pr.PrintingRequestId == id && pr.UserId == currentUser.Id);

            if (printingRequest == null)
            {
                return HttpNotFound();
            }

            return View(printingRequest);
        }

        private void AssignPrintRequestToNearest(PrintingRequest printRequest)
        {
            var customer = db.ApplicationUsers.Find(printRequest.UserId);
            if (customer == null) return;

            // Get customer location (simplified - using city as location)
            var customerLocation = customer.City ?? "Unknown";
            
            // Find available sellers and employees
            var availableSellers = db.ApplicationUsers
                .Where(u => u.Role == "Seller")
                .ToList();
                
            var availableEmployees = db.ApplicationUsers
                .Where(u => u.Role == "Employee")
                .ToList();

            // Combine and randomize
            var allAvailable = availableSellers.Concat(availableEmployees).ToList();
            
            if (!allAvailable.Any())
            {
                // No available assignees, keep as unassigned
                return;
            }

            // Simple random assignment (in a real system, you'd use geolocation)
            var random = new Random();
            var selectedAssignee = allAvailable[random.Next(allAvailable.Count)];
            
            // Assign based on role
            var isSeller = selectedAssignee.Role == "Seller";
            
            if (isSeller)
            {
                printRequest.AssignedSellerId = selectedAssignee.Id;
            }
            else
            {
                printRequest.AssignedEmployeeId = selectedAssignee.Id;
            }
            
            printRequest.AssignedDate = DateTime.Now;
            printRequest.Status = "Processing";
            
            db.SaveChanges();
            
            // Send notification email
            SendAssignmentNotification(selectedAssignee, printRequest);
        }

        private void SendAssignmentNotification(ApplicationUser assignee, PrintingRequest printRequest)
        {
            var subject = "New Print Request Assigned";
            var body = $@"
                <h3>New Print Request Assigned</h3>
                <p>Hello {assignee.UserName},</p>
                <p>A new print request has been assigned to you:</p>
                <ul>
                    <li>Request ID: #{printRequest.PrintingRequestId}</li>
                    <li>Customer: {printRequest.User?.UserName ?? "Unknown"}</li>
                    <li>Total Pages: {printRequest.TotalPages}</li>
                    <li>Fulfillment Method: {printRequest.FulfillmentMethod}</li>
                    <li>Request Date: {printRequest.RequestDate:MM/dd/yyyy HH:mm}</li>
                </ul>
                <p>Please log in to your dashboard to view and process this request.</p>
                <p>Best regards,<br>Connect2Us Team</p>
            ";
            
            _emailService.SendEmail(assignee.Email, subject, body);
        }
    }
}
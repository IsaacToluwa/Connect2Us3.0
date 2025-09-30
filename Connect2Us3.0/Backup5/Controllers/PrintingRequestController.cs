using book2us.Models;
using book2us.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

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
        public ActionResult Create([Bind(Include = "BookId,Quantity,TotalPages,FulfillmentMethod,DeliveryAddress,CustomerNotes")] PrintingRequest printingRequest)
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
                printingRequest.TotalPages = printingRequest.TotalPages > 0 ? printingRequest.TotalPages : 1;
                
                // Calculate initial costs
                printingRequest.CalculatePrintingCost();
                printingRequest.CalculateDeliveryCommission();
                
                db.PrintingRequests.Add(printingRequest);
                db.SaveChanges();
                
                // Assign to nearest seller or employee
                AssignPrintRequestToNearest(printingRequest);
                
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
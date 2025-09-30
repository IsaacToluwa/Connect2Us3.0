using book2us.Models;
using book2us.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
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

        // GET: CustomerPrinting/CreateRequest
        public ActionResult CreateRequest()
        {
            var model = new PrintingRequest
            {
                Quantity = 1,
                TotalPages = 1,
                FulfillmentMethod = "Pickup"
            };

            return View(model);
        }

        // POST: CustomerPrinting/CreateRequest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRequest([Bind(Include = "Quantity,TotalPages,FulfillmentMethod,DeliveryAddress,CustomerNotes")] PrintingRequest printingRequest, HttpPostedFileBase pdfFile)
        {
            if (ModelState.IsValid)
            {
                var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Validate PDF file
                if (pdfFile == null || pdfFile.ContentLength == 0)
                {
                    ModelState.AddModelError("pdfFile", "Please select a PDF file to print.");
                    return View(printingRequest);
                }

                if (!pdfFile.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("pdfFile", "Only PDF files are allowed.");
                    return View(printingRequest);
                }

                // Create a virtual "Printing Service" book entry for checkout integration
                var printingServiceBook = new Book
                {
                    Title = $"PDF Printing Service - {pdfFile.FileName}",
                    Author = "Book2Us Printing",
                    Description = $"Printing service for {printingRequest.TotalPages} pages",
                    Price = printingRequest.TotalPages * 3.00m, // R3 per page
                    ISBN = "PRINT-" + DateTime.Now.Ticks,
                    Category = "Printing Service",
                    Condition = "New",
                    SellerId = 1, // System seller ID
                    SellerUserName = "system@book2us.com"
                };

                db.Books.Add(printingServiceBook);
                db.SaveChanges();

                // Add to shopping cart and redirect to checkout
                var cart = GetOrCreateCart();
                var orderDetail = new OrderDetail
                {
                    BookId = printingServiceBook.Id,
                    Book = printingServiceBook,
                    Quantity = printingRequest.Quantity,
                    Price = printingServiceBook.Price
                };

                cart.CartItems.Add(orderDetail);
                // Store cart in session with the correct key
                const string CartSessionKey = "CartId";
                Session[CartSessionKey] = cart;

                // Store printing request details in session for checkout
                Session["PrintingRequestDetails"] = new
                {
                    TotalPages = printingRequest.TotalPages,
                    FulfillmentMethod = printingRequest.FulfillmentMethod,
                    DeliveryAddress = printingRequest.DeliveryAddress,
                    CustomerNotes = printingRequest.CustomerNotes,
                    FileName = pdfFile.FileName
                };

                TempData["SuccessMessage"] = "PDF added to cart! Please complete checkout to submit your printing request.";
                return RedirectToAction("Checkout", "ShoppingCart");
            }

            return View(printingRequest);
        }

        private ShoppingCart GetOrCreateCart()
        {
            const string CartSessionKey = "CartId";
            ShoppingCart cart = Session[CartSessionKey] as ShoppingCart;
            if (cart == null)
            {
                cart = new ShoppingCart();
                cart.ShoppingCartId = HttpContext.Session.SessionID;
                cart.CartItems = new List<OrderDetail>();
            }
            return cart;
        }
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
using book2us.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;

namespace book2us.Controllers
{
    public class SellerDashboardViewModel
    {
        public List<Book> MyBooks { get; set; }
        public List<PrintingRequest> AssignedPrintRequests { get; set; }
        public List<Order> RecentOrders { get; set; }
        public SellerPaymentSummary PaymentSummary { get; set; }
    }

    public class SellerPaymentSummary
    {
        public decimal TotalBookSales { get; set; }
        public decimal TotalPrintRevenue { get; set; }
        public decimal TotalCommission { get; set; }
        public int CompletedOrders { get; set; }
        public int CompletedPrintJobs { get; set; }
    }
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        private Book2UsContext db = new Book2UsContext();

        // GET: Seller
        public ActionResult Index()
        {
            var sellerId = GetCurrentSellerId();
            var viewModel = new SellerDashboardViewModel
            {
                MyBooks = db.Books.Where(b => b.SellerUserName == User.Identity.Name).Take(5).ToList(),
                AssignedPrintRequests = db.PrintingRequests
                    .Where(pr => pr.AssignedSellerId == sellerId && pr.Status != "Completed" && pr.Status != "Cancelled")
                    .OrderBy(pr => pr.RequestDate)
                    .Take(10)
                    .ToList(),
                RecentOrders = db.Orders
                    .Include(o => o.OrderDetails.Select(od => od.Book))
                    .Where(o => o.OrderDetails.Any(od => od.Book.SellerUserName == User.Identity.Name))
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToList(),
                PaymentSummary = CalculateSellerPaymentSummary(sellerId)
            };
            
            return View(viewModel);
        }

        // GET: /Seller/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null || book.SellerUserName != User.Identity.Name)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // GET: /Seller/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Seller/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Title,Author,Description,Price,ISBN,Condition,Category,PageCount,Language,Publisher,PublicationYear,Tags")] Book book)
        {
            if (ModelState.IsValid)
            {
                book.SellerUserName = User.Identity.Name;
                book.CreatedDate = DateTime.Now;
                book.UpdatedDate = DateTime.Now;
                book.IsAvailable = true;
                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(book);
        }

        // GET: /Seller/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null || book.SellerUserName != User.Identity.Name)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: /Seller/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Author,Description,Price,SellerUserName")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(book);
        }

        // GET: /Seller/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null || book.SellerUserName != User.Identity.Name)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: /Seller/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: /Seller/Orders
        public ActionResult Orders()
        {
            var orders = db.Orders
                .Include(o => o.OrderDetails.Select(od => od.Book))
                .Where(o => o.OrderDetails.Any(od => od.Book.SellerUserName == User.Identity.Name))
                .ToList();
            return View(orders);
        }

        // GET: /Seller/OrderDetails/5
        public ActionResult OrderDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Include(o => o.OrderDetails.Select(od => od.Book))
                .SingleOrDefault(o => o.OrderId == id);
            if (order == null || !order.OrderDetails.Any(od => od.Book.SellerUserName == User.Identity.Name))
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: /Seller/PrintingRequests
        public ActionResult PrintingRequests()
        {
            var sellerId = GetCurrentSellerId();
            var printingRequests = db.PrintingRequests
                .Where(pr => pr.AssignedSellerId == sellerId)
                .Include(pr => pr.User)
                .Include(pr => pr.Book)
                .OrderBy(pr => pr.Status)
                .ThenBy(pr => pr.RequestDate)
                .ToList();
            
            return View(printingRequests);
        }

        // GET: /Seller/PrintRequestDetails/5
        public ActionResult PrintRequestDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var sellerId = GetCurrentSellerId();
            var printRequest = db.PrintingRequests
                .Include(pr => pr.User)
                .Include(pr => pr.Book)
                .Include(pr => pr.AssignedSeller)
                .FirstOrDefault(pr => pr.PrintingRequestId == id);
            
            if (printRequest == null || printRequest.AssignedSellerId != sellerId)
            {
                return HttpNotFound();
            }
            
            return View(printRequest);
        }

        // POST: /Seller/UpdatePrintRequestStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePrintRequestStatus(int id, string status, string notes = "")
        {
            var sellerId = GetCurrentSellerId();
            var printRequest = db.PrintingRequests.Find(id);
            
            if (printRequest == null || printRequest.AssignedSellerId != sellerId)
            {
                TempData["ErrorMessage"] = "Print request not found or not assigned to you.";
                return RedirectToAction("PrintingRequests");
            }
            
            // Validate status transitions
            if (status == "Ready" && printRequest.Status == "Processing")
            {
                printRequest.Status = "Ready";
                printRequest.ReadyDate = DateTime.Now;
                
                // Calculate final costs
                printRequest.CalculatePrintingCost();
                printRequest.CalculateDeliveryCommission();
                
                if (!string.IsNullOrEmpty(notes))
                {
                    printRequest.EmployeeNotes = notes;
                }
                
                TempData["SuccessMessage"] = "Print request marked as ready for pickup.";
            }
            else if (status == "Completed" && printRequest.Status == "Ready" && printRequest.FulfillmentMethod == "Pickup")
            {
                printRequest.Status = "PickedUp";
                printRequest.DeliveryDate = DateTime.Now;
                
                if (!string.IsNullOrEmpty(notes))
                {
                    printRequest.EmployeeNotes = notes;
                }
                
                TempData["SuccessMessage"] = "Print request marked as picked up.";
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid status transition.";
                return RedirectToAction("PrintRequestDetails", new { id = id });
            }
            
            db.Entry(printRequest).State = EntityState.Modified;
            db.SaveChanges();
            
            return RedirectToAction("PrintingRequests");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private int GetCurrentSellerId()
        {
            var sellerUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
            return sellerUser != null ? sellerUser.Id : 0;
        }

        private SellerPaymentSummary CalculateSellerPaymentSummary(int sellerId)
        {
            var currentDate = DateTime.Now;
            var startOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
            
            // Calculate book sales
            var bookSales = db.Orders
                .Where(o => o.OrderDate >= startOfMonth && o.OrderDetails.Any(od => od.Book.SellerUserName == User.Identity.Name))
                .SelectMany(o => o.OrderDetails.Where(od => od.Book.SellerUserName == User.Identity.Name))
                .Sum(od => (decimal?)(od.Price * od.Quantity)) ?? 0m;

            // Calculate print revenue
            var printRevenue = db.PrintingRequests
                .Where(pr => pr.AssignedSellerId == sellerId && pr.Status == "Completed" && pr.RequestDate >= startOfMonth)
                .Sum(pr => (decimal?)pr.PrintingCost) ?? 0m;

            // Calculate commission (20% of delivery commission)
            var deliveryCommission = db.PrintingRequests
                .Where(pr => pr.AssignedSellerId == sellerId && pr.Status == "Completed" && pr.RequestDate >= startOfMonth)
                .Sum(pr => (decimal?)pr.DeliveryCommission) ?? 0m;

            var completedOrders = db.Orders
                .Count(o => o.OrderDate >= startOfMonth && o.OrderDetails.Any(od => od.Book.SellerUserName == User.Identity.Name));

            var completedPrintJobs = db.PrintingRequests
                .Count(pr => pr.AssignedSellerId == sellerId && pr.Status == "Completed" && pr.RequestDate >= startOfMonth);

            return new SellerPaymentSummary
            {
                TotalBookSales = bookSales,
                TotalPrintRevenue = printRevenue,
                TotalCommission = deliveryCommission * 0.2m,
                CompletedOrders = completedOrders,
                CompletedPrintJobs = completedPrintJobs
            };
        }
    }
}
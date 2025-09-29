using book2us.Models;
using book2us.Services;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;

namespace book2us.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private Book2UsContext db = new Book2UsContext();
        private readonly EmailService _emailService = new EmailService();

        // GET: Order
        public ActionResult Index()
        {
            var orders = db.Orders
                .Include(o => o.Customer)
                .Where(o => o.Username == User.Identity.Name)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        // GET: /Order/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders
                .Include(o => o.OrderDetails.Select(od => od.Book))
                .Include(o => o.Customer)
                .Include(o => o.AssignedEmployee)
                .SingleOrDefault(o => o.OrderId == id && o.Username == User.Identity.Name);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: /Order/PrintingOrders
        public ActionResult PrintingOrders()
        {
            var printingOrders = db.Orders
                .Include(o => o.Customer)
                .Where(o => o.Username == User.Identity.Name && o.IsPrintingService)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(printingOrders);
        }

        // GET: /Order/TrackOrder/5
        public ActionResult TrackOrder(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.AssignedEmployee)
                .SingleOrDefault(o => o.OrderId == id && o.Username == User.Identity.Name);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: /Order/OrderHistory
        public ActionResult OrderHistory()
        {
            var orders = db.Orders
                .Include(o => o.Customer)
                .Where(o => o.Username == User.Identity.Name)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // POST: /Order/ConfirmPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPayment(int orderId, string paymentMethod)
        {
            var order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == orderId && o.Username == User.Identity.Name);

            if (order == null)
            {
                return HttpNotFound();
            }

            // Update order status to paid
            order.Status = "Paid";
            order.PaymentMethod = paymentMethod;
            order.PaymentDate = DateTime.Now;
            
            db.SaveChanges();

            // Send payment confirmation email
            var orderType = order.IsPrintingService ? "Printing Service" : "Book Purchase";
            _emailService.SendPaymentConfirmation(
                order.Customer.Email,
                order.Customer.Username,
                order.Total,
                orderType,
                order.OrderId.ToString()
            );

            // Send seller notification if applicable
            if (order.IsPrintingService || order.OrderDetails.Any())
            {
                var sellers = db.Users.Where(u => u.Role == "Seller").ToList();
                foreach (var seller in sellers)
                {
                    _emailService.SendBookSellerNotification(
                        seller.Email,
                        seller.Username,
                        orderType,
                        order.OrderId,
                        order.Total,
                        order.Customer.Username
                    );
                }
            }

            // Send employee notification for printing services
            if (order.IsPrintingService)
            {
                var employees = db.Users.Where(u => u.Role == "Employee").ToList();
                foreach (var employee in employees)
                {
                    _emailService.SendEmployeeNotification(
                        employee.Email,
                        employee.Username,
                        order.OrderId,
                        order.Customer.Username,
                        "Printing Service"
                    );
                }
            }

            TempData["SuccessMessage"] = "Payment confirmed successfully! A confirmation email has been sent to you.";
            return RedirectToAction("Details", new { id = orderId });
        }
    }
}
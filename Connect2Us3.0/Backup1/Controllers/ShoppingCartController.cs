using book2us.Models;
using book2us.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;
using System.Web;

namespace book2us.Controllers
{
    public class ShoppingCartController : Controller
    {
        private Book2UsContext db = new Book2UsContext();
        private EmailService emailService = new EmailService();
        private const string CartSessionKey = "CartId";

        // GET: ShoppingCart
        [Authorize]
        public ActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [Authorize]
        public ActionResult AddToCart(int id)
        {
            var book = db.Books.SingleOrDefault(b => b.Id == id);
            if (book != null)
            {
                var cart = GetCart();
                var cartItem = cart.CartItems.SingleOrDefault(ci => ci.BookId == id);
                if (cartItem != null)
                {
                    cartItem.Quantity++;
                }
                else
                {
                    cart.CartItems.Add(new OrderDetail { BookId = id, Quantity = 1, Book = book });
                }
                Session[CartSessionKey] = cart;
            }
            return RedirectToAction("Index");
        }

        public ActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var cartItem = cart.CartItems.SingleOrDefault(ci => ci.BookId == id);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                }
                else
                {
                    cart.CartItems.Remove(cartItem);
                }
                Session[CartSessionKey] = cart;
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult Checkout()
        {
            // Get current user information to pre-populate shipping fields
            var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var order = new Order();
            
            if (currentUser != null)
            {
                // Pre-populate shipping fields with user profile information
                order.FirstName = currentUser.FirstName;
                order.LastName = currentUser.LastName;
                order.Email = currentUser.Email;
                order.Phone = currentUser.Phone;
                order.Address = currentUser.Address;
                order.City = currentUser.City;
                order.State = currentUser.State;
                order.PostalCode = currentUser.PostalCode;
                order.Country = currentUser.Country;
            }
            
            // Check if this is a printing service order
            var cart = GetCart();
            ViewBag.IsPrintingService = cart.CartItems.Any(item => item.Book != null && item.Book.Title.ToLower().Contains("printing"));
            
            return View(order);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Checkout(Order order, string fulfillmentMethod, int? totalPages, bool? isPrintingService)
        {
            var cart = GetCart();
            if (cart.CartItems.Count == 0)
            {
                ModelState.AddModelError("", "Sorry, your cart is empty!");
            }
            if (ModelState.IsValid)
            {
                // Get current user information
                var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (currentUser == null)
                {
                    ModelState.AddModelError("", "User not found. Please log in again.");
                    return View(order);
                }

                order.OrderDate = System.DateTime.Now;
                order.CustomerId = currentUser.Id;
                order.Username = currentUser.UserName;
                order.OrderDetails = cart.CartItems;
                order.Total = cart.CartItems.Sum(item => item.Book.Price * item.Quantity);
                
                // Handle printing service parameters
                if (isPrintingService == true)
                {
                    order.IsPrintingService = true;
                    order.PrintingStatus = "Requested";
                    order.FulfillmentMethod = fulfillmentMethod ?? "Pickup";
                    order.TotalPages = totalPages ?? 0;
                    order.CalculatePrintingCharges();
                    
                    // Add delivery charge if delivery is selected
                    if (order.FulfillmentMethod == "Delivery")
                    {
                        order.DeliveryCharge = 10.00m;
                    }
                    
                    // Auto-assign to a random employee/seller in the same location
                    order.AssignedEmployeeId = AssignRandomLocalEmployee(order.Username);
                }
                
                db.Orders.Add(order);
                await db.SaveChangesAsync();

                emailService.SendPaymentConfirmation(
                    order.Email, 
                    order.FirstName + " " + order.LastName, 
                    order.Total, 
                    order.IsPrintingService ? "Printing Service" : "Book Purchase", 
                    order.OrderId.ToString()
                );

                Session[CartSessionKey] = null;
                return RedirectToAction("Completed", new { id = order.OrderId, isPrinting = order.IsPrintingService });
            }
            
            // Re-populate ViewBag for form redisplay
            ViewBag.IsPrintingService = isPrintingService == true;
            return View(order);
        }

        public ActionResult Completed(int id, bool? isPrinting)
        {
            if (isPrinting == true)
            {
                var order = db.Orders.Include(o => o.AssignedEmployee).FirstOrDefault(o => o.OrderId == id);
                ViewBag.Order = order;
            }
            return View(id);
        }

        private ShoppingCart GetCart()
        {
            ShoppingCart cart = Session[CartSessionKey] as ShoppingCart;
            if (cart == null)
            {
                cart = new ShoppingCart();
                cart.ShoppingCartId = HttpContext.Session.SessionID;
                cart.CartItems = new List<OrderDetail>();
                Session[CartSessionKey] = cart;
            }
            return cart;
        }

        // Test action to verify field names and pre-population
        // [Authorize] - Temporarily removed for testing
        public ActionResult TestCheckout()
        {
            var order = new Order();
            
            // For testing purposes, simulate a logged-in user
            var testUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == "customer@book2us.com");
            if (testUser != null)
            {
                // Pre-populate with test user data
                order.FirstName = testUser.FirstName;
                order.LastName = testUser.LastName;
                order.Email = testUser.Email;
                order.Phone = testUser.Phone;
                order.Address = testUser.Address;
                order.City = testUser.City;
                order.State = testUser.State;
                order.PostalCode = testUser.PostalCode;
                order.Country = testUser.Country;
            }
            
            return View("Checkout", order); // Use the Checkout view for testing
        }

        private int AssignRandomLocalEmployee(string customerUsername)
        {
            var customer = db.ApplicationUsers.FirstOrDefault(u => u.UserName == customerUsername);
            if (customer == null) return 0;

            var random = new Random();
            var availableEmployees = new List<ApplicationUser>();

            // First try: same city or postal code
            availableEmployees = db.ApplicationUsers
                .Where(u => (u.Role == "Employee" || u.Role == "Seller") &&
                           (u.City == customer.City || u.PostalCode == customer.PostalCode))
                .ToList();

            // Second try: same state if no local employees
            if (!availableEmployees.Any())
            {
                availableEmployees = db.ApplicationUsers
                    .Where(u => (u.Role == "Employee" || u.Role == "Seller") &&
                               u.State == customer.State)
                    .ToList();
            }

            // Final fallback: any employee/seller
            if (!availableEmployees.Any())
            {
                availableEmployees = db.ApplicationUsers
                    .Where(u => u.Role == "Employee" || u.Role == "Seller")
                    .ToList();
            }

            // Return random employee ID, or 0 if none available
            return availableEmployees.Any() ? availableEmployees[random.Next(availableEmployees.Count)].Id : 0;
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
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
            try
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
                        cart.CartItems.Add(new OrderDetail { BookId = id, Quantity = 1, Price = book.Price, Book = book });
                    }
                    Session[CartSessionKey] = cart;
                    
                    // Add success message
                    TempData["SuccessMessage"] = $"'{book.Title}' has been added to your cart.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Book not found.";
                }
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error adding book to cart: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while adding the book to your cart.";
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult RemoveFromCart(int id)
        {
            try
            {
                var cart = GetCart();
                var cartItem = cart.CartItems.SingleOrDefault(ci => ci.BookId == id);
                
                if (cartItem != null)
                {
                    if (cartItem.Quantity > 1)
                    {
                        cartItem.Quantity--;
                        cartItem.Price = cartItem.Book.Price; // Ensure price is set correctly
                        TempData["SuccessMessage"] = $"Quantity reduced for '{cartItem.Book.Title}'.";
                    }
                    else
                    {
                        cart.CartItems.Remove(cartItem);
                        TempData["SuccessMessage"] = $"'{cartItem.Book.Title}' removed from cart.";
                    }
                    Session[CartSessionKey] = cart;
                }
                else
                {
                    TempData["ErrorMessage"] = "Item not found in cart.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error removing item from cart: {ex.Message}");
                TempData["ErrorMessage"] = "An error occurred while removing the item from your cart.";
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
        public ActionResult Checkout(Order order, string fulfillmentMethod, int? totalPages, bool? isPrintingService, string deliveryAddress, string customerNotes)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== CHECKOUT POST METHOD CALLED ===");
                System.Diagnostics.Debug.WriteLine($"fulfillmentMethod: {fulfillmentMethod}");
                System.Diagnostics.Debug.WriteLine($"totalPages: {totalPages}");
                System.Diagnostics.Debug.WriteLine($"isPrintingService: {isPrintingService}");
                System.Diagnostics.Debug.WriteLine($"deliveryAddress: {deliveryAddress}");
                System.Diagnostics.Debug.WriteLine($"Order.FirstName: {order.FirstName}");
                System.Diagnostics.Debug.WriteLine($"Order.Email: {order.Email}");
                
                var cart = GetCart();
                if (cart.CartItems.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Cart is empty - redirecting to Index");
                    ModelState.AddModelError("", "Sorry, your cart is empty!");
                    TempData["ErrorMessage"] = "Your cart is empty. Please add items before checking out.";
                    return RedirectToAction("Index");
                }
                
                // Clear any existing model state errors for optional fields
                ModelState.Remove("DeliveryAddress");
                ModelState.Remove("CustomerNotes");
                
                System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
                if (!ModelState.IsValid)
                {
                    foreach (var modelError in ModelState)
                    {
                        System.Diagnostics.Debug.WriteLine($"ModelState Error - Key: {modelError.Key}");
                        foreach (var error in modelError.Value.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"  Error: {error.ErrorMessage}");
                        }
                    }
                }
                
                if (ModelState.IsValid)
                {
                    // Get current user information
                    var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
                    if (currentUser == null)
                    {
                        ModelState.AddModelError("", "User not found. Please log in again.");
                        TempData["ErrorMessage"] = "User authentication failed. Please log in again.";
                        return RedirectToAction("Login", "Account");
                    }

                    // Calculate order total
                    decimal orderTotal = 0;
                    foreach (var item in cart.CartItems)
                    {
                        if (item.Book != null)
                        {
                            orderTotal += item.Book.Price * item.Quantity;
                        }
                    }

                    order.OrderDate = System.DateTime.Now;
                    order.CustomerId = currentUser.Id;
                    order.UserName = currentUser.UserName;
                    order.OrderDetails = cart.CartItems;
                    order.Total = orderTotal;
                    
                    // Handle printing service parameters
                    if (isPrintingService == true)
                    {
                        order.IsPrintingService = true;
                        order.PrintingStatus = "Requested";
                        order.FulfillmentMethod = fulfillmentMethod ?? "Pickup";
                        order.TotalPages = totalPages ?? 0;
                        order.DeliveryAddress = deliveryAddress;
                        order.CustomerNotes = customerNotes;
                        order.CalculatePrintingCharges();
                        
                        // Add delivery charge if delivery is selected
                        if (order.FulfillmentMethod == "Delivery")
                        {
                            order.DeliveryCharge = 10.00m;
                        }
                        
                        // Auto-assign to a random employee/seller in the same location
                        order.AssignedEmployeeId = AssignRandomLocalEmployee(order.UserName);
                    }
                    
                    db.Orders.Add(order);
                    db.SaveChanges();

                    // Send confirmation email
                    try
                    {
                        emailService.SendPaymentConfirmation(
                            order.Email, 
                            order.FirstName + " " + order.LastName, 
                            order.Total, 
                            order.IsPrintingService ? "Printing Service" : "Book Purchase", 
                            order.OrderId.ToString()
                        );
                    }
                    catch (Exception emailEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error sending confirmation email: {emailEx.Message}");
                        // Don't fail the order if email fails
                    }

                    Session[CartSessionKey] = null;
                    TempData["SuccessMessage"] = "Order placed successfully! A confirmation email has been sent to you.";
                    System.Diagnostics.Debug.WriteLine($"Order created successfully with ID: {order.OrderId}");
                    System.Diagnostics.Debug.WriteLine("Redirecting to Completed action");
                    return RedirectToAction("Completed", new { id = order.OrderId, isPrinting = order.IsPrintingService });
                }
                
                // Re-populate ViewBag for form redisplay
                ViewBag.IsPrintingService = isPrintingService == true;
                TempData["ErrorMessage"] = "Please correct the errors in the form.";
                System.Diagnostics.Debug.WriteLine("ModelState invalid - returning to view");
                return View(order);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in checkout process: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An error occurred during checkout. Please try again.";
                return RedirectToAction("Index");
            }
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
            else
            {
                // Ensure all cart items have their book information loaded
                foreach (var item in cart.CartItems)
                {
                    if (item.Book == null)
                    {
                        item.Book = db.Books.Find(item.BookId);
                    }
                }
            }
            return cart;
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

        [Authorize]
        public ActionResult TestCheckout()
        {
            System.Diagnostics.Debug.WriteLine("=== TESTCHECKOUT GET METHOD CALLED ===");
            System.Diagnostics.Debug.WriteLine("User Authenticated: " + User.Identity.IsAuthenticated);
            System.Diagnostics.Debug.WriteLine("User Name: " + User.Identity.Name);
            
            var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
            var order = new Order();
            
            if (currentUser != null)
            {
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
            
            ViewBag.IsPrintingService = true;
            return View(order);
        }

        [HttpPost]
        [Authorize]
        public ActionResult TestCheckout(Order order, string fulfillmentMethod, int? totalPages, bool? isPrintingService, string deliveryAddress, string customerNotes)
        {
            System.Diagnostics.Debug.WriteLine("=== TESTCHECKOUT POST METHOD CALLED ===");
            System.Diagnostics.Debug.WriteLine("Form data received:");
            System.Diagnostics.Debug.WriteLine($"  fulfillmentMethod: {fulfillmentMethod}");
            System.Diagnostics.Debug.WriteLine($"  totalPages: {totalPages}");
            System.Diagnostics.Debug.WriteLine($"  isPrintingService: {isPrintingService}");
            System.Diagnostics.Debug.WriteLine($"  deliveryAddress: {deliveryAddress}");
            System.Diagnostics.Debug.WriteLine($"  customerNotes: {customerNotes}");
            System.Diagnostics.Debug.WriteLine($"  Order.FirstName: {order.FirstName}");
            System.Diagnostics.Debug.WriteLine($"  Order.Email: {order.Email}");
            
            try
            {
                var cart = GetCart();
                if (cart.CartItems.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Cart is empty - creating test cart item");
                    // Create a test cart item for testing purposes
                    var testBook = db.Books.FirstOrDefault();
                    if (testBook != null)
                    {
                        cart.CartItems.Add(new OrderDetail { 
                            BookId = testBook.Id, 
                            Quantity = 1, 
                            Price = testBook.Price, 
                            Book = testBook 
                        });
                        Session[CartSessionKey] = cart;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"Cart items count: {cart.CartItems.Count}");
                
                // Clear any existing model state errors for optional fields
                ModelState.Remove("DeliveryAddress");
                ModelState.Remove("CustomerNotes");
                
                System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
                if (!ModelState.IsValid)
                {
                    foreach (var modelError in ModelState)
                    {
                        System.Diagnostics.Debug.WriteLine($"ModelState Error - Key: {modelError.Key}");
                        foreach (var error in modelError.Value.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"  Error: {error.ErrorMessage}");
                        }
                    }
                }
                
                if (ModelState.IsValid)
                {
                    // Get current user information
                    var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
                    if (currentUser == null)
                    {
                        System.Diagnostics.Debug.WriteLine("User not found");
                        ModelState.AddModelError("", "User not found. Please log in again.");
                        TempData["ErrorMessage"] = "User authentication failed. Please log in again.";
                        return View(order);
                    }

                    // Calculate order total
                    decimal orderTotal = 0;
                    foreach (var item in cart.CartItems)
                    {
                        if (item.Book != null)
                        {
                            orderTotal += item.Book.Price * item.Quantity;
                        }
                    }

                    order.OrderDate = System.DateTime.Now;
                    order.CustomerId = currentUser.Id;
                    order.UserName = currentUser.UserName;
                    order.OrderDetails = cart.CartItems;
                    order.Total = orderTotal;
                    
                    // Handle printing service parameters
                    if (isPrintingService == true)
                    {
                        order.IsPrintingService = true;
                        order.PrintingStatus = "Requested";
                        order.FulfillmentMethod = fulfillmentMethod ?? "Pickup";
                        order.TotalPages = totalPages ?? 0;
                        order.DeliveryAddress = deliveryAddress;
                        order.CustomerNotes = customerNotes;
                        order.CalculatePrintingCharges();
                        
                        // Add delivery charge if delivery is selected
                        if (order.FulfillmentMethod == "Delivery")
                        {
                            order.DeliveryCharge = 10.00m;
                        }
                        
                        // Auto-assign to a random employee/seller in the same location
                        order.AssignedEmployeeId = AssignRandomLocalEmployee(order.UserName);
                    }
                    
                    System.Diagnostics.Debug.WriteLine("Adding order to database...");
                    db.Orders.Add(order);
                    db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"Order created successfully with ID: {order.OrderId}");

                    // Send confirmation email
                    try
                    {
                        emailService.SendPaymentConfirmation(
                            order.Email, 
                            order.FirstName + " " + order.LastName, 
                            order.Total, 
                            order.IsPrintingService ? "Printing Service" : "Book Purchase", 
                            order.OrderId.ToString()
                        );
                        System.Diagnostics.Debug.WriteLine("Confirmation email sent successfully");
                    }
                    catch (Exception emailEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error sending confirmation email: {emailEx.Message}");
                        // Don't fail the order if email fails
                    }

                    Session[CartSessionKey] = null;
                    TempData["SuccessMessage"] = "Test order placed successfully! Order ID: " + order.OrderId;
                    System.Diagnostics.Debug.WriteLine("Redirecting to Completed action");
                    return RedirectToAction("Completed", new { id = order.OrderId, isPrinting = order.IsPrintingService });
                }
                
                // Re-populate ViewBag for form redisplay
                ViewBag.IsPrintingService = isPrintingService == true;
                TempData["ErrorMessage"] = "Please correct the errors in the form.";
                System.Diagnostics.Debug.WriteLine("ModelState invalid - returning to view");
                return View(order);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in test checkout process: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "An error occurred during test checkout. Please try again.";
                return View(order);
            }
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
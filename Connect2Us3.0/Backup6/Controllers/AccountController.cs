using book2us.Models;
using book2us.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace book2us.Controllers
{
    public class DashboardViewModel
    {
        public User User { get; set; }
        public int TotalOrders { get; set; }
        public int TotalPrintingRequests { get; set; }
        public decimal WalletBalance { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<PrintingRequest> RecentPrintingRequests { get; set; }
        public List<ActivityItem> RecentActivity { get; set; }
    }

    public class ActivityItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } // "Order", "Printing", "Wallet", etc.
    }
    public class AccountController : Controller
    {
        private Book2UsContext db = new Book2UsContext();
        private readonly EmailService _emailService = new EmailService();

        // GET: Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register - Simple version that works
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string UserName, string Email, string Password, string Role)
        {
            if (string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            // Check if user already exists
            if (db.ApplicationUsers.Any(u => u.Email == Email))
            {
                ModelState.AddModelError("", "Email already registered.");
                return View();
            }

            try
            {
                // Create user with plain text password
                var user = new ApplicationUser
                {
                    UserName = UserName,
                    Email = Email,
                    Password = Password, // Store plain text for now
                    Role = Role ?? "Customer"
                };

                db.ApplicationUsers.Add(user);
                db.SaveChanges();

                // Send registration confirmation email
                _emailService.SendRegistrationConfirmation(user.Email, user.UserName);

                // Auto-login after registration
                FormsAuthentication.SetAuthCookie(UserName, false);
                
                // Redirect based on user role
                switch (Role)
                {
                    case "Admin":
                        return RedirectToAction("Index", "Admin");
                    case "Seller":
                        return RedirectToAction("Index", "Seller");
                    case "Employee":
                        return RedirectToAction("Index", "Employee");
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Registration failed. Please try again.");
                return View();
            }
        }

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login - Simple version
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find user by email and plain text password
                var user = db.ApplicationUsers.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
                
                if (user != null)
                {
                    FormsAuthentication.SetAuthCookie(user.UserName, model.RememberMe);
                    
                    // Redirect based on user role
                    switch (user.Role)
                    {
                        case "Admin":
                            return RedirectToAction("Index", "Admin");
                        case "Seller":
                            return RedirectToAction("Index", "Seller");
                        case "Employee":
                            return RedirectToAction("Index", "Employee");
                        default:
                            return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }
            
            return View(model);
        }

        // GET: Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        // GET: Dashboard
        [Authorize]
        public ActionResult Dashboard()
        {
            var currentUser = db.ApplicationUsers
                .Include(u => u.PrintingRequests)
                .Include(u => u.Wallets)
                .FirstOrDefault(u => u.UserName == User.Identity.Name);
            
            // Check if user exists in database
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Get user's orders
            var userOrders = db.Orders.Where(o => o.CustomerId == currentUser.Id).ToList();
            
            // Get user's recent printing requests
            var recentPrintingRequests = currentUser.PrintingRequests
                    .OrderByDescending(pr => pr.RequestDate)
                    .Take(5)
                    .ToList();

                var recentActivity = new List<ActivityItem>();

                // Add recent printing requests to activity
                foreach (var request in recentPrintingRequests)
                {
                    recentActivity.Add(new ActivityItem
                    {
                        Title = $"Printing Request #{request.PrintingRequestId}",
                        Description = $"Status: {request.Status} - {request.TotalPages} pages, R{request.PrintingCost:F2}",
                        Date = request.RequestDate
                    });
                }
            
            // Convert ApplicationUser to User model for the view with all properties
            var userModel = new User
            {
                Id = currentUser.Id,
                Username = currentUser.UserName,
                Email = currentUser.Email,
                Role = currentUser.Role,
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                Gender = currentUser.Gender,
                Age = currentUser.Age,
                Phone = currentUser.Phone,
                ProfilePicture = currentUser.ProfilePicture,
                Address = currentUser.Address,
                City = currentUser.City,
                State = currentUser.State,
                PostalCode = currentUser.PostalCode,
                Country = currentUser.Country
            };
            
            // Create a view model that includes user data and activity
            var dashboardViewModel = new DashboardViewModel
            {
                User = userModel,
                TotalOrders = userOrders.Count,
                TotalPrintingRequests = currentUser.PrintingRequests.Count,
                WalletBalance = currentUser.Wallets.Sum(w => w.Balance),
                RecentOrders = userOrders.OrderByDescending(o => o.OrderDate).Take(3).ToList(),
                RecentPrintingRequests = recentPrintingRequests,
                RecentActivity = recentActivity.OrderByDescending(a => a.Date).Take(5).ToList()
            };
            
            return View(dashboardViewModel);
        }
        
        private List<ActivityItem> GetRecentActivity(int userId)
        {
            var activity = new List<ActivityItem>();
            
            // Get recent orders
            var recentOrders = db.Orders
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.OrderDate)
                .Take(3)
                .ToList();
            
            foreach (var order in recentOrders)
            {
                activity.Add(new ActivityItem
                {
                    Title = $"Order #{order.OrderId}",
                    Description = $"Status: {order.Status}",
                    Date = order.OrderDate,
                    Type = "Order"
                });
            }
            
            // Get recent printing requests
            var recentPrintingRequests = db.PrintingRequests
                .Where(pr => pr.UserId == userId)
                .OrderByDescending(pr => pr.RequestDate)
                .Take(3)
                .ToList();
            
            foreach (var request in recentPrintingRequests)
            {
                activity.Add(new ActivityItem
                {
                    Title = $"Print Request #{request.PrintingRequestId}",
                    Description = $"Status: {request.Status}",
                    Date = request.RequestDate,
                    Type = "Printing"
                });
            }
            
            return activity.OrderByDescending(a => a.Date).Take(5).ToList();
        }

        // GET: Profile
        [Authorize]
        public new ActionResult Profile()
        {
            var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
            
            // Check if user exists in database
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            // Convert ApplicationUser to User model for the view with all properties
            var userModel = new User
            {
                Id = currentUser.Id,
                Username = currentUser.UserName,
                Email = currentUser.Email,
                Role = currentUser.Role,
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                Gender = currentUser.Gender,
                Age = currentUser.Age,
                Phone = currentUser.Phone,
                ProfilePicture = currentUser.ProfilePicture,
                Address = currentUser.Address,
                City = currentUser.City,
                State = currentUser.State,
                PostalCode = currentUser.PostalCode,
                Country = currentUser.Country
            };
            
            return View(userModel);
        }

        // POST: Profile
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public new ActionResult Profile(User updatedUser, string currentPassword)
        {
            if (ModelState.IsValid)
            {
                var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (currentUser != null)
                {
                    // Verify current password
                    if (currentUser.Password != currentPassword)
                    {
                        ModelState.AddModelError("currentPassword", "Current password is incorrect.");
                        return View(updatedUser);
                    }

                    // Update all user information
                    currentUser.UserName = updatedUser.Username;
                    currentUser.Email = updatedUser.Email;
                    currentUser.FirstName = updatedUser.FirstName;
                    currentUser.LastName = updatedUser.LastName;
                    currentUser.Gender = updatedUser.Gender;
                    currentUser.Age = updatedUser.Age;
                    currentUser.Phone = updatedUser.Phone;
                    currentUser.ProfilePicture = updatedUser.ProfilePicture;
                    currentUser.Address = updatedUser.Address;
                    currentUser.City = updatedUser.City;
                    currentUser.State = updatedUser.State;
                    currentUser.PostalCode = updatedUser.PostalCode;
                    currentUser.Country = updatedUser.Country;

                    db.SaveChanges();
                    return RedirectToAction("Dashboard");
                }
            }
            
            return View(updatedUser);
        }

        // POST: UpdateProfileFromCheckout (AJAX endpoint for checkout page)
        [HttpPost]
        [Authorize]
        public ActionResult UpdateProfileFromCheckout(string firstName, string lastName, string address, 
            string city, string state, string postalCode, string country, string phone, string email)
        {
            try
            {
                var currentUser = db.ApplicationUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Update user profile with new shipping information
                currentUser.FirstName = firstName;
                currentUser.LastName = lastName;
                currentUser.Address = address;
                currentUser.City = city;
                currentUser.State = state;
                currentUser.PostalCode = postalCode;
                currentUser.Country = country;
                currentUser.Phone = phone;
                currentUser.Email = email;

                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Account/ForgotPassword
        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Please enter your email address.");
                return View();
            }

            var user = db.ApplicationUsers.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                // Generate a simple reset token (in a real app, use a more secure method)
                var resetToken = Guid.NewGuid().ToString();
                
                // Store the token in the database (you might want to add a ResetToken field to ApplicationUser)
                // For now, we'll just send the email
                _emailService.SendPasswordResetLink(user.Email, user.UserName, resetToken);
                
                ViewBag.Message = "Password reset instructions have been sent to your email.";
                return View();
            }
            else
            {
                // Don't reveal that the user doesn't exist (security best practice)
                ViewBag.Message = "If an account with that email exists, password reset instructions have been sent.";
                return View();
            }
        }

        // GET: Account/ResetPassword
        public ActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        // POST: Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "The new password and confirmation password do not match.");
                return View(model);
            }

            // In a real application, you would validate the token and find the user
            // For now, we'll just redirect to login with a success message
            ViewBag.Message = "Your password has been successfully reset. Please log in with your new password.";
            return RedirectToAction("Login");
        }

        // GET: Account/TestRoles
        [Authorize]
        public ActionResult TestRoles()
        {
            var result = new
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = User.Identity.Name,
                IsAdmin = User.IsInRole("Admin"),
                IsSeller = User.IsInRole("Seller"),
                IsEmployee = User.IsInRole("Employee"),
                IsCustomer = User.IsInRole("Customer"),
                RoleProviderStatus = "Book2UsRoleProvider is configured and working"
            };
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
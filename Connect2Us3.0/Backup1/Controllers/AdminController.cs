using book2us.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;

namespace book2us.Controllers
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBooks { get; set; }
        public int TotalOrders { get; set; }
        public int TotalPrintRequests { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public int PendingWithdrawals { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal MonthlyWithdrawals { get; set; }
        public List<ApplicationUser> RecentUsers { get; set; } = new List<ApplicationUser>();
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<WithdrawalRequest> PendingWithdrawalRequests { get; set; } = new List<WithdrawalRequest>();
    }

    public class AdminProfileViewModel
    {
        public ApplicationUser Admin { get; set; }
        public List<BankAccount> BankAccounts { get; set; }
        public List<WithdrawalRequest> WithdrawalHistory { get; set; }
        public decimal AvailableBalance { get; set; }
    }

    public class CreateAdminViewModel
    {
        [Required]
        public string UserName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class MockWithdrawalViewModel
    {
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public int BankAccountId { get; set; }
        
        public string Notes { get; set; }
    }
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private Book2UsContext db = new Book2UsContext();

        // GET: Admin
        public ActionResult Index()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            
            var model = new AdminDashboardViewModel
            {
                TotalUsers = db.ApplicationUsers.Count(),
                TotalBooks = db.Books.Count(),
                TotalOrders = db.Orders.Count(),
                TotalPrintRequests = db.PrintingRequests.Count(),
                
                // Calculate total revenue from orders and print requests
                TotalRevenue = db.Orders.Sum(o => (decimal?)o.TotalAmount) ?? 0,
                TotalWithdrawals = db.WithdrawalRequests.Where(w => w.Status == WithdrawalStatus.Completed).Sum(w => (decimal?)w.Amount) ?? 0,
                
                // Monthly analytics
                MonthlyRevenue = db.Orders.Where(o => o.OrderDate.Month == currentMonth && o.OrderDate.Year == currentYear).Sum(o => (decimal?)o.TotalAmount) ?? 0,
                MonthlyWithdrawals = db.WithdrawalRequests.Where(w => w.RequestedDate.Month == currentMonth && w.RequestedDate.Year == currentYear && w.Status == WithdrawalStatus.Completed).Sum(w => (decimal?)w.Amount) ?? 0,
                
                PendingWithdrawals = db.WithdrawalRequests.Count(w => w.Status == WithdrawalStatus.Pending),
                
                RecentUsers = db.ApplicationUsers.OrderByDescending(u => u.Id).Take(5).ToList(),
                RecentOrders = db.Orders.OrderByDescending(o => o.OrderDate).Take(5).ToList(),
                PendingWithdrawalRequests = db.WithdrawalRequests.Where(w => w.Status == WithdrawalStatus.Pending).OrderBy(w => w.RequestedDate).Take(10).ToList()
            };
            
            return View(model);
        }

        // GET: Admin/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.ApplicationUsers.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserName,Password,Email,Role")] ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                db.ApplicationUsers.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.ApplicationUsers.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserName,Password,Email,Role")] ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser user = db.ApplicationUsers.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ApplicationUser user = db.ApplicationUsers.Find(id);
            db.ApplicationUsers.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Admin/Profile
        public new ActionResult Profile()
        {
            var username = User.Identity.Name;
            var admin = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (admin == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new AdminProfileViewModel
            {
                Admin = admin,
                BankAccounts = db.BankAccounts.Where(b => b.UserId == admin.Id && b.IsActive).ToList(),
                WithdrawalHistory = db.WithdrawalRequests.Where(w => w.UserId == admin.Id).OrderByDescending(w => w.RequestedDate).ToList(),
                AvailableBalance = db.Wallets.Where(w => w.UserId == admin.Id).Sum(w => (decimal?)w.Balance) ?? 0
            };

            return View(model);
        }

        // GET: Admin/EditProfile
        public ActionResult EditProfile()
        {
            var username = User.Identity.Name;
            var admin = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (admin == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(admin);
        }

        // POST: Admin/EditProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfile([Bind(Include = "Id,UserName,Email,FirstName,LastName,Phone,Address,City,State,Country,Gender,Age")] ApplicationUser admin)
        {
            if (ModelState.IsValid)
            {
                var existingAdmin = db.ApplicationUsers.Find(admin.Id);
                if (existingAdmin != null && existingAdmin.UserName == User.Identity.Name)
                {
                    existingAdmin.Email = admin.Email;
                    existingAdmin.FirstName = admin.FirstName;
                    existingAdmin.LastName = admin.LastName;
                    existingAdmin.Phone = admin.Phone;
                    existingAdmin.Address = admin.Address;
                    existingAdmin.City = admin.City;
                    existingAdmin.State = admin.State;
                    existingAdmin.Country = admin.Country;
                    existingAdmin.Gender = admin.Gender;
                    existingAdmin.Age = admin.Age;
                    
                    db.SaveChanges();
                    TempData["Success"] = "Profile updated successfully";
                    return RedirectToAction("Profile");
                }
            }
            
            return View(admin);
        }

        // GET: Admin/Banking
        public ActionResult Banking()
        {
            return RedirectToAction("Index", "Banking");
        }

        // GET: Admin/MockWithdrawal
        public ActionResult MockWithdrawal()
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new MockWithdrawalViewModel
            {
                BankAccountId = 0
            };

            ViewBag.BankAccounts = new SelectList(db.BankAccounts.Where(b => b.UserId == user.Id && b.IsActive), "BankAccountId", "BankName");
            return View(model);
        }

        // POST: Admin/MockWithdrawal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MockWithdrawal(MockWithdrawalViewModel model)
        {
            var username = User.Identity.Name;
            var user = db.ApplicationUsers.SingleOrDefault(u => u.UserName == username);
            
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var availableBalance = db.Wallets.Where(w => w.UserId == user.Id).Sum(w => (decimal?)w.Balance) ?? 0;
                
                if (model.Amount <= 0)
                {
                    ModelState.AddModelError("Amount", "Withdrawal amount must be greater than zero");
                }
                else if (model.Amount > availableBalance)
                {
                    ModelState.AddModelError("Amount", "Insufficient balance");
                }
                else
                {
                    // Create mock withdrawal request
                    var withdrawalRequest = new WithdrawalRequest
                    {
                        UserId = user.Id,
                        BankAccountId = model.BankAccountId,
                        Amount = model.Amount,
                        Fee = 0, // No fee for admin
                        NetAmount = model.Amount,
                        Status = WithdrawalStatus.Completed, // Auto-complete for admin
                        Notes = model.Notes ?? "Admin mock withdrawal",
                        RequestedBy = user.UserName,
                        ProcessedBy = "System",
                        RequestedDate = DateTime.Now,
                        ProcessedDate = DateTime.Now
                    };

                    db.WithdrawalRequests.Add(withdrawalRequest);
                    
                    // Update wallet balance (mock deduction)
                    var wallet = db.Wallets.FirstOrDefault(w => w.UserId == user.Id);
                    if (wallet != null)
                    {
                        wallet.Balance -= model.Amount;
                    }
                    
                    db.SaveChanges();
                    
                    TempData["Success"] = $"Mock withdrawal of {model.Amount:C} completed successfully";
                    return RedirectToAction("Profile");
                }
            }

            ViewBag.BankAccounts = new SelectList(db.BankAccounts.Where(b => b.UserId == user.Id && b.IsActive), "BankAccountId", "BankName", model.BankAccountId);
            return View(model);
        }

        // GET: Admin/CreateAdmin
        public ActionResult CreateAdmin()
        {
            return View();
        }

        // POST: Admin/CreateAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdmin(CreateAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username or email already exists
                if (db.ApplicationUsers.Any(u => u.UserName == model.UserName))
                {
                    ModelState.AddModelError("UserName", "Username already exists");
                }
                else if (db.ApplicationUsers.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists");
                }
                else
                {
                    var newAdmin = new ApplicationUser
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        Password = model.Password, // In production, this should be hashed
                        Role = "Admin",
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Phone = model.Phone,
                        Address = model.Address,
                        City = model.City,
                        State = model.State,
                        Country = model.Country,
                        ProfilePicture = null
                    };

                    db.ApplicationUsers.Add(newAdmin);
                    db.SaveChanges();

                    // Create wallet for new admin
                    var wallet = new Wallet
                    {
                        UserId = newAdmin.Id,
                        Balance = 0,
                        Currency = "ZAR",
                        IsActive = true,
                        CreatedDate = DateTime.Now,
                        LastModified = DateTime.Now
                    };
                    db.Wallets.Add(wallet);
                    db.SaveChanges();

                    TempData["Success"] = "Admin created successfully";
                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        // GET: Admin/WithdrawalRequests
        public ActionResult WithdrawalRequests()
        {
            var withdrawalRequests = db.WithdrawalRequests
                .Include(w => w.User)
                .Include(w => w.BankAccount)
                .OrderByDescending(w => w.RequestedDate)
                .ToList();

            return View(withdrawalRequests);
        }

        // POST: Admin/ProcessWithdrawal/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessWithdrawal(int id, string action, string notes = "")
        {
            var withdrawalRequest = db.WithdrawalRequests.Find(id);
            if (withdrawalRequest == null)
            {
                return HttpNotFound();
            }

            var currentAdmin = User.Identity.Name;

            if (action == "approve")
            {
                withdrawalRequest.Status = WithdrawalStatus.Approved;
                withdrawalRequest.ProcessedBy = currentAdmin;
                withdrawalRequest.ProcessedDate = DateTime.Now;
                withdrawalRequest.Notes = notes;
            }
            else if (action == "reject")
            {
                withdrawalRequest.Status = WithdrawalStatus.Rejected;
                withdrawalRequest.ProcessedBy = currentAdmin;
                withdrawalRequest.ProcessedDate = DateTime.Now;
                withdrawalRequest.RejectionReason = notes;
                
                // Refund the amount to user's wallet
                var wallet = db.Wallets.FirstOrDefault(w => w.UserId == withdrawalRequest.UserId);
                if (wallet != null)
                {
                    wallet.Balance += withdrawalRequest.Amount;
                }
            }

            db.SaveChanges();
            
            TempData["Success"] = $"Withdrawal request {action}d successfully";
            return RedirectToAction("WithdrawalRequests");
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
using book2us.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace book2us.Models
{
    public class EmployeeDashboardViewModel
    {
        public List<PrintingRequest> AssignedPrintRequests { get; set; }
        public List<Order> DeliveryAssignments { get; set; }
        public EmployeePaymentSummary PaymentSummary { get; set; }
        public List<Order> RecentCompletedDeliveries { get; set; }
    }

    public class EmployeePaymentSummary
    {
        public decimal TotalCommissionThisMonth { get; set; }
        public int CompletedDeliveriesThisMonth { get; set; }
        public decimal AverageCommissionPerDelivery { get; set; }
        public decimal PendingCommission { get; set; }
        public List<CommissionBreakdown> CommissionBreakdowns { get; set; }
    }

    public class CommissionBreakdown
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal OrderTotal { get; set; }
        public decimal CommissionAmount { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string ServiceType { get; set; } // "Book Delivery" or "Printing Service"
    }
}

namespace book2us.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private Book2UsContext db = new Book2UsContext();

        // GET: Employee
        public ActionResult Index()
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            var currentEmployee = db.ApplicationUsers.Find(currentEmployeeId);
            
            var dashboardViewModel = new EmployeeDashboardViewModel
            {
                // Print requests assigned to this employee
                AssignedPrintRequests = db.PrintingRequests
                    .Where(pr => pr.AssignedEmployeeId == currentEmployeeId)
                    .Include(pr => pr.User)
                    .Include(pr => pr.Book)
                    .OrderBy(pr => pr.RequestDate)
                    .ToList(),
                
                // Delivery assignments
                DeliveryAssignments = db.Orders
                    .Where(o => o.IsPrintingService && 
                               o.PrintingStatus == "Ready" && 
                               o.FulfillmentMethod == "Delivery" &&
                               o.AssignedEmployeeId == currentEmployeeId)
                    .Include(o => o.Customer)
                    .OrderBy(o => o.ReadyForDeliveryDate)
                    .ToList(),
                
                // Payment summary for this employee
                PaymentSummary = CalculateEmployeePaymentSummary(currentEmployeeId),
                
                // Recent completed deliveries for commission calculation
                RecentCompletedDeliveries = db.Orders
                    .Where(o => o.IsPrintingService && 
                               (o.PrintingStatus == "Delivered" || o.PrintingStatus == "PickedUp") &&
                               o.AssignedEmployeeId == currentEmployeeId &&
                               o.DeliveryDate >= DateTime.Now.AddDays(-30))
                    .Include(o => o.Customer)
                    .OrderByDescending(o => o.DeliveryDate)
                    .Take(10)
                    .ToList()
            };
            
            return View(dashboardViewModel);
        }

        // GET: /Employee/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Include(o => o.OrderDetails.Select(od => od.Book))
                .Include(o => o.Customer)
                .Include(o => o.AssignedEmployee)
                .SingleOrDefault(o => o.OrderId == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: /Employee/PrintingOrders
        public ActionResult PrintingOrders()
        {
            var printingOrders = db.Orders
                .Where(o => o.IsPrintingService)
                .Include(o => o.Customer)
                .Include(o => o.AssignedEmployee)
                .OrderBy(o => o.PrintingStatus)
                .ThenBy(o => o.OrderDate)
                .ToList();
            return View(printingOrders);
        }

        // GET: /Employee/UpdatePrintingStatus/5
        public ActionResult UpdatePrintingStatus(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null || !order.IsPrintingService)
            {
                return HttpNotFound();
            }
            
            // Get customer location for location-based assignment
            var customer = db.ApplicationUsers.FirstOrDefault(u => u.UserName == order.UserName);
            var availableEmployees = new List<ApplicationUser>();
            
            if (customer != null)
            {
                // Get employees from same city or zip code
                availableEmployees = db.ApplicationUsers
                    .Where(u => (u.Role == "Employee" || u.Role == "Seller") &&
                               (u.City == customer.City || u.PostalCode == customer.PostalCode))
                    .ToList();
                
                // If no local employees, fall back to same state
                if (!availableEmployees.Any())
                {
                    availableEmployees = db.ApplicationUsers
                        .Where(u => (u.Role == "Employee" || u.Role == "Seller") &&
                                   u.State == customer.State)
                        .ToList();
                }
            }
            
            // If still no employees, show all employees/sellers
            if (!availableEmployees.Any())
            {
                availableEmployees = db.ApplicationUsers
                    .Where(u => u.Role == "Employee" || u.Role == "Seller")
                    .ToList();
            }
            
            ViewBag.AvailableEmployees = availableEmployees;
            
            return View(order);
        }

        // POST: /Employee/UpdatePrintingStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePrintingStatus([Bind(Include = "Id,PrintingStatus,FulfillmentMethod,AssignedEmployeeId,TotalPages")] Order order)
        {
            if (ModelState.IsValid)
            {
                var existingOrder = db.Orders.Find(order.Id);
                if (existingOrder != null && existingOrder.IsPrintingService)
                {
                    existingOrder.PrintingStatus = order.PrintingStatus;
                    existingOrder.FulfillmentMethod = order.FulfillmentMethod;
                    existingOrder.AssignedEmployeeId = order.AssignedEmployeeId;
                    existingOrder.TotalPages = order.TotalPages;
                    
                    // Update dates based on status changes
                    if (order.PrintingStatus == "Ready" && existingOrder.ReadyForDeliveryDate == null)
                    {
                        existingOrder.ReadyForDeliveryDate = DateTime.Now;
                    }
                    
                    if (order.PrintingStatus == "Delivered" || order.PrintingStatus == "PickedUp")
                    {
                        existingOrder.DeliveryDate = DateTime.Now;
                    }
                    
                    // Recalculate charges
                    existingOrder.CalculatePrintingCharges();
                    
                    db.Entry(existingOrder).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    return RedirectToAction("PrintingOrders");
                }
            }
            
            ViewBag.AvailableEmployees = db.ApplicationUsers
                .Where(u => u.Role == "Employee")
                .ToList();
            
            return View(order);
        }

        // GET: /Employee/MarkAsReady/5
        public ActionResult MarkAsReady(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null || !order.IsPrintingService)
            {
                return HttpNotFound();
            }
            
            order.PrintingStatus = "Ready";
            order.ReadyForDeliveryDate = DateTime.Now;
            
            // If delivery method and no assigned employee, assign randomly to local employee
            if (order.FulfillmentMethod == "Delivery" && (order.AssignedEmployeeId == null || order.AssignedEmployeeId == 0))
            {
                order.AssignedEmployeeId = AssignRandomLocalEmployeeForDelivery(order.UserName);
            }
            
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            
            return RedirectToAction("PrintingOrders");
        }

        // GET: /Employee/DeliveryAssignments
        public ActionResult DeliveryAssignments()
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            var currentEmployee = db.ApplicationUsers.Find(currentEmployeeId);
            
            // Assign any unassigned delivery orders to local employees randomly
            AssignUnassignedDeliveries();
            
            var assignedDeliveries = db.Orders
                .Where(o => o.IsPrintingService && 
                           o.PrintingStatus == "Ready" && 
                           o.FulfillmentMethod == "Delivery" &&
                           o.AssignedEmployeeId == currentEmployeeId)
                .Include(o => o.Customer)
                .OrderBy(o => o.ReadyForDeliveryDate)
                .ToList();
            
            return View(assignedDeliveries);
        }

        // POST: /Employee/MarkAsDelivered/5
        [HttpPost]
        public ActionResult MarkAsDelivered(int id)
        {
            var order = db.Orders.Find(id);
            if (order != null && order.IsPrintingService && order.PrintingStatus == "Ready")
            {
                order.PrintingStatus = "Delivered";
                order.DeliveryDate = DateTime.Now;
                
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }
            
            return RedirectToAction("DeliveryAssignments");
        }

        // POST: /Employee/MarkAsPickedUp/5
        [HttpPost]
        public ActionResult MarkAsPickedUp(int id)
        {
            var order = db.Orders.Find(id);
            if (order != null && order.IsPrintingService && order.PrintingStatus == "Ready" && order.FulfillmentMethod == "Pickup")
            {
                order.PrintingStatus = "PickedUp";
                order.DeliveryDate = DateTime.Now;
                
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }
            
            return RedirectToAction("PrintingOrders");
        }

        // GET: /Employee/PrintingRequests
        public ActionResult PrintingRequests()
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            var printingRequests = db.PrintingRequests
                .Where(pr => pr.AssignedEmployeeId == currentEmployeeId)
                .Include(pr => pr.User)
                .Include(pr => pr.Book)
                .OrderBy(pr => pr.Status)
                .ThenBy(pr => pr.RequestDate)
                .ToList();
            
            return View(printingRequests);
        }

        // GET: /Employee/PrintRequestDetails/5
        public ActionResult PrintRequestDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            
            var printRequest = db.PrintingRequests
                .Include(pr => pr.User)
                .Include(pr => pr.Book)
                .Include(pr => pr.AssignedEmployee)
                .FirstOrDefault(pr => pr.PrintingRequestId == id);
            
            if (printRequest == null)
            {
                return HttpNotFound();
            }
            
            // Ensure the current employee can only view their assigned requests
            var currentEmployeeId = GetCurrentEmployeeId();
            if (printRequest.AssignedEmployeeId != currentEmployeeId)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
            
            return View(printRequest);
        }

        // POST: /Employee/UpdatePrintRequestStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePrintRequestStatus(int id, string status, string notes = "")
        {
            var printRequest = db.PrintingRequests.Find(id);
            var currentEmployeeId = GetCurrentEmployeeId();
            
            if (printRequest == null || printRequest.AssignedEmployeeId != currentEmployeeId)
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
                
                TempData["SuccessMessage"] = "Print request marked as ready for " + 
                    (printRequest.FulfillmentMethod == "Delivery" ? "delivery" : "pickup") + ".";
            }
            else if (status == "Completed" && printRequest.Status == "Ready")
            {
                if (printRequest.FulfillmentMethod == "Delivery")
                {
                    printRequest.Status = "Delivered";
                    printRequest.DeliveryDate = DateTime.Now;
                    TempData["SuccessMessage"] = "Print request marked as delivered.";
                }
                else if (printRequest.FulfillmentMethod == "Pickup")
                {
                    printRequest.Status = "PickedUp";
                    printRequest.DeliveryDate = DateTime.Now;
                    TempData["SuccessMessage"] = "Print request marked as picked up.";
                }
                
                if (!string.IsNullOrEmpty(notes))
                {
                    printRequest.EmployeeNotes = notes;
                }
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

        // POST: /Employee/MarkPrintRequestReady/5
        [HttpPost]
        public ActionResult MarkPrintRequestReady(int id)
        {
            var printRequest = db.PrintingRequests.Find(id);
            if (printRequest != null && printRequest.Status == "Processing" && printRequest.AssignedEmployeeId == GetCurrentEmployeeId())
            {
                printRequest.Status = "Ready";
                printRequest.ReadyDate = DateTime.Now;
                
                // Calculate final costs
                printRequest.CalculatePrintingCost();
                printRequest.CalculateDeliveryCommission();
                
                db.Entry(printRequest).State = EntityState.Modified;
                db.SaveChanges();
                
                TempData["SuccessMessage"] = "Print request marked as ready for delivery/pickup.";
            }
            
            return RedirectToAction("Index");
        }

        private int GetCurrentEmployeeId()
        {
            var username = User.Identity.Name;
            var employee = db.ApplicationUsers.FirstOrDefault(u => u.UserName == username);
            return employee?.Id ?? 0;
        }

        private void AssignUnassignedDeliveries()
        {
            var random = new Random();
            
            // Find all unassigned delivery orders that are ready
            var unassignedDeliveries = db.Orders
                .Where(o => o.IsPrintingService && 
                           o.PrintingStatus == "Ready" && 
                           o.FulfillmentMethod == "Delivery" &&
                           (o.AssignedEmployeeId == null || o.AssignedEmployeeId == 0))
                .Include(o => o.Customer)
                .ToList();
            
            foreach (var order in unassignedDeliveries)
            {
                var customer = db.ApplicationUsers.FirstOrDefault(u => u.UserName == order.UserName);
                if (customer == null) continue;

                var availableEmployees = new List<ApplicationUser>();

                // First try: same city or postal code
                availableEmployees = db.ApplicationUsers
                    .Where(u => u.Role == "Employee" &&
                               (u.City == customer.City || u.PostalCode == customer.PostalCode))
                    .ToList();

                // Second try: same state if no local employees
                if (!availableEmployees.Any())
                {
                    availableEmployees = db.ApplicationUsers
                        .Where(u => u.Role == "Employee" &&
                                   u.State == customer.State)
                        .ToList();
                }

                // Final fallback: any employee
                if (!availableEmployees.Any())
                {
                    availableEmployees = db.ApplicationUsers
                        .Where(u => u.Role == "Employee")
                        .ToList();
                }

                // Assign randomly to one of the available employees
                if (availableEmployees.Any())
                {
                    var assignedEmployee = availableEmployees[random.Next(availableEmployees.Count)];
                    order.AssignedEmployeeId = assignedEmployee.Id;
                    
                    db.Entry(order).State = EntityState.Modified;
                }
            }
            
            if (unassignedDeliveries.Any())
            {
                db.SaveChanges();
            }
        }

        private int AssignRandomLocalEmployeeForDelivery(string customerUsername)
        {
            var random = new Random();
            var customer = db.ApplicationUsers.FirstOrDefault(u => u.UserName == customerUsername);
            if (customer == null) return 0;

            var availableEmployees = new List<ApplicationUser>();

            // First try: same city or postal code
            availableEmployees = db.ApplicationUsers
                .Where(u => u.Role == "Employee" &&
                           (u.City == customer.City || u.PostalCode == customer.PostalCode))
                .ToList();

            // Second try: same state if no local employees
            if (!availableEmployees.Any())
            {
                availableEmployees = db.ApplicationUsers
                    .Where(u => u.Role == "Employee" &&
                               u.State == customer.State)
                    .ToList();
            }

            // Final fallback: any employee
            if (!availableEmployees.Any())
            {
                availableEmployees = db.ApplicationUsers
                    .Where(u => u.Role == "Employee")
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

        private EmployeePaymentSummary CalculateEmployeePaymentSummary(int employeeId)
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            
            // Get completed deliveries for this employee in the current month
            var completedDeliveries = db.Orders
                .Where(o => o.AssignedEmployeeId == employeeId &&
                           (o.PrintingStatus == "Delivered" || o.PrintingStatus == "PickedUp") &&
                           o.DeliveryDate.HasValue &&
                           o.DeliveryDate.Value.Month == currentMonth &&
                           o.DeliveryDate.Value.Year == currentYear)
                .Include(o => o.Customer)
                .ToList();

            var commissionBreakdowns = new List<CommissionBreakdown>();
            decimal totalCommission = 0;

            foreach (var delivery in completedDeliveries)
            {
                var commission = CalculateCommission(delivery);
                totalCommission += commission;

                commissionBreakdowns.Add(new CommissionBreakdown
                {
                    OrderId = delivery.OrderId,
                    CustomerName = (!string.IsNullOrEmpty(delivery.Customer?.FirstName) && !string.IsNullOrEmpty(delivery.Customer?.LastName)) 
                        ? delivery.Customer.FirstName + " " + delivery.Customer.LastName 
                        : delivery.Customer?.UserName ?? "Unknown",
                    OrderTotal = delivery.Total,
                    CommissionAmount = commission,
                    DeliveryDate = delivery.DeliveryDate.Value,
                    ServiceType = delivery.IsPrintingService ? "Printing Service" : "Book Delivery"
                });
            }

            // Calculate pending commission (deliveries that are ready but not yet completed)
            var pendingDeliveries = db.Orders
                .Where(o => o.AssignedEmployeeId == employeeId &&
                           o.PrintingStatus == "Ready" &&
                           o.FulfillmentMethod == "Delivery")
                .ToList();

            decimal pendingCommission = pendingDeliveries.Sum(d => CalculateCommission(d));

            return new EmployeePaymentSummary
            {
                TotalCommissionThisMonth = totalCommission,
                CompletedDeliveriesThisMonth = completedDeliveries.Count,
                AverageCommissionPerDelivery = completedDeliveries.Any() ? totalCommission / completedDeliveries.Count : 0,
                PendingCommission = pendingCommission,
                CommissionBreakdowns = commissionBreakdowns.OrderByDescending(cb => cb.DeliveryDate).ToList()
            };
        }

        private decimal CalculateCommission(Order order)
        {
            // 20% commission on the total order value
            return order.Total * 0.20m;
        }
    }
}
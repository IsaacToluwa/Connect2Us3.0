using book2us.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace book2us.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private Book2UsContext db = new Book2UsContext();

        // GET: Employee
        public ActionResult Index()
        {
            var orders = db.Orders.Include(o => o.Customer).ToList();
            return View(orders);
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
            var customer = db.ApplicationUsers.FirstOrDefault(u => u.UserName == order.Username);
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
                        
                        // Auto-assign delivery employee if delivery method selected and no employee assigned
                        if (order.FulfillmentMethod == "Delivery" && (order.AssignedEmployeeId == null || order.AssignedEmployeeId == 0))
                        {
                            order.AssignedEmployeeId = AssignRandomLocalEmployeeForDelivery(existingOrder.Username);
                            existingOrder.AssignedEmployeeId = order.AssignedEmployeeId;
                        }
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
                order.AssignedEmployeeId = AssignRandomLocalEmployeeForDelivery(order.Username);
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
                var customer = db.ApplicationUsers.FirstOrDefault(u => u.UserName == order.Username);
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
    }
}
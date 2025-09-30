using book2us.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace book2us.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private Book2UsContext db = new Book2UsContext();

        // GET: Admin
        public ActionResult Index()
        {
            return View(db.ApplicationUsers.ToList());
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Admin/ManagePrintingOrders
        public ActionResult ManagePrintingOrders()
        {
            var printingOrders = db.PrintingRequests
                .Include(p => p.Book)
                .Include(p => p.User)
                .Include(p => p.AssignedEmployee)
                .OrderByDescending(p => p.RequestDate)
                .ToList();
            
            // Get list of employees for assignment dropdown
            var employees = db.ApplicationUsers
                .Where(u => u.Role == "Employee")
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.UserName
                })
                .ToList();
            
            ViewBag.Employees = employees;
            
            return View(printingOrders);
        }

        // GET: Admin/AllPrintingOrders
        public ActionResult AllPrintingOrders()
        {
            var printingOrders = db.PrintingRequests
                .Include(p => p.Book)
                .Include(p => p.User)
                .Include(p => p.AssignedEmployee)
                .OrderByDescending(p => p.RequestDate)
                .ToList();
            
            // Get list of employees for assignment dropdown
            var employees = db.ApplicationUsers
                .Where(u => u.Role == "Employee")
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.UserName
                })
                .ToList();
            
            ViewBag.Employees = employees;
            
            return View(printingOrders);
        }

        // POST: Admin/AssignDelivery
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignDelivery(int id, int employeeId)
        {
            var printingRequest = db.PrintingRequests.Find(id);
            if (printingRequest != null)
            {
                printingRequest.AssignedEmployeeId = employeeId;
                printingRequest.Status = "Assigned";
                db.SaveChanges();
                
                TempData["SuccessMessage"] = "Delivery assigned to employee successfully.";
            }
            
            return RedirectToAction("ManagePrintingOrders");
        }
    }
}
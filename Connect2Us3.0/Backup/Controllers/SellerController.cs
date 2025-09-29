using book2us.Models;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;

namespace book2us.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        private Book2UsContext db = new Book2UsContext();

        // GET: Seller
        public ActionResult Index()
        {
            var books = db.Books.Where(b => b.SellerUserName == User.Identity.Name);
            return View(books.ToList());
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
        public ActionResult Create([Bind(Include="Title,Author,Description,Price")] Book book)
        {
            if (ModelState.IsValid)
            {
                book.SellerUserName = User.Identity.Name;
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
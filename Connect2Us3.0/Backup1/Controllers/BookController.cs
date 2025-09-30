using book2us.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace book2us.Controllers
{
    public class BookController : Controller
    {
        private Book2UsContext db = new Book2UsContext();

        // GET: Book
        public ActionResult Index(string searchString)
        {
            try
            {
                // Check if database exists and has books
                if (db.Database.Exists())
                {
                    var books = from b in db.Books
                                select b;

                    if (!string.IsNullOrEmpty(searchString))
                    {
                        books = books.Where(s => s.Title.Contains(searchString));
                    }

                    var bookList = books.ToList();
                    return View(bookList);
                }
                else
                {
                    // Return empty list if database doesn't exist
                    return View(new List<Book>());
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine("Error in Book Index: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
                // Return empty list on error
                return View(new List<Book>());
            }
        }

        // GET: Book/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // GET: Book/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Title,Author,Description,Price,ISBN,Condition,Category,SellerId,SellerUserName")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(book);
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
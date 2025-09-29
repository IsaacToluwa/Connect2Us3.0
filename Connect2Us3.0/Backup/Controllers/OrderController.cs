using book2us.Models;
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

        // GET: Order
        public ActionResult Index()
        {
            var orders = db.Orders.Where(o => o.Username == User.Identity.Name);
            return View(orders.ToList());
        }

        // GET: /Order/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Include(o => o.OrderDetails.Select(od => od.Book))
                .SingleOrDefault(o => o.OrderId == id && o.Username == User.Identity.Name);
            if (order == null)
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
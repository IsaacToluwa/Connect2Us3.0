using book2us.Models;
using book2us.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net;
using System.Data.Entity;

namespace book2us.Controllers
{
    public class ShoppingCartController : Controller
    {
        private Book2UsContext db = new Book2UsContext();
        private EmailService emailService = new EmailService();
        private const string CartSessionKey = "CartId";

        // GET: ShoppingCart
        public ActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

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

        public ActionResult Checkout()
        {
            return View(new Order());
        }

        [HttpPost]
        public async Task<ActionResult> Checkout(Order order)
        {
            var cart = GetCart();
            if (cart.CartItems.Count == 0)
            {
                ModelState.AddModelError("", "Sorry, your cart is empty!");
            }
            if (ModelState.IsValid)
            {
                order.OrderDate = System.DateTime.Now;
                order.OrderDetails = cart.CartItems;
                order.Total = cart.CartItems.Sum(item => item.Book.Price * item.Quantity);
                db.Orders.Add(order);
                await db.SaveChangesAsync();

                await emailService.SendEmailAsync(order.Email, "Order Confirmation", $"Your order with ID {order.OrderId} has been placed successfully.");

                Session[CartSessionKey] = null;
                return RedirectToAction("Completed", new { id = order.OrderId });
            }
            return View(order);
        }

        public ActionResult Completed(int id)
        {
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
using book2us.Models;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace book2us.Controllers
{
    public class AccountController : Controller
    {
        private Book2UsContext db = new Book2UsContext();

        // GET: Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        public ActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public ActionResult Login(User user)
        {
            var usr = db.Users.FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);
            if (usr != null)
            {
                FormsAuthentication.SetAuthCookie(usr.Username, false);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }
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
            var currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            return View(currentUser);
        }

        // GET: Profile
        [Authorize]
        public ActionResult Profile()
        {
            var currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            return View(currentUser);
        }

        // POST: Profile
        [HttpPost]
        [Authorize]
        public ActionResult Profile(User updatedUser)
        {
            if (ModelState.IsValid)
            {
                var currentUser = db.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
                if (currentUser != null)
                {
                    currentUser.Email = updatedUser.Email;
                    currentUser.Username = updatedUser.Username;
                    // Don't update password here for security
                    db.SaveChanges();
                    return RedirectToAction("Dashboard");
                }
            }
            return View(updatedUser);
        }
    }
}
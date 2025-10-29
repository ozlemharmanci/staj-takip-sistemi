using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StajTakipSistemi.Models;

namespace StajTakipSistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly MongoDbContext _context;

        public AccountController(MongoDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.Find(u => u.Email == email && u.Password == password).FirstOrDefault();

            if (user == null)
            {
                ViewBag.Error = "Geçersiz giriş bilgileri!";
                return View();
            }

            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Role", user.Role);

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Admin");
            else
                return RedirectToAction("Index", "Student");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
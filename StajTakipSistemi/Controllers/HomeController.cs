using Microsoft.AspNetCore.Mvc;

namespace StajTakipSistemi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Eğer kullanıcı zaten giriş yapmışsa, rolüne göre yönlendir
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("Email")))
            {
                var role = HttpContext.Session.GetString("Role");
                if (role == "Admin")
                    return RedirectToAction("Index", "Admin");
                else
                    return RedirectToAction("Index", "Student");
            }

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
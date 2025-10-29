using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StajTakipSistemi.Models;

namespace StajTakipSistemi.Controllers
{
    public class AdminController : Controller
    {
        private readonly MongoDbContext _context;

        public AdminController(MongoDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            // Sadece silinmemiş kayıtları getir
            var stajlar = _context.Stajlar.Find(s => s.SilindiMi != true).ToList();
            return View(stajlar);
        }

        public IActionResult Onayla(string id)
        {
            var staj = _context.Stajlar.Find(s => s.Id == id).FirstOrDefault();
            if (staj != null)
            {
                staj.OnaylandiMi = true;
                _context.Stajlar.ReplaceOne(s => s.Id == id, staj);

                TempData["SuccessMessage"] = $"{staj.OgrenciEmail} adlı öğrencinin stajı onaylandı.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Reddet(string id, string silmeNedeni)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(silmeNedeni))
            {
                TempData["ErrorMessage"] = "Reddetme nedeni boş olamaz.";
                return RedirectToAction("Index");
            }

            var staj = _context.Stajlar.Find(s => s.Id == id).FirstOrDefault();
            if (staj != null)
            {
                staj.SilindiMi = true;
                staj.SilmeTarihi = DateTime.Now;
                staj.SilenKullanici = HttpContext.Session.GetString("Email");
                staj.SilmeNedeni = silmeNedeni;

                _context.Stajlar.ReplaceOne(s => s.Id == id, staj);

                TempData["WarningMessage"] = $"{staj.OgrenciEmail} adlı öğrencinin stajı reddedildi.";
            }

            return RedirectToAction("Index");
        
    }

        // Silinen kayıtları görüntüleme
        public IActionResult SilinenKayitlar()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var silinenStajlar = _context.Stajlar.Find(s => s.SilindiMi == true).ToList();
            return View(silinenStajlar);
        }

        // Silinen kaydı geri alma
        public IActionResult GeriAl(string id)
        {
            var staj = _context.Stajlar.Find(s => s.Id == id).FirstOrDefault();
            if (staj != null)
            {
                staj.SilindiMi = false;
                staj.SilmeTarihi = null;
                staj.SilenKullanici = null;
                staj.SilmeNedeni = null;

                _context.Stajlar.ReplaceOne(s => s.Id == id, staj);

                TempData["SuccessMessage"] = "Kayıt başarıyla geri alındı.";
            }
            return RedirectToAction("SilinenKayitlar");
        }

        // Kalıcı silme (hard delete)
        public IActionResult KaliciSil(string id)
        {
            var staj = _context.Stajlar.Find(s => s.Id == id).FirstOrDefault();
            if (staj != null)
            {
                _context.Stajlar.DeleteOne(s => s.Id == id);
                TempData["WarningMessage"] = "Kayıt kalıcı olarak silindi.";
            }
            return RedirectToAction("SilinenKayitlar");
        }

        public IActionResult BelgeGoruntule(string id)
        {
            var staj = _context.Stajlar.Find(s => s.Id == id).FirstOrDefault();
            if (staj == null || string.IsNullOrEmpty(staj.DosyaYolu))
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", staj.DosyaYolu.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/pdf", Path.GetFileName(filePath));
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using StajTakipSistemi.Models;

namespace StajTakipSistemi.Controllers
{
    public class StudentController : Controller
    {
        private readonly MongoDbContext _context;

        public StudentController(MongoDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email) || HttpContext.Session.GetString("Role") != "Student")
                return RedirectToAction("Login", "Account");

            // Reddedilmiş stajları kontrol et ve bildirim oluştur
            var reddedilenStajlar = _context.Stajlar
                .Find(s => s.OgrenciEmail == email && s.SilindiMi == true && s.SilenKullanici != null && !string.IsNullOrEmpty(s.SilmeNedeni))
                .ToList();

            if (reddedilenStajlar.Any())
            {
                var reddetmeMesajlari = new List<string>();
                foreach (var staj in reddedilenStajlar)
                {
                    reddetmeMesajlari.Add(
                        $"\"{staj.Firma}\" firmasına ait {staj.StajDonemi} staj talebiniz " +
                        $"{staj.SilmeTarihi?.ToString("dd.MM.yyyy")} tarihinde " +
                        $"şu nedenle reddedilmiştir: \"{staj.SilmeNedeni}\""
                    );
                }

                // Tüm reddetme mesajlarını birleştir
                ViewBag.ReddedilmeUyarisi = string.Join("<br/><br/>", reddetmeMesajlari);
            }

            // Sadece silinmemiş (aktif) stajları göster
            var stajlar = _context.Stajlar.Find(s => s.OgrenciEmail == email && s.SilindiMi != true).ToList();
            return View(stajlar);
        }

        public IActionResult StajTalebi()
        {
            return View();
        }

        [HttpPost]
        public IActionResult StajTalebi(Staj staj)
        {
            staj.OgrenciEmail = HttpContext.Session.GetString("Email");
            staj.OnaylandiMi = false;
            staj.DosyaYolu = "";
            staj.SilindiMi = false;
            staj.SilmeTarihi = null;
            staj.SilenKullanici = null;
            staj.SilmeNedeni = null;

            _context.Stajlar.InsertOne(staj);
            return RedirectToAction("Index");
        }

        public IActionResult BelgeYukle(string id)
        {
            var staj = _context.Stajlar.Find(s => s.Id == id).FirstOrDefault();
            return View(staj);
        }

        [HttpPost]
        public IActionResult BelgeYukle(string id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Lütfen bir dosya seçiniz.";
                return View();
            }

            // Sadece PDF dosyalarını kabul et
            if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
            {
                ViewBag.Error = "Sadece PDF dosyaları yükleyebilirsiniz.";
                return View();
            }

            // wwwroot/belgeler klasörünü kontrol et, yoksa oluştur
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "belgeler");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Benzersiz dosya adı oluştur
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Dosyayı kaydet
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Veritabanında dosya yolunu güncelle
            var staj = _context.Stajlar.Find(s => s.Id == id).FirstOrDefault();
            if (staj != null)
            {
                staj.DosyaYolu = "/belgeler/" + uniqueFileName;
                _context.Stajlar.ReplaceOne(s => s.Id == id, staj);
            }

            return RedirectToAction("Index");
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

        // Reddedilme bildirimlerini temizleme action'ı
        [HttpPost]
        public IActionResult BildirimleriTemizle()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email) || HttpContext.Session.GetString("Role") != "Student")
                return RedirectToAction("Login", "Account");

            // Reddedilmiş stajları getir
            var reddedilenStajlar = _context.Stajlar
                .Find(s => s.OgrenciEmail == email && s.SilindiMi == true && s.SilenKullanici != null)
                .ToList();

            // Reddedilme nedenlerini temizle (bildirimler bir daha gösterilmez)
            foreach (var staj in reddedilenStajlar)
            {
                staj.SilmeNedeni = null;
                _context.Stajlar.ReplaceOne(s => s.Id == staj.Id, staj);
            }

            return Json(new { success = true, message = "Bildirimler temizlendi." });
        }
    }
}
using Microsoft.AspNetCore.Http;
using StajTakipSistemi.Models;
using MongoDB.Driver;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// MongoDB Context
builder.Services.AddSingleton<MongoDbContext>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "StajTakip.Session";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseSession();

// Belgeler klasörünü otomatik oluştur
var belgelerKlasoru = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "belgeler");
if (!Directory.Exists(belgelerKlasoru))
{
    Directory.CreateDirectory(belgelerKlasoru);
    File.WriteAllText(Path.Combine(belgelerKlasoru, ".gitkeep"), string.Empty);
    Console.WriteLine("📁 Belgeler klasörü oluşturuldu: " + belgelerKlasoru);
}

// Test verilerini kontrol et ve ekle (sadece development ortamında)
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

        // MongoDB bağlantısını test et
        var database = context.Users.Database;
        database.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();
        Console.WriteLine(" MongoDB bağlantısı başarılı");

        // Test kullanıcılarını kontrol et ve ekle
        var userCount = context.Users.CountDocuments(Builders<User>.Filter.Empty);
        if (userCount == 0)
        {
            Console.WriteLine("👥 Test kullanıcıları ekleniyor...");

            var users = new[]
            {
                new User {
                    Email = "admin@stajtakip.com",
                    Password = "admin123",
                    Role = "Admin"
                },
                new User {
                    Email = "mehmet@stajtakip.com",
                    Password = "mehmet123",
                    Role = "Student"
                },
                new User {
                    Email = "ayse@stajtakip.com",
                    Password = "ayse123",
                    Role = "Student"
                }
            };
            context.Users.InsertMany(users);
            Console.WriteLine("✅ Test kullanıcıları eklendi");
        }
        else
        {
            Console.WriteLine($"📊 Mevcut kullanıcı sayısı: {userCount}");
        }

        // Test staj kayıtlarını kontrol et ve ekle
        var stajCount = context.Stajlar.CountDocuments(Builders<Staj>.Filter.Empty);
        if (stajCount == 0)
        {
            Console.WriteLine("📋 Test staj kayıtları ekleniyor...");

            var stajlar = new[]
            {
                new Staj
                {
                    OgrenciEmail = "mehmet@stajtakip.com",
                    StajDonemi = "2024 Yaz Dönemi",
                    Firma = "ABC Teknoloji A.Ş.",
                    BaslangicTarihi = new DateTime(2024, 6, 1),
                    BitisTarihi = new DateTime(2024, 7, 31),
                    ToplamGunSayisi = 45,
                    Ders = "Staj I",
                    SaglikSigortasiVarMi = true,
                    DosyaYolu = "/belgeler/mehmet_staj1.pdf",
                    OnaylandiMi = true,
                    SilindiMi = false,
                    SilmeTarihi = null,
                    SilenKullanici = null,
                    SilmeNedeni = null
                },
                new Staj
                {
                    OgrenciEmail = "ayse@stajtakip.com",
                    StajDonemi = "2024 Yaz Dönemi",
                    Firma = "DataSoft Bilişim Ltd. Şti.",
                    BaslangicTarihi = new DateTime(2024, 7, 1),
                    BitisTarihi = new DateTime(2024, 8, 31),
                    ToplamGunSayisi = 44,
                    Ders = "Staj I",
                    SaglikSigortasiVarMi = true,
                    DosyaYolu = "",
                    OnaylandiMi = false,
                    SilindiMi = true,
                    SilmeTarihi = new DateTime(2024, 9, 15),
                    SilenKullanici = "admin@stajtakip.com",
                    SilmeNedeni = "Eksik belge ve staj süresi yetersiz"
                },
                new Staj
                {
                    OgrenciEmail = "mehmet@stajtakip.com",
                    StajDonemi = "2024 Güz Dönemi",
                    Firma = "TechInnovation Yazılım",
                    BaslangicTarihi = new DateTime(2024, 9, 15),
                    BitisTarihi = new DateTime(2024, 10, 15),
                    ToplamGunSayisi = 22,
                    Ders = "Staj II",
                    SaglikSigortasiVarMi = false,
                    DosyaYolu = "",
                    OnaylandiMi = false,
                    SilindiMi = false,
                    SilmeTarihi = null,
                    SilenKullanici = null,
                    SilmeNedeni = null
                }
            };
            context.Stajlar.InsertMany(stajlar);
            Console.WriteLine("Test staj kayıtları eklendi");

            // Veritabanı durumunu göster
            var onaylananStajlar = context.Stajlar.CountDocuments(s => s.OnaylandiMi == true);
            var bekleyenStajlar = context.Stajlar.CountDocuments(s => s.OnaylandiMi == false && s.SilindiMi == false);
            var reddedilenStajlar = context.Stajlar.CountDocuments(s => s.SilindiMi == true);

            Console.WriteLine($"\n📊 VERİTABANI DURUMU:");
            Console.WriteLine($"   Toplam Kullanıcı: {userCount + 3}");
            Console.WriteLine($"   Toplam Staj Kaydı: {stajCount + 3}");
            Console.WriteLine($"   Onaylanmış Stajlar: {onaylananStajlar}");
            Console.WriteLine($"   Bekleyen Stajlar: {bekleyenStajlar}");
            Console.WriteLine($"   Reddedilmiş Stajlar: {reddedilenStajlar}");
        }
        else
        {
            Console.WriteLine($"📊 Mevcut staj kaydı sayısı: {stajCount}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ MongoDB bağlantı hatası: {ex.Message}");
        Console.WriteLine("💡 Lütfen MongoDB'nin çalıştığından emin olun:");
        Console.WriteLine("   - MongoDB Compass'ı açın");
        Console.WriteLine("   - Veya terminalde 'mongod' komutunu çalıştırın");
        Console.WriteLine("   - seed-data.js dosyasını manuel çalıştırmak için:");
        Console.WriteLine("     mongosh localhost:27017/StajTakip seed-data.js");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("\n🎉 Staj Takip Sistemi başlatılıyor...");
Console.WriteLine("🌐 URL: https://localhost:7107");
Console.WriteLine("🌐 URL: http://localhost:5066");
Console.WriteLine("👤 Admin Giriş: admin@stajtakip.com / admin123");
Console.WriteLine("👤 Öğrenci Giriş: mehmet@stajtakip.com / mehmet123");
Console.WriteLine("👤 Öğrenci Giriş: ayse@stajtakip.com / ayse123");
Console.WriteLine("\n🚀 Uygulama hazır!");

app.Run();
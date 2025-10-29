// Staj Takip Sistemi - MongoDB Test Verileri
// Kullanım: mongosh localhost:27017/StajTakip seed-data.js

print("Staj Takip Sistemi - Test verileri yükleniyor...");

// Veritabanını seç
db = db.getSiblingDB('StajTakip');

// Mevcut verileri temizle (opsiyonel - ilk kurulum için)
db.Users.deleteMany({});
db.Stajlar.deleteMany({});

print("Eski veriler temizlendi.");

// Kullanıcıları ekle
var users = [
    {
        "Email": "admin@stajtakip.com",
        "Password": "admin123",
        "Role": "Admin"
    },
    {
        "Email": "mehmet@stajtakip.com",
        "Password": "mehmet123",
        "Role": "Student"
    },
    {
        "Email": "ayse@stajtakip.com",
        "Password": "ayse123",
        "Role": "Student"
    }
];

var userResult = db.Users.insertMany(users);
print("Kullanıcılar eklendi: " + userResult.insertedCount + " adet");

// Staj verilerini ekle (gerçekçi tarihlerle)
var stajlar = [
    {
        "OgrenciEmail": "mehmet@stajtakip.com",
        "StajDonemi": "2024 Yaz Dönemi",
        "Firma": "ABC Teknoloji A.Ş.",
        "BaslangicTarihi": new Date("2024-06-01"),
        "BitisTarihi": new Date("2024-07-31"),
        "ToplamGunSayisi": 45,
        "Ders": "Staj I",
        "SaglikSigortasiVarMi": true,
        "DosyaYolu": "/belgeler/mehmet_staj1.pdf",
        "OnaylandiMi": true,
        "SilindiMi": false,
        "SilmeTarihi": null,
        "SilenKullanici": null,
        "SilmeNedeni": null
    },
    {
        "OgrenciEmail": "ayse@stajtakip.com",
        "StajDonemi": "2024 Yaz Dönemi",
        "Firma": "DataSoft Bilişim Ltd. Şti.",
        "BaslangicTarihi": new Date("2024-07-01"),
        "BitisTarihi": new Date("2024-08-31"),
        "ToplamGunSayisi": 44,
        "Ders": "Staj I",
        "SaglikSigortasiVarMi": true,
        "DosyaYolu": "",
        "OnaylandiMi": false,
        "SilindiMi": true,
        "SilmeTarihi": new Date("2024-09-15"),
        "SilenKullanici": "admin@stajtakip.com",
        "SilmeNedeni": "Eksik belge ve staj süresi yetersiz"
    },
    {
        "OgrenciEmail": "mehmet@stajtakip.com",
        "StajDonemi": "2024 Güz Dönemi",
        "Firma": "TechInnovation Yazılım",
        "BaslangicTarihi": new Date("2024-09-15"),
        "BitisTarihi": new Date("2024-10-15"),
        "ToplamGunSayisi": 22,
        "Ders": "Staj II",
        "SaglikSigortasiVarMi": false,
        "DosyaYolu": "",
        "OnaylandiMi": false,
        "SilindiMi": false,
        "SilmeTarihi": null,
        "SilenKullanici": null,
        "SilmeNedeni": null
    }
];

var stajResult = db.Stajlar.insertMany(stajlar);
print("Staj kayıtları eklendi: " + stajResult.insertedCount + " adet");

// Sonuçları göster
print("\n=== VERİTABANI DURUMU ===");
print("Toplam Kullanıcı: " + db.Users.countDocuments());
print("Toplam Staj Kaydı: " + db.Stajlar.countDocuments());
print("Onaylanmış Stajlar: " + db.Stajlar.countDocuments({ "OnaylandiMi": true }));
print("Bekleyen Stajlar: " + db.Stajlar.countDocuments({ "OnaylandiMi": false, "SilindiMi": false }));
print("Reddedilmiş Stajlar: " + db.Stajlar.countDocuments({ "SilindiMi": true }));

print(" Test verileri başarıyla yüklendi!");
print(" MongoDB Compass'ta görüntülemek için: mongodb://localhost:27017/StajTakip");
print(" Uygulamayı başlatmak için: dotnet run");
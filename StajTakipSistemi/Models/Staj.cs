using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace StajTakipSistemi.Models
{
    public class Staj
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string OgrenciEmail { get; set; }
        public string StajDonemi { get; set; }
        public string Firma { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public int ToplamGunSayisi { get; set; }
        public string Ders { get; set; }
        public bool SaglikSigortasiVarMi { get; set; }
        public string DosyaYolu { get; set; }
        public bool OnaylandiMi { get; set; }

        // Soft Delete Alanları
        public bool SilindiMi { get; set; }
        public DateTime? SilmeTarihi { get; set; }
        public string SilenKullanici { get; set; }
        public string SilmeNedeni { get; set; }
    }
}
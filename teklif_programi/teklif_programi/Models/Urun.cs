// Gerekli isim alanları: Veri doğrulama, veritabanı şeması ve veri bağlama için
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

// Teklif programı model sınıflarının isim alanı
namespace teklif_programi.Models
{
    // Urun: Sistemdeki ürünleri temsil eden model sınıfı
    public class Urun
    {
        // UrunId: Ürünün benzersiz kimliği (primary key)
        public int UrunId { get; set; }

        // UrunKodu: Ürünün kodu (varsayılan boş string)
        public string UrunKodu { get; set; } = string.Empty;

        // Kategori: Ürünün kategorisi (varsayılan boş string)
        public string Kategori { get; set; } = string.Empty;

        // UrunAciklamasi: Ürünün açıklaması (varsayılan boş string)
        public string UrunAciklamasi { get; set; } = string.Empty;

        // UrunAciklamasiEn: Ürünün İngilizce açıklaması (varsayılan boş string)
        public string UrunAciklamasiEn { get; set; } = string.Empty;

        // Adet: Ürün miktarı, veritabanına kaydedilmez (NotMapped), genellikle geçici veya UI için kullanılır
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int Adet { get; set; }

        // BirimFiyat: Ürünün birim satış fiyatı
        public decimal BirimFiyat { get; set; }

        // MaliyetFiyati: Ürünün maliyet fiyatı
        public decimal MaliyetFiyati { get; set; }

        // FiyatTL: Ürün fiyatının TL cinsinden değeri
        public decimal FiyatTL { get; set; }

        // FiyatUSD: Ürün fiyatının USD cinsinden değeri
        public decimal FiyatUSD { get; set; }

        // FiyatEUR: Ürün fiyatının EUR cinsinden değeri
        public decimal FiyatEUR { get; set; }

        // EklenmeTarihi: Ürünün sisteme eklendiği tarih
        public DateTime EklenmeTarihi { get; set; }

        // TeklifUrunleri: Ürünün bağlı olduğu teklif ürünlerinin listesi (1-N ilişki)
        public virtual ICollection<TeklifUrun> TeklifUrunleri { get; set; } = new List<TeklifUrun>();
    }
}
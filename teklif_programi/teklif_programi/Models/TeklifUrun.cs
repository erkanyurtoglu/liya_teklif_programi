using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    // TeklifUrun: Bir teklifin içindeki ürünleri temsil eden model
    public class TeklifUrun
    {
        // TeklifUrunId: Veritabanında kaydın benzersiz kimliği (primary key)
        public int TeklifUrunId { get; set; }

        // TeklifId: Ürünün bağlı olduğu teklifin kimliği (foreign key)
        public int TeklifId { get; set; }

        // UrunId: Ürünün kimliği (foreign key)
        public int UrunId { get; set; }

        // Adet: Teklifteki ürün miktarı
        public int Adet { get; set; }

        // BirimFiyat: Ürünün indirimsiz birim fiyatı
        public decimal BirimFiyat { get; set; }

        // IndirimliBirimFiyat: İndirimli birim fiyat
        public decimal IndirimliBirimFiyat { get; set; }

        // ToplamTutar: Ürün toplam fiyatı (Adet * IndirimliBirimFiyat)
        public decimal ToplamTutar { get; set; }

        // Opsiyonel bırakılarak geçmiş kayıtlarda değer zorunlu tutulmaz.
        public decimal? MaliyetFiyati { get; set; }

        // UrunAciklamasi: Teklif kaydına özgü (güncellenebilir) açıklama
        public string? UrunAciklamasi { get; set; }

        // UrunAciklamasiTr: Teklif kaydına özgü Türkçe açıklama
        public string? UrunAciklamasiTr { get; set; }

        // UrunAciklamasiEn: Teklif kaydına özgü İngilizce açıklama
        public string? UrunAciklamasiEn { get; set; }

        // ParaBirimi: Fiyatın para birimi (TL, USD, EUR)
        public decimal ParaBirimi { get; set; }

        // FiyatTL: Fiyatın TL cinsinden değeri
        public decimal FiyatTL { get; set; }

        // FiyatUSD: Fiyatın USD cinsinden değeri
        public decimal FiyatUSD { get; set; }

        // FiyatEUR: Fiyatın EUR cinsinden değeri
        public decimal FiyatEUR { get; set; }

        // Tamamlandi: Ürünün üretim sürecinin tamamlanıp tamamlanmadığını belirtir
        public bool? Tamamlandi { get; set; } 

        // UretimNotu: Ürünün üretim süreciyle ilgili not
        public string? UretimNotu { get; set; }

        // Teklif: Bağlı teklif nesnesi (lazy loading için virtual)
        public virtual Teklif Teklif { get; set; } = null!;

        // Urun: Bağlı ürün nesnesi (lazy loading için virtual)
        public virtual Urun Urun { get; set; } = null!;


        // Görüntüleme için biçimlendirilmiş fiyat alanları
        [NotMapped]
        public string BirimFiyatText { get; set; } = string.Empty;

        [NotMapped]
        public string IndirimliBirimFiyatText { get; set; } = string.Empty;

        [NotMapped]
        public string MaliyetFiyatText { get; set; } = string.Empty;
    }
}

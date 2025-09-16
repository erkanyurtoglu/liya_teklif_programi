using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    public class Teklif
    // Teklif bilgilerini temsil eden model sınıfı.
    {
        public int TeklifId { get; set; }
        // Teklifin benzersiz kimliği (primary key).

        public int MusteriId { get; set; }
        // Teklife ait müşterinin kimliği (foreign key).

        public int? PersonelId { get; set; }
        // Teklifi oluşturan personelin kimliği (foreign key, opsiyonel).

        public DateTime OlusturmaTarihi { get; set; }
        // Teklifin oluşturulma tarihi.

        public decimal GenelIndirimOrani { get; set; }
        // Teklife uygulanan genel indirim oranı.

        public decimal KdvOrani { get; set; }
        // Teklife uygulanan KDV oranı.

        public string Durum { get; set; } = "Beklemede";
        // Teklifin durumu (varsayılan: "Beklemede").

        public string? MusteriNotu { get; set; }
        // Müşteriye ait notlar (opsiyonel).

        [Required]
        [StringLength(50)]
        public string ParaBirimi { get; set; } = "TL";
        // Teklifin para birimi (varsayılan: "TL", maksimum 50 karakter).

        public string Dil { get; set; } = "TR";
        // Teklifin dili (varsayılan: "TR").


        public string IlgiliKisi { get; set; } = string.Empty;
        // Teklifte belirtilen ilgili kişi adı.

        public string IlgiliKisiTelefonu { get; set; } = string.Empty;
        // İlgili kişinin telefon numarası.

        public string IlgiliKisiEposta { get; set; } = string.Empty;
        // İlgili kişinin e-posta adresi.

        public string? TeslimatSekli { get; set; }
        // Teklifte belirtilen teslimat şekli (opsiyonel).

        public string? TeslimatYeri { get; set; }
        // Teslimatın yapılacağı yer (opsiyonel).

        public DateTime? TeslimatTarihi { get; set; }
        // Teklifte belirtilen teslimat tarihi (opsiyonel).

        public DateTime? TeslimTarihi { get; set; }
        // Gerçekleşen teslim tarihi (opsiyonel).


        public virtual Musteri Musteri { get; set; } = null!;
        // Teklife ait müşteri nesnesi (1-N ilişki).

        public virtual Personel? Personel { get; set; }
        // Teklifi oluşturan personel nesnesi (opsiyonel, 1-N ilişki).

        public virtual ICollection<TeklifUrun> TeklifUrunleri { get; set; } = new List<TeklifUrun>();
        // Teklife ait ürünlerin listesi (1-N ilişki).

        public virtual TeklifToplam? TeklifToplam { get; set; }
        // Teklife ait toplam bilgileri (1-1 ilişki, opsiyonel).


        public virtual SevkBilgileri? SevkBilgileri { get; set; }
        // Teklife ait sevk ve irsaliye bilgileri (1-1 ilişki, opsiyonel).
    }
}
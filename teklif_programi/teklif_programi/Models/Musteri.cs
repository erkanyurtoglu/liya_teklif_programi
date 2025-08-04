using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    [Table("musteriler")]  // Veritabanındaki tablo ismi
    public class Musteri  // C# sınıfı tekil ve PascalCase
    {
        [Key]
        public int musteri_id { get; set; }

        public string firma_adi { get; set; }
        public string firma_adresi { get; set; }
        public string firma_telefonu { get; set; }
        public string firma_eposta { get; set; }
        public string vergi_dairesi { get; set; }
        public string vergi_numarasi { get; set; }
        public string ilgili_kisi { get; set; }
        public string ilgili_kisi_telefonu { get; set; }
        public DateTime eklenme_tarihi { get; set; } = DateTime.Now;
    }
}

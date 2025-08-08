using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace teklif_programi.Models
{
    public class Urun
    {
        public int UrunId { get; set; }
        public string UrunKodu { get; set; } = string.Empty;
        public string Kategori { get; set; } = string.Empty;
        public string UrunAciklamasi { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public int Adet { get; set; }  // EF tarafında maplenmez

        public decimal BirimFiyat { get; set; }
        public decimal MaliyetFiyati { get; set; }
        public decimal FiyatTL { get; set; }
        public decimal FiyatUSD { get; set; }
        public decimal FiyatEUR { get; set; }
        public DateTime EklenmeTarihi { get; set; }

        public virtual ICollection<TeklifUrun> TeklifUrunleri { get; set; } = new List<TeklifUrun>();
    }
}

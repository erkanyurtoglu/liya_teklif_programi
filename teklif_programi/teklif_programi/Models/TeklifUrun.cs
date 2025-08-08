using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    public class TeklifUrun
    {
        public int TeklifUrunId { get; set; }
        public int TeklifId { get; set; }
        public int UrunId { get; set; }
        public int Adet { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal IndirimliBirimFiyat { get; set; }
        public decimal ToplamTutar { get; set; }
        public decimal ParaBirimi { get; set; }
        public decimal FiyatTL { get; set; }
        public decimal FiyatUSD { get; set; }
        public decimal FiyatEUR { get; set; }

        public virtual Teklif Teklif { get; set; } = null!;
        public virtual Urun Urun { get; set; } = null!;
    }
}

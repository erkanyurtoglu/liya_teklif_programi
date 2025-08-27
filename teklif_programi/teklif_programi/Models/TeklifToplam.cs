using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    public class TeklifToplam
    // Teklifin toplam finansal bilgilerini temsil eden model sınıfı.
    {
        public int ToplamId { get; set; }
        // Teklif toplamının benzersiz kimliği (primary key).

        public int TeklifId { get; set; }
        // İlgili teklifin kimliği (foreign key).

        public decimal IndirimliToplam { get; set; }
        // İndirim uygulandıktan sonraki toplam tutar.

        public decimal KdvTutari { get; set; }
        // Teklife uygulanan KDV tutarı.

        public decimal GenelToplam { get; set; }
        // Teklifin KDV dahil genel toplam tutarı.

        public decimal PaketlemeUcreti { get; set; }
        // Teklif için eklenen paketleme ücreti.

        public virtual Teklif Teklif { get; set; } = null!;
        // İlgili teklif nesnesi (1-1 ilişki).
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    public class TeklifToplam
    {
        public int ToplamId { get; set; }
        public int TeklifId { get; set; }
        public decimal IndirimliToplam { get; set; }
        public decimal KdvTutari { get; set; }
        public decimal GenelToplam { get; set; }

        public virtual Teklif Teklif { get; set; } = null!;
    }
}

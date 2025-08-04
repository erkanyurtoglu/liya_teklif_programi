using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    [Table("teklif_toplamlari")]
    public class TeklifToplam
    {
        [Key]
        public int toplam_id { get; set; }

        public int teklif_id { get; set; }

        public decimal indirimli_toplam { get; set; }

        public decimal kdv_tutari { get; set; }

        public decimal genel_toplam { get; set; }

        [ForeignKey("teklif_id")]
        public virtual Teklif Teklif { get; set; }
    }
}

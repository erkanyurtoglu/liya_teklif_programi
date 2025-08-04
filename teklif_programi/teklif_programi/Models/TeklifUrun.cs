using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    [Table("teklif_urunleri")]
    public class TeklifUrun
    {
        [Key]
        public int teklif_urun_id { get; set; }

        public int teklif_id { get; set; }

        public int urun_id { get; set; }

        public int adet { get; set; }

        public decimal birim_fiyat { get; set; }

        public decimal indirimli_birim_fiyat { get; set; }

        public decimal toplam_tutar { get; set; }

        [ForeignKey("teklif_id")]
        public virtual Teklif Teklif { get; set; }

        [ForeignKey("urun_id")]
        public virtual Urun Urun { get; set; }
    }
}

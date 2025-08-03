using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teklif_programi.Models
{
    public class TeklifToplam
    {
        [Key]
        [Column("toplam_id")]
        public int toplam_id { get; set; }  // Primary key

        [Column("teklif_id")]
        public int teklif_id { get; set; }  // Foreign key

        [Column("indirimli_toplam")]
        public decimal indirimli_toplam { get; set; }

        [Column("kdv_tutari")]
        public decimal kdv_tutari { get; set; }

        [Column("genel_toplam")]
        public decimal genel_toplam { get; set; }

        // Navigation property: Teklif ile 1:1 ilişki
        [ForeignKey("TeklifId")]
        public virtual Teklif Teklif { get; set; }
    }
}

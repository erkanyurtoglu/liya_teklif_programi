using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teklif_programi.Models
{
    public class TeklifUrun
    {
        [Key]
        public int teklif_urun_id { get; set; }  // Primary key

        public int teklif_id { get; set; }  // Foreign key

        public int urun_id { get; set; }  // Foreign key

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

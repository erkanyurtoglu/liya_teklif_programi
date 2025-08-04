using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    [Table("teklifler")]
    public class Teklif
    {
        [Key]
        public int teklif_id { get; set; }

        public int musteri_id { get; set; }

        public int? personel_id { get; set; } // Nullable

        public DateTime olusturma_tarihi { get; set; } = DateTime.Now;

        public decimal genel_indirim_orani { get; set; }

        public decimal kdv_orani { get; set; }

        [ForeignKey("musteri_id")]
        public virtual Musteri Musteri { get; set; }

        [ForeignKey("personel_id")]
        public virtual Personel Personel { get; set; }

        public virtual ICollection<TeklifUrun> TeklifUrunleri { get; set; } = new List<TeklifUrun>();
    }
}

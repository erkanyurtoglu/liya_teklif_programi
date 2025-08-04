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
        public int TeklifId { get; set; }

        public int MusteriId { get; set; }

        public int? PersonelId { get; set; } // Nullable

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public decimal GenelIndirimOrani { get; set; }

        public decimal KdvOrani { get; set; }

        [ForeignKey(nameof(MusteriId))]
        public virtual Musteri Musteri { get; set; } = null!;

        [ForeignKey(nameof(PersonelId))]
        public virtual Personel? Personel { get; set; }  // Nullable navigation property

        public virtual ICollection<TeklifUrun> TeklifUrunleri { get; set; } = new List<TeklifUrun>();
    }
}

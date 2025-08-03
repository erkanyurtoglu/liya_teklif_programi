using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teklif_programi.Models
{
    public class Teklif
    {
        [Key]
        public int teklif_id { get; set; }  // teklif_id ile eşleşmeli

        public int musteri_id { get; set; }  // musteri_id ile eşleşmeli

        public int personel_id { get; set; }  // personel_id ile eşleşmeli

        public DateTime olusturma_tarihi { get; set; } = DateTime.Now; // olusturma_tarihi

        public decimal genel_indirim_orani { get; set; }

        public decimal kdv_orani { get; set; }

        // Bu üç alan teklif_toplamlari tablosunda olduğundan kaldırabiliriz:
        // public decimal IndirimliAraToplam { get; set; }
        // public decimal KdvTutari { get; set; }
        // public decimal GenelToplam { get; set; }

        [ForeignKey("musteri_id")]
        public virtual Musteri Musteri { get; set; }

        [ForeignKey("personel_id")]
        public virtual Personel Personel { get; set; }


        public virtual ICollection<TeklifUrun> TeklifUrunleri { get; set; } = new List<TeklifUrun>();

        public virtual TeklifToplam TeklifToplam { get; set; }
    }
}

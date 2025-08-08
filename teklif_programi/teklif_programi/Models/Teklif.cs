using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    public class Teklif
    {
        public int TeklifId { get; set; }
        public int MusteriId { get; set; }
        public int? PersonelId { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
        public decimal GenelIndirimOrani { get; set; }
        public decimal KdvOrani { get; set; }
        public string Durum { get; set; } = "Beklemede";
        public string? MusteriNotu { get; set; }

        public virtual Musteri Musteri { get; set; } = null!;
        public virtual Personel? Personel { get; set; }

        // TeklifUrun.cs için Geçerli
        public virtual ICollection<TeklifUrun> TeklifUrunleri { get; set; } = new List<TeklifUrun>();

        //Teklif Toplam.cs için Gerekli:
        public virtual TeklifToplam? TeklifToplam { get; set; }




    }
}

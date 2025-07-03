using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teklif_programi.Models
{
    public class TeklifDetay
    {
        [Key]
        public int DetayID { get; set; }

        public int TeklifNoID { get; set; }

        public string UrunKoduID { get; set; }

        public int Adet { get; set; }

        public decimal BirimFiyat { get; set; }

        public decimal ToplamFiyat { get; set; }

        [ForeignKey("TeklifNoID")]
        public virtual Teklif Teklif { get; set; }

        [ForeignKey("UrunKoduID")]
        public virtual UrunData Urun { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teklif_programi.Models
{
    public class UrunData
    {
        [Key]
        public string UrunKoduID { get; set; } // Elle girilecek

        public string Kategori { get; set; }
        public string Aciklama { get; set; }
        public int Adet { get; set; }
        public decimal BirimSatisFiyati { get; set; }
        public decimal SatisToplamFiyati { get; set; }
        public decimal YurticiMaliyet { get; set; }
        public decimal ToplamFiyat { get; set; }
    }
}

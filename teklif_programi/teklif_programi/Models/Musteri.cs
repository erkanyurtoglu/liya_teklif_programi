using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teklif_programi.Models
{
    public class Musteri
    {
        [Key]  // 👈 Bu satırı ekle
        public int musteri_id { get; set; }

        public string firma_adi { get; set; }
        public string firma_adresi { get; set; }
        public string firma_telefonu { get; set; }
        public string firma_eposta { get; set; }
        public string vergi_dairesi { get; set; }
        public string vergi_numarasi { get; set; }
        public string ilgili_kisi { get; set; }
        public string ilgili_kisi_telefonu { get; set; }
        public DateTime eklenme_tarihi { get; set; } = DateTime.Now;



    }
}
    
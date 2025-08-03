using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teklif_programi.Models
{
    public class Personel
    {
        [Key]
        public int personel_id { get; set; }
        public string ad_soyad { get; set; }
        public string telefon { get; set; }
        public string pozisyon { get; set; }
        public string sifre {  get; set; }
        public DateTime eklenme_tarihi { get; set; } = DateTime.Now;
    }
}

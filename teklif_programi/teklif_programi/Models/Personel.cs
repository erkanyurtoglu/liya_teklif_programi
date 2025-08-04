using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    [Table("personeller")]
    public class Personel
    {
        [Key]
        public int personel_id { get; set; }
        public string ad_soyad { get; set; }
        public string telefon { get; set; }
        public string pozisyon { get; set; }
        public string sifre { get; set; }
        public DateTime eklenme_tarihi { get; set; } = DateTime.Now;
    }
}

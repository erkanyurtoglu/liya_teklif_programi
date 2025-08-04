using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    [Table("personeller")]
    public class Personel
    {
        [Key]
        public int PersonelId { get; set; }

        [Required]
        public string AdSoyad { get; set; } = string.Empty;

        [Required]
        public string Telefon { get; set; } = string.Empty;

        [Required]
        public string Pozisyon { get; set; } = string.Empty;

        [Required]
        public string Sifre { get; set; } = string.Empty;

        public DateTime EklenmeTarihi { get; set; } = DateTime.Now;
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    public class Personel
    {
        public int PersonelId { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string Pozisyon { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
        public DateTime EklenmeTarihi { get; set; }

        //TeklifConfiguration.cs için eklendi:
        public virtual ICollection<Teklif> Teklifler { get; set; } = new List<Teklif>();
    }
}

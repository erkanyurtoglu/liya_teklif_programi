using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teklif_programi.Models
{
    public class PersonelData
    {
        [Key]
        public int PersonelKoduID { get; set; }
        public string AdSoyad { get; set; }
        public string Pozisyon { get; set; }
        public string Telefon { get; set; }
        public string PersonelSifre {  get; set; }
    }
}

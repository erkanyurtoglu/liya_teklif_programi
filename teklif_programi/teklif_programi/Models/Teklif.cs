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
        public int TeklifNoID { get; set; }

        public int FirmaKoduID { get; set; }
        public int PersonelKoduID { get; set; }
        public DateTime TeklifTarihi { get; set; } = DateTime.Now;
        public decimal ToplamTutar { get; set; }

        [ForeignKey("FirmaKoduID")]
        public virtual Firma Firma { get; set; }

        [ForeignKey("PersonelKoduID")]
        public virtual PersonelData Personel { get; set; }

        public virtual ICollection<TeklifDetay> TeklifDetaylari { get; set; } = new List<TeklifDetay>();
    }
}

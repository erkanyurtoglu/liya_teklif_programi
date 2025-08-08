using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    public class Musteri
    {
        public int MusteriId { get; set; }
        public string FirmaAdi { get; set; } = string.Empty;
        public string FirmaAdresi { get; set; } = string.Empty;
        public string FirmaTelefonu { get; set; } = string.Empty;
        public string FirmaEposta { get; set; } = string.Empty;
        public string VergiDairesi { get; set; } = string.Empty;
        public string VergiNumarasi { get; set; } = string.Empty;
        public string IlgiliKisi { get; set; } = string.Empty;
        public string IlgiliKisiTelefonu { get; set; } = string.Empty;
        public DateTime EklenmeTarihi { get; set; }

        //TeklifConfiguration.cs için eklendi:

        public virtual ICollection<Teklif> Teklifler { get; set; } = new List<Teklif>();
    }
}

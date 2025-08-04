using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    [Table("musteriler")]
    public class Musteri
    {
        [Key]
        public int MusteriId { get; set; }

        [Required]
        public string FirmaAdi { get; set; } = string.Empty;

        [Required]
        public string FirmaAdresi { get; set; } = string.Empty;

        [Required]
        public string FirmaTelefonu { get; set; } = string.Empty;

        [Required]
        public string FirmaEposta { get; set; } = string.Empty;

        [Required]
        public string VergiDairesi { get; set; } = string.Empty;

        [Required]
        public string VergiNumarasi { get; set; } = string.Empty;

        [Required]
        public string IlgiliKisi { get; set; } = string.Empty;

        [Required]
        public string IlgiliKisiTelefonu { get; set; } = string.Empty;

        public DateTime EklenmeTarihi { get; set; } = DateTime.Now;
    }
}

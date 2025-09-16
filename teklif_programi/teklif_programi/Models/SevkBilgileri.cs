using System;
using System.ComponentModel.DataAnnotations;

namespace teklif_programi.Models
{
    public class SevkBilgileri
    {
        public int SevkBilgileriId { get; set; }

        public int TeklifId { get; set; }

        // Fatura Bilgileri
        [MaxLength(200)]
        public string? FaturaBasligi { get; set; }

        [MaxLength(500)]
        public string? FaturaAdresi { get; set; }

        [MaxLength(150)]
        public string? FaturaVergiDairesi { get; set; }

        [MaxLength(50)]
        public string? FaturaVergiNo { get; set; }

        [MaxLength(150)]
        public string? FaturaYetkili { get; set; }

        [MaxLength(150)]
        public string? FaturaEposta { get; set; }

        // İrsaliye Bilgileri
        [MaxLength(200)]
        public string? IrsaliyeBasligi { get; set; }

        [MaxLength(500)]
        public string? IrsaliyeAdresi { get; set; }

        [MaxLength(150)]
        public string? IrsaliyeVergiDairesi { get; set; }

        [MaxLength(50)]
        public string? IrsaliyeVergiNo { get; set; }

        [MaxLength(150)]
        public string? IrsaliyeYetkili { get; set; }

        [MaxLength(50)]
        public string? IrsaliyeTelefon { get; set; }

        [MaxLength(150)]
        public string? IrsaliyeEposta { get; set; }

        // Sipariş Bilgileri
        [MaxLength(50)]
        public string? SiparisKdv { get; set; }

        [MaxLength(150)]
        public string? FaturaSekli { get; set; }

        [MaxLength(150)]
        public string? FaturaTipi { get; set; }

        [MaxLength(100)]
        public string? Taksit { get; set; }

        [MaxLength(200)]
        public string? Garanti { get; set; }

        [MaxLength(200)]
        public string? Teslimat { get; set; }

        [MaxLength(200)]
        public string? FaturaEdilecekKisi { get; set; }

        [MaxLength(500)]
        public string? EkFaturaNotu { get; set; }

        public DateTime? SiparisTarihi { get; set; }

        public string? Aciklamalar { get; set; }

        public virtual Teklif Teklif { get; set; } = null!;
    }
}
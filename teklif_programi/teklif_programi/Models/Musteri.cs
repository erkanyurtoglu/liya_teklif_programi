using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
{
    public class Musteri
    // Müşteri bilgilerini temsil eden model sınıfı.
    {
        public int MusteriId { get; set; }
        // Müşterinin benzersiz kimliği (primary key).

        public string FirmaAdi { get; set; } = string.Empty;
        // Firmanın adı, varsayılan olarak boş string.

        public string FirmaAdresi { get; set; } = string.Empty;
        // Firmanın adresi, varsayılan olarak boş string.

        public string FirmaTelefonu { get; set; } = string.Empty;
        // Firmanın telefon numarası, varsayılan olarak boş string.

        public string FirmaEposta { get; set; } = string.Empty;
        // Firmanın e-posta adresi, varsayılan olarak boş string.

        public string VergiDairesi { get; set; } = string.Empty;
        // Firmanın vergi dairesi, varsayılan olarak boş string.

        public string VergiNumarasi { get; set; } = string.Empty;
        // Firmanın vergi numarası, varsayılan olarak boş string.

        public string IlgiliKisi { get; set; } = string.Empty;
        // Firma ile iletişim kurulan kişinin adı, varsayılan olarak boş string.

        public string IlgiliKisiTelefonu { get; set; } = string.Empty;
        // İlgili kişinin telefon numarası, varsayılan olarak boş string.

        public DateTime EklenmeTarihi { get; set; }
        // Müşterinin sisteme eklenme tarihi.

        public virtual ICollection<Teklif> Teklifler { get; set; } = new List<Teklif>();
        // Müşteriye ait tekliflerin listesi (1-N ilişki için).
    }
}

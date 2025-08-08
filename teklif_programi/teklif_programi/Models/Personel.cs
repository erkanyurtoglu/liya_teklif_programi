using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace teklif_programi.Models
// Model sınıflarının bulunduğu ad alanı.
{
    public class Personel
    // Personel bilgilerini temsil eden model sınıfı.
    {
        public int PersonelId { get; set; }
        // Personelin benzersiz kimliği (primary key).

        public string AdSoyad { get; set; } = string.Empty;
        // Personelin adı ve soyadı, varsayılan olarak boş string.

        public string Telefon { get; set; } = string.Empty;
        // Personelin telefon numarası, varsayılan olarak boş string.

        public string Pozisyon { get; set; } = string.Empty;
        // Personelin pozisyonu, varsayılan olarak boş string.

        public string Sifre { get; set; } = string.Empty;
        // Personelin şifresi (hashlenmiş), varsayılan olarak boş string.

        public DateTime EklenmeTarihi { get; set; }
        // Personelin sisteme eklenme tarihi.

        public virtual ICollection<Teklif> Teklifler { get; set; } = new List<Teklif>();
        // Personele ait tekliflerin listesi (1-N ilişki için).
    }
}

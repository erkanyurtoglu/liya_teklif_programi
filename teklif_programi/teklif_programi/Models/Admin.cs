using System;

namespace teklif_programi.Models
{
    /// <summary>
    /// Sisteme giriş yapabilecek yönetici hesaplarını temsil eder.
    /// </summary>
    public class Admin
    {
        public int AdminId { get; set; }
        // Yöneticinin benzersiz kimliği.

        public string KullaniciAdi { get; set; } = string.Empty;
        // Yöneticinin kullanıcı adı.

        public string Sifre { get; set; } = string.Empty;
        // Yöneticinin şifresi (hashlenmiş).
    }
}
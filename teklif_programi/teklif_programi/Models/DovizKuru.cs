using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace teklif_programi.Models
// Model sınıflarının bulunduğu ad alanı.
{
    public class DovizKuru
    // Döviz kuru bilgilerini temsil eden model sınıfı.
    {
        public string DovizCinsi { get; set; } = string.Empty;
        // Döviz türünü tutar (örn. USD, EUR), varsayılan olarak boş string.

        public decimal Alis { get; set; }
        // Döviz alış kurunu tutar.

        public decimal Satis { get; set; }
        // Döviz satış kurunu tutar.

        public DovizKuru() { }
        // Parametresiz yapıcı (constructor), varsayılan değerlerle nesne oluşturur.

        public DovizKuru(string dovizCinsi, decimal alis, decimal satis)
        // Parametreli yapıcı, döviz kuru nesnesini belirtilen değerlerle başlatır.
        {
            DovizCinsi = dovizCinsi;
            Alis = alis;
            Satis = satis;
        }
    }
}


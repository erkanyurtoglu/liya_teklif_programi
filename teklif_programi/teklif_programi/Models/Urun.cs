using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace teklif_programi.Models
{
    public class Urun : INotifyPropertyChanged
    {
        [Key]

        [Column("urun_id")]
        public int urun_id { get; set; } // Yeni int birincil anahtar

        [Column("urun_kodu")]
        public string urun_kodu { get; set; }  // Primary key

        [Column("kategori")]
        public string kategori { get; set; }

        [Column("urun_aciklamasi")]
        public string urun_aciklamasi { get; set; }

        private int _adet;

        [NotMapped] // Veritabanında yok, sadece UI'da kullanılacak
        public int adet
        {
            get => _adet;
            set
            {
                if (_adet != value)
                {
                    _adet = value;
                    OnPropertyChanged();
                }
            }
        }

        [Column("birim_fiyat")]
        public decimal birim_fiyat { get; set; }

        [Column("maliyet_fiyati")]
        public decimal maliyet_fiyati { get; set; }

        // INotifyPropertyChanged implementasyonu
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace teklif_programi.Models
{
    [Table("urunler")]
    public class Urun : INotifyPropertyChanged
    {
        [Key]
        public int UrunId { get; set; }  // Birincil anahtar

        public string UrunKodu { get; set; } = string.Empty;

        public string Kategori { get; set; } = string.Empty;

        public string UrunAciklamasi { get; set; } = string.Empty;

        private int _adet;
        [NotMapped]  // Veritabanında yok
        public int Adet
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

        public decimal BirimFiyat { get; set; }

        public decimal MaliyetFiyati { get; set; }

        public DateTime EklenmeTarihi { get; set; } = DateTime.Now;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

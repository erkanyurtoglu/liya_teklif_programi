using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace teklif_programi.Models
{
    public class UrunData : INotifyPropertyChanged
    {
        [Key]
        public string UrunKoduID { get; set; }

        public string Kategori { get; set; }
        public string Aciklama { get; set; }

        private int _adet;
        public int Adet
        {
            get => _adet;
            set
            {
                if (value != _adet)
                {
                    _adet = value;
                    // Adet değişince indirimi de dikkate alarak güncelle
                    SatisToplamFiyati = BirimSatisFiyati * _adet * (1 - indirim / 100m);
                    ToplamFiyat = SatisToplamFiyati;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SatisToplamFiyati));
                    OnPropertyChanged(nameof(ToplamFiyat));
                }
            }
        }

        private decimal _indirim;

        [NotMapped]
        public decimal indirim
        {
            get => _indirim;
            set
            {
                if (_indirim != value)
                {
                    _indirim = value;
                    SatisToplamFiyati = BirimSatisFiyati * Adet * (1 - _indirim / 100m);
                    ToplamFiyat = SatisToplamFiyati;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SatisToplamFiyati));
                    OnPropertyChanged(nameof(ToplamFiyat));
                }
            }
        }


        public decimal BirimSatisFiyati { get; set; }

        private decimal _satisToplamFiyati;
        public decimal SatisToplamFiyati
        {
            get => _satisToplamFiyati;
            set
            {
                if (value != _satisToplamFiyati)
                {
                    _satisToplamFiyati = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal YurticiMaliyet { get; set; }

        private decimal _toplamFiyat;
        public decimal ToplamFiyat
        {
            get => _toplamFiyat;
            set
            {
                if (value != _toplamFiyat)
                {
                    _toplamFiyat = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

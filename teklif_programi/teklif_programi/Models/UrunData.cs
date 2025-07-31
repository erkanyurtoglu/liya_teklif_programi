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
                    RecalculatePrices();
                    OnPropertyChanged();
                }
            }
        }

        private decimal _genelIndirim;
        [NotMapped]
        public decimal GenelIndirim
        {
            get => _genelIndirim;
            set
            {
                if (_genelIndirim != value)
                {
                    _genelIndirim = value;
                    RecalculatePrices();
                    OnPropertyChanged();
                }
            }
        }

        private decimal _kdvOrani;
        [NotMapped]
        public decimal KdvOrani
        {
            get => _kdvOrani;
            set
            {
                if (_kdvOrani != value)
                {
                    _kdvOrani = value;
                    RecalculatePrices();
                    OnPropertyChanged();
                }
            }
        }

        public decimal BirimSatisFiyati { get; set; }
        public decimal YurticiMaliyet { get; set; }
        public decimal IndirimliToplamFiyat => BirimSatisFiyati * (1 - GenelIndirim / 100m);

        public decimal ToplamSatisFiyati => IndirimliToplamFiyat * Adet;

        public decimal KdvDahilToplamFiyat => ToplamSatisFiyati * (1 + KdvOrani / 100m);

        private void RecalculatePrices()
        {
            OnPropertyChanged(nameof(ToplamSatisFiyati));
            OnPropertyChanged(nameof(IndirimliToplamFiyat));
            OnPropertyChanged(nameof(KdvDahilToplamFiyat));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
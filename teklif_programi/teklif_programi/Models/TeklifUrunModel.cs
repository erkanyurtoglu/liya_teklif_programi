using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace teklif_programi.Models
{
    public class TeklifUrunModel : INotifyPropertyChanged
    {
        public int UrunId { get; set; }

        public string UrunKodu { get; set; } = string.Empty;

        public string UrunAciklamasi { get; set; } = string.Empty;

        private decimal _birimFiyat;
        public decimal BirimFiyat
        {
            get => _birimFiyat;
            set
            {
                if (_birimFiyat != value)
                {
                    _birimFiyat = value;
                    OnPropertyChanged();
                    OnBirimFiyatDegisti?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private int _adet = 1;
        public int Adet
        {
            get => _adet;
            set
            {
                if (_adet != value)
                {
                    _adet = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Toplam));
                }
            }
        }

        private decimal _indirimliFiyat;
        public decimal IndirimliFiyat
        {
            get => _indirimliFiyat;
            set
            {
                if (_indirimliFiyat != value)
                {
                    _indirimliFiyat = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Toplam));
                }
            }
        }

        public decimal Toplam => Adet * IndirimliFiyat;

        public decimal FiyatTL { get; set; }
        public decimal FiyatUSD { get; set; }
        public decimal FiyatEUR { get; set; }

        public event EventHandler? OnBirimFiyatDegisti;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
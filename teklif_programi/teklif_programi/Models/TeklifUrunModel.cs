using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace teklif_programi.Models
{
    public class TeklifUrunModel : INotifyPropertyChanged
    {
        public int urun_id { get; set; }
        public string urun_kodu { get; set; }
        public string urun_aciklamasi { get; set; }

        private decimal _birim_fiyat;
        public decimal birim_fiyat
        {
            get => _birim_fiyat;
            set
            {
                if (_birim_fiyat != value)
                {
                    _birim_fiyat = value;
                    OnPropertyChanged();
                    // Birim fiyat değiştiğinde dışarıya bildirim
                    OnBirimFiyatDegisti?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private int _adet = 1;
        public int adet
        {
            get => _adet;
            set
            {
                if (_adet != value)
                {
                    _adet = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(toplam));
                }
            }
        }

        private decimal _indirimli_fiyat;
        public decimal indirimli_fiyat
        {
            get => _indirimli_fiyat;
            set
            {
                if (_indirimli_fiyat != value)
                {
                    _indirimli_fiyat = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(toplam));
                }
            }
        }

        public decimal toplam => adet * indirimli_fiyat;

        // Birim fiyat değiştiğinde ViewModel’de dinlemek için event
        public event EventHandler? OnBirimFiyatDegisti;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

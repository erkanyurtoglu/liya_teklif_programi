using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;

namespace teklif_programi.Models
{
    public class TeklifUrunModel : INotifyPropertyChanged
    {
        private int _urunId;
        private string _urunKodu = string.Empty;
        private string _urunAciklamasi = string.Empty;
        private int _adet;
        private decimal _birimFiyat;
        private decimal _indirimliFiyat;
        private decimal _fiyatTL;
        private decimal _fiyatUSD;
        private decimal _fiyatEUR;
        private string _birimFiyatText = string.Empty;
        private string _indirimliFiyatText = string.Empty;
        private string _toplamText = string.Empty;

        public int UrunId
        {
            get => _urunId;
            set { _urunId = value; OnPropertyChanged(); }
        }

        public string UrunKodu
        {
            get => _urunKodu;
            set { _urunKodu = value; OnPropertyChanged(); }
        }

        public string UrunAciklamasi
        {
            get => _urunAciklamasi;
            set { _urunAciklamasi = value; OnPropertyChanged(); }
        }

        public int Adet
        {
            get => _adet;
            set
            {
                if (_adet != value)
                {
                    _adet = value;
                    OnPropertyChanged();
                    OnBirimFiyatDegisti?.Invoke(this, EventArgs.Empty); // Adet değiştiğinde tetikle
                }
            }
        }

        public decimal BirimFiyat
        {
            get => _birimFiyat;
            set
            {
                if (_birimFiyat != value)
                {
                    _birimFiyat = value;
                    OnPropertyChanged();
                    OnBirimFiyatDegisti?.Invoke(this, EventArgs.Empty); // BirimFiyat değiştiğinde tetikle
                }
            }
        }

        public decimal IndirimliFiyat
        {
            get => _indirimliFiyat;
            set
            {
                if (_indirimliFiyat != value)
                {
                    _indirimliFiyat = value;
                    OnPropertyChanged();
                    OnBirimFiyatDegisti?.Invoke(this, EventArgs.Empty); // IndirimliFiyat değiştiğinde tetikle
                }
            }
        }

        public string BirimFiyatText
        {
            get => _birimFiyatText;
            set { _birimFiyatText = value; OnPropertyChanged(); }
        }

        public string IndirimliFiyatText
        {
            get => _indirimliFiyatText;
            set
            {
                if (_indirimliFiyatText != value)
                {
                    _indirimliFiyatText = value;
                    OnPropertyChanged();
                    // Metni decimal'e çevir ve IndirimliFiyat'ı güncelle
                    if (decimal.TryParse(value.Replace("₺", "").Replace("$", "").Replace("€", "").Trim(), NumberStyles.Currency, CultureInfo.CurrentCulture, out decimal parsedValue))
                    {
                        IndirimliFiyat = parsedValue; // Bu, OnBirimFiyatDegisti'yi tetikler
                    }
                }
            }
        }

        public string ToplamText
        {
            get => _toplamText;
            set { _toplamText = value; OnPropertyChanged(); }
        }

        public decimal FiyatTL
        {
            get => _fiyatTL;
            set { _fiyatTL = value; OnPropertyChanged(); }
        }

        public decimal FiyatUSD
        {
            get => _fiyatUSD;
            set { _fiyatUSD = value; OnPropertyChanged(); }
        }

        public decimal FiyatEUR
        {
            get => _fiyatEUR;
            set { _fiyatEUR = value; OnPropertyChanged(); }
        }

        public decimal Toplam => Adet * IndirimliFiyat;

        public event EventHandler OnBirimFiyatDegisti;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
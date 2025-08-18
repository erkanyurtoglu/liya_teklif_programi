using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Globalization;
using teklif_programi.Services;

namespace teklif_programi.Models
{
    public class TeklifUrunModel : INotifyPropertyChanged
    {
        private int _urunId;
        private string _urunKodu = string.Empty;
        private string _urunAciklamasi = string.Empty;
        private string _urunAciklamasiTr = string.Empty;
        private string _urunAciklamasiEn = string.Empty;
        private int _adet;
        private decimal _birimFiyat;
        private decimal _indirimliFiyat;
        private decimal _fiyatTL;
        private decimal _fiyatUSD;
        private decimal _fiyatEUR;
        private string _birimFiyatText = string.Empty;
        private string _indirimliFiyatText = string.Empty;
        private string _toplamText = string.Empty;
        private decimal _maliyetFiyati;
        private string _maliyetFiyatText = string.Empty;

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

        public string UrunAciklamasiTr
        {
            get => _urunAciklamasiTr;
            set { _urunAciklamasiTr = value; OnPropertyChanged(); }
        }

        public string UrunAciklamasiEn
        {
            get => _urunAciklamasiEn;
            set { _urunAciklamasiEn = value; OnPropertyChanged(); }
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

                    var raw = value
                        .Replace("₺", string.Empty)
                        .Replace("$", string.Empty)
                        .Replace("€", string.Empty)
                        .Replace(" ", string.Empty)
                        .Trim();

                    if (string.IsNullOrWhiteSpace(raw)) return;

                    var lastComma = raw.LastIndexOf(',');
                    var lastDot = raw.LastIndexOf('.');

                    if (lastComma > lastDot)
                    {
                        raw = raw.Replace(".", string.Empty);
                        raw = raw.Replace(",", ".");
                    }
                    else if (lastDot > lastComma)
                    {
                        raw = raw.Replace(",", string.Empty);
                    }
                    else
                    {
                        raw = raw.Replace(",", string.Empty).Replace(".", string.Empty);
                    }

                    if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedValue))
                    {
                        IndirimliFiyat = parsedValue;
                    }
                }
            }
        }

        public string ToplamText
        {
            get => _toplamText;
            set { _toplamText = value; OnPropertyChanged(); }
        }

        public decimal MaliyetFiyati
        {
            get => _maliyetFiyati;
            set { _maliyetFiyati = value; OnPropertyChanged(); }
        }

        public string MaliyetFiyatText
        {
            get => _maliyetFiyatText;
            set { _maliyetFiyatText = value; OnPropertyChanged(); }
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

        // Toplam tutar hesaplaması TeklifHesaplayici üzerinden yapılır
        public decimal Toplam => TeklifHesaplayici.HesaplaToplam(Adet, IndirimliFiyat);

        public event EventHandler OnBirimFiyatDegisti;
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
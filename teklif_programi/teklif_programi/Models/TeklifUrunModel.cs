using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace teklif_programi.Models
{
    // TeklifUrunModel: Teklifteki ürünlerin UI için veri modelini temsil eder, INotifyPropertyChanged ile veri bağlama desteği
    public class TeklifUrunModel : INotifyPropertyChanged
    {
        // UrunId: Ürünün benzersiz kimliği
        public int UrunId { get; set; }

        // UrunKodu: Ürünün kodu (varsayılan boş string)
        public string UrunKodu { get; set; } = string.Empty;

        // UrunAciklamasi: Ürünün açıklaması (varsayılan boş string)
        public string UrunAciklamasi { get; set; } = string.Empty;

        // _birimFiyat: Ürünün indirimsiz birim fiyatı için özel alan
        private decimal _birimFiyat;
        // BirimFiyat: Ürünün birim fiyatı, değiştiğinde OnPropertyChanged ve OnBirimFiyatDegisti tetiklenir
        public decimal BirimFiyat
        {
            get => _birimFiyat;
            set
            {
                if (_birimFiyat != value)
                {
                    _birimFiyat = value;
                    OnPropertyChanged(); // UI’yi güncellemek için olay tetiklenir
                    OnBirimFiyatDegisti?.Invoke(this, EventArgs.Empty); // Fiyat değişim olayı
                }
            }
        }

        // _adet: Ürün miktarı için özel alan (varsayılan 1)
        private int _adet = 1;
        // Adet: Ürün miktarı, değiştiğinde Toplam ve OnBirimFiyatDegisti tetiklenir
        public int Adet
        {
            get => _adet;
            set
            {
                if (_adet != value)
                {
                    _adet = value;
                    OnPropertyChanged(); // UI’yi güncellemek için olay tetiklenir
                    OnPropertyChanged(nameof(Toplam)); // Toplam alanını günceller
                    OnBirimFiyatDegisti?.Invoke(this, EventArgs.Empty); // Fiyat değişim olayı
                }
            }
        }

        // _indirimliFiyat: İndirimli fiyat için özel alan
        private decimal _indirimliFiyat;
        // IndirimliFiyat: İndirimli birim fiyat, değiştiğinde Toplam ve OnBirimFiyatDegisti tetiklenir
        public decimal IndirimliFiyat
        {
            get => _indirimliFiyat;
            set
            {
                if (_indirimliFiyat != value)
                {
                    _indirimliFiyat = value;
                    OnPropertyChanged(); // UI’yi güncellemek için olay tetiklenir
                    OnBirimFiyatDegisti?.Invoke(this, EventArgs.Empty); // Fiyat değişim olayı
                    OnPropertyChanged(nameof(Toplam)); // Toplam alanını günceller
                }
            }
        }

        // Toplam: Ürün toplam fiyatı (Adet * IndirimliFiyat), sadece get özelliği var
        public decimal Toplam => Adet * IndirimliFiyat;

        // FiyatTL: Fiyatın TL cinsinden değeri
        public decimal FiyatTL { get; set; }

        // FiyatUSD: Fiyatın USD cinsinden değeri
        public decimal FiyatUSD { get; set; }

        // FiyatEUR: Fiyatın EUR cinsinden değeri
        public decimal FiyatEUR { get; set; }

        // OnBirimFiyatDegisti: Fiyat veya adet değiştiğinde tetiklenen olay
        public event EventHandler? OnBirimFiyatDegisti;

        // PropertyChanged: UI veri bağlama için özellik değişim olayı
        public event PropertyChangedEventHandler? PropertyChanged;

        // OnPropertyChanged: Özellik değiştiğinde UI’yi güncellemek için kullanılan yöntem
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
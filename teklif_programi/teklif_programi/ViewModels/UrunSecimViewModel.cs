using teklif_programi.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using teklif_programi.Data;
using teklif_programi.Models;

namespace teklif_programi.ViewModels
{
    public class UrunSecimViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context;
        private readonly Window _window;
        private ObservableCollection<Urun> _urunler = new();
        private ObservableCollection<Urun> _filtrelenmisUrunler = new();
        private string _aramaMetni = string.Empty;
        private Urun? _secilenUrun;
        private int _yeniUrunAdet = 1;
        private decimal _yeniUrunIndirimliFiyat = 0;

        public UrunSecimViewModel(Window window)
        {
            _context = new TeklifDbContext();
            _window = window ?? throw new ArgumentNullException(nameof(window));
            LoadUrunler();
            UrunEkleOnayCommand = new RelayCommand(UrunEkleOnay, CanUrunEkleOnay);
            IptalCommand = new RelayCommand(Iptal);
        }

        private void LoadUrunler()
        {
            try
            {
                var urunList = _context.Urunler?.ToList();
                if (urunList != null && urunList.Any())
                {
                    _urunler = new ObservableCollection<Urun>(urunList);
                }
                else
                {
                    _urunler = new ObservableCollection<Urun>
                    {
                        new Urun { UrunId = 1, UrunKodu = "URUN001", UrunAciklamasi = "Test Ürün", BirimFiyat = 100.00m }
                    };
                }
                _filtrelenmisUrunler = new ObservableCollection<Urun>(_urunler);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürünler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                _urunler = new ObservableCollection<Urun>
                {
                    new Urun { UrunId = 1, UrunKodu = "URUN001", UrunAciklamasi = "Test Ürün", BirimFiyat = 100.00m }
                };
                _filtrelenmisUrunler = new ObservableCollection<Urun>(_urunler);
            }
        }

        public ObservableCollection<Urun> FiltrelenmisUrunler
        {
            get => _filtrelenmisUrunler;
            set { _filtrelenmisUrunler = value; OnPropertyChanged(); }
        }

        public string AramaMetni
        {
            get => _aramaMetni;
            set
            {
                _aramaMetni = value;
                OnPropertyChanged();
                FiltreleUrunler();
            }
        }

        public Urun? SecilenUrun
        {
            get => _secilenUrun;
            set
            {
                _secilenUrun = value;
                if (_secilenUrun != null)
                {
                    YeniUrunIndirimliFiyat = _secilenUrun.BirimFiyat; // Veritabanındaki birim fiyatı indirimli fiyata ata
                }
                OnPropertyChanged();
                UrunEkleOnayCommand.RaiseCanExecuteChanged();
            }
        }

        public int YeniUrunAdet
        {
            get => _yeniUrunAdet;
            set
            {
                _yeniUrunAdet = value > 0 ? value : 1;
                OnPropertyChanged();
                UrunEkleOnayCommand.RaiseCanExecuteChanged(); // Yazım hatası düzeltildi
            }
        }

        public decimal YeniUrunIndirimliFiyat
        {
            get => _yeniUrunIndirimliFiyat;
            set
            {
                _yeniUrunIndirimliFiyat = value >= 0 ? value : 0;
                OnPropertyChanged();
                UrunEkleOnayCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand UrunEkleOnayCommand { get; private set; }
        public RelayCommand IptalCommand { get; private set; }

        private void FiltreleUrunler()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(AramaMetni))
                {
                    FiltrelenmisUrunler = new ObservableCollection<Urun>(_urunler);
                }
                else
                {
                    var filtre = AramaMetni.ToLower();
                    FiltrelenmisUrunler = new ObservableCollection<Urun>(
                        (_urunler ?? new ObservableCollection<Urun>()).Where(u => u.UrunKodu.Contains(filtre, StringComparison.OrdinalIgnoreCase) || u.UrunAciklamasi.Contains(filtre, StringComparison.OrdinalIgnoreCase)));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürünler filtrelenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                FiltrelenmisUrunler = new ObservableCollection<Urun>(_urunler);
            }
        }

        private bool CanUrunEkleOnay()
        {
            return SecilenUrun != null && YeniUrunAdet > 0 && YeniUrunIndirimliFiyat >= 0;
        }

        private void UrunEkleOnay()
        {
            if (SecilenUrun != null)
            {
                _window.DialogResult = true;
                _window.Close();
            }
            else
            {
                MessageBox.Show("Lütfen bir ürün seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Iptal()
        {
            _window.DialogResult = false;
            _window.Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
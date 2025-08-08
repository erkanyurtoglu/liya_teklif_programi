// Gerekli isim alanları: MVVM, veritabanı, koleksiyonlar ve UI için
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using teklif_programi.Data;
using teklif_programi.Models;
using teklif_programi.view;

// ViewModel sınıflarının isim alanı
namespace teklif_programi.ViewModels
{
    // GecmisTekliflerViewModel: Geçmiş teklifleri yöneten ve UI ile bağlayan ViewModel
    public class GecmisTekliflerViewModel : INotifyPropertyChanged
    {
        // _context: Veritabanı bağlantısı için DbContext
        private readonly TeklifDbContext _context;
        // _teklifArama: Arama metni için özel alan
        private string _teklifArama = string.Empty;
        // _tumTeklifler: Tüm tekliflerin listesi
        private ObservableCollection<Teklif> _tumTeklifler = new();
        // _filtrelenmisTeklifler: Filtrelenmiş tekliflerin listesi
        private ObservableCollection<Teklif> _filtrelenmisTeklifler = new();

        // Kurucu: DbContext başlatılır, koleksiyonlar oluşturulur ve teklifler yüklenir
        public GecmisTekliflerViewModel()
        {
            _context = new TeklifDbContext();
            TumTeklifler = new();
            FiltrelenmisTeklifler = new();
            TeklifleriYukle(); // Teklifleri yükler
            DetayGosterCommand = new RelayCommand<Teklif>(DetayGoster); // Detay komutu bağlanır
        }

        // TumTeklifler: Tüm tekliflerin ObservableCollection’ı, UI ile bağlı
        public ObservableCollection<Teklif> TumTeklifler
        {
            get => _tumTeklifler;
            set { _tumTeklifler = value; OnPropertyChanged(); }
        }

        // FiltrelenmisTeklifler: Filtrelenmiş tekliflerin ObservableCollection’ı, UI ile bağlı
        public ObservableCollection<Teklif> FiltrelenmisTeklifler
        {
            get => _filtrelenmisTeklifler;
            set { _filtrelenmisTeklifler = value; OnPropertyChanged(); }
        }

        // TeklifArama: Arama metni, değiştiğinde filtreleme yapar
        public string TeklifArama
        {
            get => _teklifArama;
            set
            {
                if (_teklifArama != value)
                {
                    _teklifArama = value;
                    OnPropertyChanged();
                    TeklifleriFiltrele(); // Arama metni değiştiğinde filtreleme tetiklenir
                }
            }
        }

        // DetayGosterCommand: Teklif detayını gösteren komut
        public RelayCommand<Teklif> DetayGosterCommand { get; }

        // TeklifleriYukle: Veritabanından teklifleri yükler ve koleksiyonlara ekler
        private void TeklifleriYukle()
        {
            try
            {
                var teklifler = _context.Teklifler.Include(t => t.Musteri).ToList(); // Müşteri ile birlikte teklifleri çeker
                TumTeklifler.Clear();
                FiltrelenmisTeklifler.Clear();
                foreach (var teklif in teklifler)
                {
                    TumTeklifler.Add(teklif);
                    FiltrelenmisTeklifler.Add(teklif);
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya mesaj gösterir
                MessageBox.Show($"Teklifler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // TeklifleriFiltrele: Arama metnine göre teklifleri filtreler
        private void TeklifleriFiltrele()
        {
            if (string.IsNullOrWhiteSpace(TeklifArama))
            {
                FiltrelenmisTeklifler = new ObservableCollection<Teklif>(TumTeklifler); // Arama yoksa tüm teklifler gösterilir
            }
            else
            {
                var filtreli = TumTeklifler.Where(t =>
                    t.TeklifId.ToString().Contains(TeklifArama, StringComparison.OrdinalIgnoreCase) || // Teklif ID ile eşleşir
                    (t.Musteri?.FirmaAdi?.Contains(TeklifArama, StringComparison.OrdinalIgnoreCase) ?? false)).ToList(); // Firma adına göre eşleşir
                FiltrelenmisTeklifler = new ObservableCollection<Teklif>(filtreli);
            }
            OnPropertyChanged(nameof(FiltrelenmisTeklifler)); // UI’yi günceller
        }

        // DetayGoster: Seçilen teklifin detay penceresini açar
        private void DetayGoster(Teklif? teklif)
        {
            if (teklif == null)
            {
                MessageBox.Show("Lütfen bir teklif seçin!", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning); // Teklif seçilmemişse uyarı
                return;
            }

            try
            {
                var detayWindow = new TeklifDetayWindow
                {
                    DataContext = new TeklifDetayViewModel(teklif) // Detay penceresine ViewModel bağlanır
                };
                detayWindow.ShowDialog(); // Detay penceresini modal olarak açar
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya mesaj gösterir
                MessageBox.Show($"Detay penceresi açılırken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // PropertyChanged: UI veri bağlama için özellik değişim olayı
        public event PropertyChangedEventHandler? PropertyChanged;
        // OnPropertyChanged: Özellik değiştiğinde UI’yi günceller
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}
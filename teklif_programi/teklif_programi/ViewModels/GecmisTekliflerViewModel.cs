using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using teklif_programi.Data;
using teklif_programi.Models;
using teklif_programi.view;

namespace teklif_programi.ViewModels
{
    public class GecmisTekliflerViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context;
        private string _teklifArama = string.Empty;
        private ObservableCollection<Teklif> _tumTeklifler = new();
        private ObservableCollection<Teklif> _filtrelenmisTeklifler = new();
        private string _secilenTarihFiltresi;
        private DateTime? _baslangicTarihi;
        private DateTime? _bitisTarihi;

        public ObservableCollection<Teklif> TumTeklifler
        {
            get => _tumTeklifler;
            set { _tumTeklifler = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Teklif> FiltrelenmisTeklifler
        {
            get => _filtrelenmisTeklifler;
            set { _filtrelenmisTeklifler = value; OnPropertyChanged(); }
        }

        public string TeklifArama
        {
            get => _teklifArama;
            set
            {
                if (_teklifArama != value)
                {
                    _teklifArama = value;
                    OnPropertyChanged();
                    TeklifleriFiltrele();
                }
            }
        }

        public ObservableCollection<string> TarihFiltreSecenekleri { get; } = new() { "1 Gün", "1 Hafta", "15 Gün", "30 Gün", "Özel Tarih" };

        public string SecilenTarihFiltresi
        {
            get => _secilenTarihFiltresi;
            set
            {
                _secilenTarihFiltresi = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BaslangicTarihiVisibility));
                OnPropertyChanged(nameof(BitisTarihiVisibility));
                TeklifleriFiltrele();
            }
        }

        public DateTime? BaslangicTarihi
        {
            get => _baslangicTarihi;
            set
            {
                _baslangicTarihi = value;
                OnPropertyChanged();
                TeklifleriFiltrele();
            }
        }

        public DateTime? BitisTarihi
        {
            get => _bitisTarihi;
            set
            {
                _bitisTarihi = value;
                OnPropertyChanged();
                TeklifleriFiltrele();
            }
        }

        public Visibility BaslangicTarihiVisibility => SecilenTarihFiltresi == "Özel Tarih" ? Visibility.Visible : Visibility.Collapsed;
        public Visibility BitisTarihiVisibility => SecilenTarihFiltresi == "Özel Tarih" ? Visibility.Visible : Visibility.Collapsed;

        public RelayCommand<Teklif> DetayGosterCommand { get; }

        public GecmisTekliflerViewModel()
        {
            _context = new TeklifDbContext();
            _secilenTarihFiltresi = "1 Hafta";
            DetayGosterCommand = new RelayCommand<Teklif>(DetayGoster);
            TeklifleriYukle();
        }

        private void TeklifleriYukle()
        {
            try
            {
                var teklifler = _context.Teklifler
                    .Include(t => t.Musteri)
                    .Include(t => t.Personel)
                    .Include(t => t.TeklifToplam)
                    .ToList();
                TumTeklifler.Clear();
                FiltrelenmisTeklifler.Clear();
                foreach (var teklif in teklifler)
                {
                    TumTeklifler.Add(teklif);
                    FiltrelenmisTeklifler.Add(teklif);
                }
                TeklifleriFiltrele();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Teklifler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TeklifleriFiltrele()
        {
            var bugun = DateTime.Today;
            var filtreliTeklifler = TumTeklifler.ToList();

            // Tarih filtresi
            switch (SecilenTarihFiltresi)
            {
                case "1 Gün":
                    filtreliTeklifler = filtreliTeklifler.Where(t => t.OlusturmaTarihi >= bugun.AddDays(-1) && t.OlusturmaTarihi < bugun).ToList();
                    break;
                case "1 Hafta":
                    filtreliTeklifler = filtreliTeklifler.Where(t => t.OlusturmaTarihi >= bugun.AddDays(-7) && t.OlusturmaTarihi < bugun).ToList();
                    break;
                case "15 Gün":
                    filtreliTeklifler = filtreliTeklifler.Where(t => t.OlusturmaTarihi >= bugun.AddDays(-15) && t.OlusturmaTarihi < bugun).ToList();
                    break;
                case "30 Gün":
                    filtreliTeklifler = filtreliTeklifler.Where(t => t.OlusturmaTarihi >= bugun.AddDays(-30) && t.OlusturmaTarihi < bugun).ToList();
                    break;
                case "Özel Tarih":
                    if (BaslangicTarihi.HasValue && BitisTarihi.HasValue)
                    {
                        var baslangic = BaslangicTarihi.Value.Date;
                        var bitis = BitisTarihi.Value.Date.AddDays(1).AddTicks(-1); // Bitiş gününü dahil etmek için bir gün ekleyip son saniyeye ayarlar
                        filtreliTeklifler = filtreliTeklifler.Where(t => t.OlusturmaTarihi >= baslangic && t.OlusturmaTarihi <= bitis).ToList();
                    }
                    break;
            }

            // Arama metni filtresi
            if (!string.IsNullOrWhiteSpace(TeklifArama))
            {
                filtreliTeklifler = filtreliTeklifler.Where(t =>
                    t.TeklifId.ToString().Contains(TeklifArama, StringComparison.OrdinalIgnoreCase) ||
                    (t.Musteri != null && t.Musteri.FirmaAdi != null && t.Musteri.FirmaAdi.Contains(TeklifArama, StringComparison.OrdinalIgnoreCase)) ||
                    (t.Personel != null && t.Personel.AdSoyad != null && t.Personel.AdSoyad.Contains(TeklifArama, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            FiltrelenmisTeklifler = new ObservableCollection<Teklif>(filtreliTeklifler.OrderByDescending(t => t.OlusturmaTarihi));
        }

        private void DetayGoster(Teklif? teklif)
        {
            if (teklif == null)
            {
                MessageBox.Show("Lütfen bir teklif seçin!", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var detayWindow = new TeklifDetayWindow
                {
                    DataContext = new TeklifDetayViewModel(teklif)
                };
                detayWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Detay penceresi açılırken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}
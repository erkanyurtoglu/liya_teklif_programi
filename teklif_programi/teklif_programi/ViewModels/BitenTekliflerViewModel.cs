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
using teklif_programi.view;

namespace teklif_programi.ViewModels
{
    public class BitenTekliflerViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context;
        private ObservableCollection<Teklif> _tumTeklifler = new();
        private ObservableCollection<Teklif> _filtrelenmisTeklifler = new();
        private string _teklifArama = string.Empty;
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

        public ObservableCollection<string> TarihFiltreSecenekleri { get; } = new()
        {
            "Hepsi",
            "1 Gün",
            "1 Hafta",
            "15 Gün",
            "30 Gün",
            "Özel Tarih"
        };

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
        public RelayCommand<Teklif> SevkBilgileriCommand { get; }

        public BitenTekliflerViewModel()
        {
            _context = new TeklifDbContext();
            _secilenTarihFiltresi = "Hepsi";
            DetayGosterCommand = new RelayCommand<Teklif>(DetayGoster);
            SevkBilgileriCommand = new RelayCommand<Teklif>(SevkBilgileriniGoster);
            TeklifleriYukle();

            EventHub.TeklifGuncellendi += OnTeklifGuncellendi;
        }

        ~BitenTekliflerViewModel()
        {
            EventHub.TeklifGuncellendi -= OnTeklifGuncellendi;
        }

        private void TeklifleriYukle()
        {
            try
            {
                var teklifler = _context.Teklifler
                    .Include(t => t.Musteri)
                    .Include(t => t.Personel)
                    .Include(t => t.TeklifToplam)
                    .Include(t => t.SevkBilgileri)
                    .Include(t => t.TeklifUrunleri)
                        .ThenInclude(tu => tu.Urun)
                    .Where(t => t.Durum == "Tamamlandı")
                    .AsNoTracking()
                    .ToList();

                TumTeklifler.Clear();
                foreach (var teklif in teklifler)
                    TumTeklifler.Add(teklif);

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

            switch (SecilenTarihFiltresi)
            {
                case "Hepsi":
                    break;
                case "1 Gün":
                    filtreliTeklifler = filtreliTeklifler.Where(t => t.OlusturmaTarihi.Date >= bugun.AddDays(-1)).ToList();
                    break;
                case "1 Hafta":
                    filtreliTeklifler = filtreliTeklifler.Where(t => t.OlusturmaTarihi.Date >= bugun.AddDays(-7)).ToList();
                    break;
                case "15 Gün":
                    filtreliTeklifler = filtreliTeklifler.Where(t => t.OlusturmaTarihi.Date >= bugun.AddDays(-15)).ToList();
                    break;
                case "30 Gün":
                    filtreliTeklifler = filtreliTeklifler.Where(t => t.OlusturmaTarihi.Date >= bugun.AddDays(-30)).ToList();
                    break;
                case "Özel Tarih":
                    if (BaslangicTarihi.HasValue && BitisTarihi.HasValue)
                    {
                        var baslangic = BaslangicTarihi.Value.Date;
                        var bitis = BitisTarihi.Value.Date;
                        filtreliTeklifler = filtreliTeklifler.Where(t => t.OlusturmaTarihi.Date >= baslangic && t.OlusturmaTarihi.Date <= bitis).ToList();
                    }
                    break;
            }

            if (!string.IsNullOrWhiteSpace(TeklifArama))
            {
                filtreliTeklifler = filtreliTeklifler.Where(t =>
                    t.TeklifId.ToString().Contains(TeklifArama, StringComparison.OrdinalIgnoreCase) ||
                    (t.Musteri?.FirmaAdi?.Contains(TeklifArama, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.Personel?.AdSoyad?.Contains(TeklifArama, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (t.TeklifUrunleri != null && t.TeklifUrunleri.Any(tu =>
                        (tu.Urun?.UrunAciklamasi?.Contains(TeklifArama, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (tu.Urun?.UrunKodu?.Contains(TeklifArama, StringComparison.OrdinalIgnoreCase) ?? false)
                    ))
                ).ToList();
            }

            FiltrelenmisTeklifler.Clear();
            foreach (var teklif in filtreliTeklifler.OrderByDescending(t => t.OlusturmaTarihi))
                FiltrelenmisTeklifler.Add(teklif);
        }

        private void DetayGoster(Teklif? teklif)
        {
            if (teklif is null) return;
            var detay = new AlinanTeklifDetayWindow(teklif);
            detay.ShowDialog();
        }

        private void SevkBilgileriniGoster(Teklif? teklif)
        {
            if (teklif is null) return;

            try
            {
                var window = new SevkBilgileriWindow(teklif);
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Sevk bilgileri açılırken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnTeklifGuncellendi(int teklifId)
        {
            TeklifleriYukle();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

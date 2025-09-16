using teklif_programi.Helpers;
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
using teklif_programi.Services;

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
        public RelayCommand<Teklif> SilCommand { get; }

        public GecmisTekliflerViewModel()
        {
            _context = new TeklifDbContext();
            _secilenTarihFiltresi = "Hepsi";
            DetayGosterCommand = new RelayCommand<Teklif>(DetayGoster);
            SilCommand = new RelayCommand<Teklif>(TeklifiSil);
            TeklifleriYukle();

            EventHub.TeklifGuncellendi += OnTeklifGuncellendi;
        }

        ~GecmisTekliflerViewModel()
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
                    .Include(t => t.TeklifUrunleri)
                        .ThenInclude(tu => tu.Urun)
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

            // Tarih filtresi
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

            // Arama metni filtresi
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

            // Yeni koleksiyon atamak yerine temizle + doldur
            FiltrelenmisTeklifler.Clear();
            foreach (var teklif in filtreliTeklifler.OrderByDescending(t => t.OlusturmaTarihi))
                FiltrelenmisTeklifler.Add(teklif);
        }

        private void OnTeklifGuncellendi(int teklifId)
        {
            try
            {
                var updated = _context.Teklifler
                    .Include(t => t.Musteri)
                    .Include(t => t.Personel)
                    .Include(t => t.TeklifToplam)
                    .Include(t => t.TeklifUrunleri)
                        .ThenInclude(tu => tu.Urun)
                    .AsNoTracking()
                    .FirstOrDefault(t => t.TeklifId == teklifId);

                if (updated is null) return;

                var mevcut = TumTeklifler.FirstOrDefault(x => x.TeklifId == teklifId);
                if (mevcut != null)
                {
                    var index = TumTeklifler.IndexOf(mevcut);
                    TumTeklifler[index] = updated;
                }
                else
                {
                    TumTeklifler.Insert(0, updated);
                }

                TeklifleriFiltrele();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Teklif güncellenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                var detayWindow = new TeklifDetayWindow(teklif);

                var owner = Application.Current?.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.IsActive && w.IsVisible)
                    ?? Application.Current?.MainWindow;

                if (owner != null && owner.IsVisible)
                {
                    detayWindow.Owner = owner;
                    detayWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }
                else
                {
                    detayWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                detayWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Detay penceresi açılırken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TeklifiSil(Teklif? teklif)
        {
            if (teklif == null)
            {
                MessageBox.Show("Silinecek teklif bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var pwdDialog = new PasswordDialog
            {
                Owner = Application.Current?.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w.IsActive && w.IsVisible)
                    ?? Application.Current?.MainWindow
            };

            bool? result = pwdDialog.ShowDialog();
            if (result == true)
            {
                if (PasswordService.Verify(pwdDialog.EnteredPassword))
                {
                    if (MessageBox.Show("Bu teklifi kalıcı olarak silmek istediğinizden emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var silinecek = _context.Teklifler.FirstOrDefault(t => t.TeklifId == teklif.TeklifId);
                            if (silinecek != null)
                            {
                                _context.Teklifler.Remove(silinecek);
                                _context.SaveChanges();
                            }

                            TumTeklifler.Remove(teklif);
                            FiltrelenmisTeklifler.Remove(teklif);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Teklif silinirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Şifre yanlış. Silme işlemi iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}

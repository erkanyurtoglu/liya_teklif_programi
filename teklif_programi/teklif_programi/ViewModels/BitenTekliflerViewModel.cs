using teklif_programi.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private ObservableCollection<Teklif> _bitenTeklifler = new();

        public ObservableCollection<Teklif> BitenTeklifler
        {
            get => _bitenTeklifler;
            set { _bitenTeklifler = value; OnPropertyChanged(); }
        }

        public RelayCommand<Teklif> DetayGosterCommand { get; }

        public BitenTekliflerViewModel()
        {
            _context = new TeklifDbContext();
            DetayGosterCommand = new RelayCommand<Teklif>(DetayGoster);
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
                    .Include(t => t.TeklifUrunleri)
                        .ThenInclude(tu => tu.Urun)
                    .Where(t => t.Durum == "Tamamlandı")
                    .AsNoTracking()
                    .ToList();

                BitenTeklifler.Clear();
                foreach (var teklif in teklifler)
                    BitenTeklifler.Add(teklif);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Teklifler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DetayGoster(Teklif? teklif)
        {
            if (teklif is null) return;
            var detay = new AlinanTeklifDetayWindow(teklif);
            detay.ShowDialog();
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
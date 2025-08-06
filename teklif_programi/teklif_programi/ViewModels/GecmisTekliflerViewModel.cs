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

namespace teklif_programi.ViewModels
{
    public class GecmisTekliflerViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context;
        private string _teklifArama = string.Empty;
        private ObservableCollection<Teklif> _tumTeklifler;
        private ObservableCollection<Teklif> _filtrelenmisTeklifler;

        public GecmisTekliflerViewModel()
        {
            _context = new TeklifDbContext();
            TumTeklifler = new ObservableCollection<Teklif>();
            FiltrelenmisTeklifler = new ObservableCollection<Teklif>();
            TeklifleriYukle();
            DetayGosterCommand = new RelayCommand<Teklif>(DetayGoster);
        }

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

        public RelayCommand<Teklif> DetayGosterCommand { get; }

        private void TeklifleriYukle()
        {
            try
            {
                var teklifler = _context.Teklifler.Include(t => t.Musteri).ToList();
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
                MessageBox.Show($"Teklifler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TeklifleriFiltrele()
        {
            if (string.IsNullOrWhiteSpace(TeklifArama))
            {
                FiltrelenmisTeklifler = new ObservableCollection<Teklif>(TumTeklifler);
            }
            else
            {
                var filtreli = TumTeklifler.Where(t =>
                    t.TeklifId.ToString().Contains(TeklifArama, StringComparison.OrdinalIgnoreCase) ||
                    (t.Musteri?.FirmaAdi?.Contains(TeklifArama, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
                FiltrelenmisTeklifler = new ObservableCollection<Teklif>(filtreli);
            }
            OnPropertyChanged(nameof(FiltrelenmisTeklifler));
        }

        private void DetayGoster(Teklif teklif)
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
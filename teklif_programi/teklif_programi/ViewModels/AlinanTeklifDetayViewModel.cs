using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using teklif_programi.Data;
using teklif_programi.Helpers;
using teklif_programi.Models;

namespace teklif_programi.ViewModels
{
    public class AlinanTeklifDetayViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context;
        public Teklif Teklif { get; }
        public ObservableCollection<TeklifUrun> Urunler { get; } = new();

        public RelayCommand KaydetCommand { get; }

        public AlinanTeklifDetayViewModel(Teklif teklif)
        {
            _context = new TeklifDbContext();
            Teklif = _context.Teklifler
                .Include(t => t.TeklifUrunleri)
                    .ThenInclude(tu => tu.Urun)
                .First(t => t.TeklifId == teklif.TeklifId);

            foreach (var u in Teklif.TeklifUrunleri)
            {
                Urunler.Add(u);
            }

            KaydetCommand = new RelayCommand(Kaydet);
        }

        private void Kaydet()
        {
            _context.SaveChanges();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
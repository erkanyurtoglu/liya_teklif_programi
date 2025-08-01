using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using teklif_programi.Data;
using teklif_programi.Models;

namespace teklif_programi.view
{
    public partial class GecmisTekliflerim : UserControl
    {
        private TeklifDbContext _db = new TeklifDbContext();
        private ObservableCollection<Teklif> TekliflerListesi = new ObservableCollection<Teklif>();

        public GecmisTekliflerim()
        {
            InitializeComponent();
            TeklifListele();
        }

        private void TeklifListele(string arama = "")
        {
            var tekliflerQuery = _db.Teklifler
                .Include(t => t.Firma)
                .Include(t => t.Personel)
                .Include(t => t.TeklifDetaylari)
                .ThenInclude(td => td.Urun)
                .Where(t => string.IsNullOrEmpty(arama)
                    || t.TeklifNoID.ToString().Contains(arama)
                    || (t.Firma != null && t.Firma.FirmaAdi.Contains(arama))
                    || (t.Personel != null && t.Personel.AdSoyad.Contains(arama)))
                .OrderByDescending(t => t.TeklifTarihi)
                .ToList();

            TekliflerListesi.Clear();
            foreach (var teklif in tekliflerQuery)
            {
                TekliflerListesi.Add(teklif);
            }

            dataGridTeklifler.ItemsSource = TekliflerListesi;
        }

        private void BtnDetay_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenTeklif = button?.DataContext as Teklif;
            if (secilenTeklif != null)
            {
                var detayPencere = new TeklifDetayWindow(secilenTeklif);
                if (detayPencere.ShowDialog() == true)
                {

                }
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenTeklif = button?.DataContext as Teklif;
            if (secilenTeklif != null)
            {
                if (MessageBox.Show("Teklifi silmek istediğinize emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _db.Teklifler.Remove(secilenTeklif);
                    _db.SaveChanges();
                    TeklifListele();
                }
            }
        }
    }
}

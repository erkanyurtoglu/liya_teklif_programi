using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using teklif_programi.Data;
using teklif_programi.Models;

namespace teklif_programi.view
{
    /// <summary>
    /// Interaction logic for GecmisTekliflerim.xaml
    /// </summary>
    public partial class GecmisTekliflerim : UserControl
    {
        private readonly TeklifDbContext _db = new();
        private readonly ObservableCollection<Teklif> _tekliflerListesi = new();

        public GecmisTekliflerim()
        {
            InitializeComponent();
            TeklifListele();
        }

        private void TeklifListele(string arama = "")
        {
            var tekliflerQuery = _db.Teklifler
                .Include(t => t.Musteri)
                .Include(t => t.Personel)
                .Include(t => t.TeklifToplam)
                .Where(t =>
                    string.IsNullOrWhiteSpace(arama)
                    || t.TeklifId.ToString().Contains(arama)
                    || (t.Musteri != null && t.Musteri.FirmaAdi.Contains(arama, StringComparison.OrdinalIgnoreCase))
                    || (t.Personel != null && t.Personel.AdSoyad.Contains(arama, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(t => t.OlusturmaTarihi)
                .ToList();

            _tekliflerListesi.Clear();
            foreach (var teklif in tekliflerQuery)
            {
                _tekliflerListesi.Add(teklif);
            }

            dataGridTeklifler.ItemsSource = _tekliflerListesi;
        }

        private void BtnAra_Click(object sender, RoutedEventArgs e)
        {
            string arama = txtArama.Text.Trim();
            TeklifListele(arama);
        }

        private void BtnDetay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: Teklif secilenTeklif })
            {
                MessageBox.Show(
                    $"Teklif No: {secilenTeklif.TeklifId}\nFirma: {secilenTeklif.Musteri?.FirmaAdi}\nPersonel: {secilenTeklif.Personel?.AdSoyad}",
                    "Teklif Detayı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: Teklif secilenTeklif })
            {
                var result = MessageBox.Show("Teklifi silmek istediğinize emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _db.Teklifler.Remove(secilenTeklif);
                    _db.SaveChanges();
                    TeklifListele();
                }
            }
        }
    }
}

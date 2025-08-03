using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using teklif_programi.Data;
using teklif_programi.Models;
using Microsoft.EntityFrameworkCore;

namespace teklif_programi.view
{


    /// <summary>
    /// Interaction logic for GecmisTekliflerim.xaml
    /// </summary>
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
                .Include(t => t.Musteri)
                .Include(t => t.Personel)
                .Where(t => string.IsNullOrEmpty(arama)
                    || t.teklif_id.ToString().Contains(arama)
                    || (t.Musteri != null && t.Musteri.firma_adi.Contains(arama))
                    || (t.Personel != null && t.Personel.ad_soyad.Contains(arama)))
                .OrderByDescending(t => t.olusturma_tarihi)
                .ToList();

            TekliflerListesi.Clear();
            foreach (var teklif in tekliflerQuery)
            {
                TekliflerListesi.Add(teklif);
            }

            dataGridTeklifler.ItemsSource = TekliflerListesi;
        }

        private void BtnAra_Click(object sender, RoutedEventArgs e)
        {
            string arama = txtArama.Text.Trim();
            TeklifListele(arama);
        }

        private void BtnDetay_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenTeklif = button?.DataContext as Teklif;
            if (secilenTeklif != null)
            {
                MessageBox.Show($"Teklif No: {secilenTeklif.teklif_id}\nFirma: {secilenTeklif.Musteri.firma_adi}\nPersonel: {secilenTeklif.Personel.ad_soyad}");
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

// Gerekli isim alanları: Entity Framework, WPF, koleksiyonlar ve uygulama modelleri için
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using teklif_programi.Data;
using teklif_programi.Models;
using teklif_programi.ViewModels;

// WPF kullanıcı kontrollerinin isim alanı
namespace teklif_programi.view
{
    /// <summary>
    /// GecmisTekliflerim.xaml: Geçmiş teklifleri listeleyen ve yöneten kullanıcı kontrolü
    /// </summary>
    public partial class GecmisTekliflerim : UserControl
    {
        // _db: Veritabanı bağlantısı için DbContext
        private readonly TeklifDbContext _db = new();
        // _tekliflerListesi: Tekliflerin ObservableCollection’ı, UI ile bağlı
        private readonly ObservableCollection<Teklif> _tekliflerListesi = new();

        // Kurucu: Kontrolü başlatır, teklifleri listeler ve DataContext’i ViewModel’e bağlar
        public GecmisTekliflerim()
        {
            InitializeComponent(); // WPF kontrolünü başlatır
            TeklifListele(); // Teklifleri yükler
            DataContext = new GecmisTekliflerViewModel(); // ViewModel bağlanır
        }

        // TeklifListele: Veritabanından teklifleri çeker, filtreler ve DataGrid’e bağlar
        private void TeklifListele(string arama = "")
        {
            var tekliflerQuery = _db.Teklifler
                .Include(t => t.Musteri) // Müşteri bilgilerini çeker
                .Include(t => t.Personel) // Personel bilgilerini çeker
                .Include(t => t.TeklifToplam) // Toplam bilgilerini çeker
                .Where(t =>
                    string.IsNullOrWhiteSpace(arama) // Arama yoksa tüm teklifler
                    || t.TeklifId.ToString().Contains(arama) // Teklif ID ile filtre
                    || (t.Musteri != null && t.Musteri.FirmaAdi.Contains(arama, StringComparison.OrdinalIgnoreCase)) // Firma adına göre filtre
                    || (t.Personel != null && t.Personel.AdSoyad.Contains(arama, StringComparison.OrdinalIgnoreCase))) // Personel adına göre filtre
                .OrderByDescending(t => t.OlusturmaTarihi) // Tarihe göre sıralar
                .ToList();
            _tekliflerListesi.Clear();
            foreach (var teklif in tekliflerQuery)
            {
                _tekliflerListesi.Add(teklif); // Teklifleri koleksiyona ekler
            }
            dataGridTeklifler.ItemsSource = _tekliflerListesi; // DataGrid’e bağlar
        }

        // BtnAra_Click: Arama butonuna tıklandığında teklifleri filtreler
        private void BtnAra_Click(object sender, RoutedEventArgs e)
        {
            string arama = txtArama.Text.Trim(); // Arama metnini alır
            TeklifListele(arama); // Filtrelenmiş teklifleri listeler
        }

        // BtnDetay_Click: Seçilen teklifin detaylarını mesaj kutusunda gösterir
        private void BtnDetay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: Teklif secilenTeklif })
            {
                MessageBox.Show(
                    $"Teklif No: {secilenTeklif.TeklifId}\nFirma: {secilenTeklif.Musteri?.FirmaAdi}\nPersonel: {secilenTeklif.Personel?.AdSoyad}",
                    "Teklif Detayı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // BtnSil_Click: Seçilen teklifi siler, onay ister
        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: Teklif secilenTeklif })
            {
                var result = MessageBox.Show("Teklifi silmek istediğinize emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _db.Teklifler.Remove(secilenTeklif); // Teklifi veritabanından siler
                    _db.SaveChanges(); // Değişiklikleri kaydeder
                    TeklifListele(); // Listeyi günceller
                }
            }
        }
    }
}
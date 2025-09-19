using System;
using System.Collections.Generic;
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
using teklif_programi.Data;    // Veritabanı bağlantısı için
using teklif_programi.Models; // Urun model sınıfı

namespace teklif_programi.view
{
    /// <summary>
    /// UrunEkle.xaml kullanıcı kontrolü.
    /// Yeni ürün ekleme işlemlerini yapar.
    /// </summary>
    public partial class UrunEkle : UserControl
    {
        // Veritabanı erişimi için DbContext
        public TeklifDbContext _db = new TeklifDbContext();

        // Constructor
        public UrunEkle()
        {
            InitializeComponent(); // XAML bileşenlerini yükler
        }

        /// <summary>
        /// "Kaydet" butonuna basıldığında çalışır.
        /// Formdaki bilgilerle yeni bir ürün nesnesi oluşturur
        /// ve veritabanına ekler.
        /// </summary>
        private void Kaydet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Yeni ürün nesnesi oluştur
                Urun yeniUrun = new Urun()
                {
                    UrunKodu = txtUrunKodu.Text.Trim(),               // Ürün kodu
                    Kategori = txtUrunKategori.Text.Trim(),           // Kategori
                    UrunAciklamasi = txtUrunAciklama.Text.Trim(),     // Açıklama
                    EklenmeTarihi = DateTime.Now,                     // Eklenme tarihi
                    BirimFiyat = decimal.Parse(txtBirimSatisFiyati.Text.Trim()), // TL satış fiyatı
                    MaliyetFiyati = decimal.Parse(txtYurticiMaliyet.Text.Trim()), // Maliyet
                    FiyatTL = decimal.Parse(txtBirimSatisFiyati.Text.Trim()),     // TL fiyat
                    FiyatUSD = decimal.Parse(txtDolarBirimFiyati.Text.Trim()),    // USD fiyat
                    FiyatEUR = decimal.Parse(txtEuroBirimFiyati.Text.Trim()),     // EUR fiyat
                };

                // Veritabanına ekle
                _db.Urunler.Add(yeniUrun);
                _db.SaveChanges();

                // Başarılı mesaj
                MessageBox.Show("Ürün başarıyla kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Hata olursa kullanıcıya göster
                MessageBox.Show("Hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// "İptal" butonuna basıldığında çalışır.
        /// Tüm TextBox alanlarını temizler.
        /// </summary>
        private void Iptal_Click(object sender, RoutedEventArgs e)
        {
            txtUrunKodu.Text = "";
            txtUrunKategori.Text = "";
            txtUrunAciklama.Text = "";
            txtBirimSatisFiyati.Text = "";
            txtYurticiMaliyet.Text = "";
            txtDolarBirimFiyati.Text = "";
            txtEuroBirimFiyati.Text = "";
        }
    }
}

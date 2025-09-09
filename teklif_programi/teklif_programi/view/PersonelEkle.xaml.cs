// Gerekli isim alanları: WPF, veritabanı ve temel sistem fonksiyonları için
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
using teklif_programi.Data;
using teklif_programi.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

// WPF kullanıcı kontrollerinin isim alanı
namespace teklif_programi.view
{
    /// <summary>
    /// PersonelEkle.xaml: Yeni personel eklemek için kullanılan WPF kullanıcı kontrolü
    /// </summary>
    public partial class PersonelEkle : UserControl
    {
        // _db: Veritabanı bağlantısı için DbContext
        public TeklifDbContext _db = new TeklifDbContext();

        // Kurucu: Kontrolü başlatır
        public PersonelEkle()
        {
            InitializeComponent(); // WPF kontrolünü başlatır
        }

        // Kaydet_Click: Kaydet butonuna tıklandığında yeni personeli veritabanına ekler
        private void Kaydet_Click(object sender, RoutedEventArgs e)
        {
            // Tüm alanların dolu olup olmadığını kontrol eder
            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text) ||
                string.IsNullOrWhiteSpace(txtPozisyon.Text) ||
                string.IsNullOrWhiteSpace(txtKullaniciAdi.Text) ||
                string.IsNullOrWhiteSpace(txtTelefon.Text) ||
                string.IsNullOrWhiteSpace(txtSifre.Text))
            {
                MessageBox.Show("Lütfen tüm alanları doldurunuz.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Yeni personel nesnesi oluşturur
            var personel = new Personel
            {
                AdSoyad = txtAdSoyad.Text,
                Pozisyon = txtPozisyon.Text,
                KullaniciAdi = txtKullaniciAdi.Text,
                Telefon = txtTelefon.Text,
                Sifre = txtSifre.Text,
            };

            _db.Personeller.Add(personel); // Personeli veritabanına ekler
            _db.SaveChanges(); // Değişiklikleri kaydeder

            MessageBox.Show("Personel başarıyla eklendi.", "Kayıt Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

            // TextBox'ları temizler
            txtAdSoyad.Clear();
            txtPozisyon.Clear();
            txtKullaniciAdi.Clear();
            txtTelefon.Clear();
            txtSifre.Clear();
        }
    }
}
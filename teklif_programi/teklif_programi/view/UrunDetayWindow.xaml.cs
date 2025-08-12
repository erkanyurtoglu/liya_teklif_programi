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
using System.Windows.Shapes;
using teklif_programi.Data;   // Veritabanı erişimi için
using teklif_programi.Models; // Urun model sınıfı
using teklif_programi.Services; // Parola doğrulama servisi

namespace teklif_programi.view
{
    /// <summary>
    /// UrunDetayWindow.xaml için etkileşim mantığı.
    /// Bu pencere, seçilen bir ürünün detaylarını görüntüleme ve güncelleme imkanı sağlar.
    /// </summary>
    public partial class UrunDetayWindow : Window
    {
        // Pencerede düzenlenecek ürün nesnesi
        private Urun _urun;

        // Veritabanı işlemleri için DbContext
        private readonly TeklifDbContext _db = new TeklifDbContext();

        /// <summary>
        /// Pencere oluşturulurken düzenlenecek ürün parametre olarak alınır
        /// ve form alanları ürün bilgileriyle doldurulur.
        /// </summary>
        public UrunDetayWindow(Urun urun)
        {
            InitializeComponent();
            _urun = urun;

            // Ürün bilgilerini TextBox’lara doldur
            txtUrunKodu.Text = _urun.UrunKodu;
            txtKategori.Text = _urun.Kategori;
            txtAciklama.Text = _urun.UrunAciklamasi;
            txt2025BirimSatisFiyati.Text = _urun.BirimFiyat.ToString("F2"); // 2 ondalık format
            txtDolarBirimSatisFiyati.Text = _urun.FiyatUSD.ToString("F2");
            txtEuroBirimSatisFiyati.Text = _urun.FiyatEUR.ToString("F2");
            txtYurticiMaliyetBirimFiyati.Text = _urun.MaliyetFiyati.ToString("F2");
        }

        /// <summary>
        /// Kaydet butonuna tıklandığında çalışır.
        /// Kullanıcıdan şifre ister, doğruysa ürünü günceller.
        /// </summary>
        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            var pwdWindow = new PasswordDialog { Owner = this };

            // Şifre penceresi onaylandı ve parola doğruysa işlem yapılır
            if (pwdWindow.ShowDialog() == true && PasswordService.Verify(pwdWindow.EnteredPassword))
            {
                // TextBox’lardaki değerler ürüne aktarılır
                _urun.Kategori = txtKategori.Text;
                _urun.UrunAciklamasi = txtAciklama.Text;
                _urun.BirimFiyat = decimal.Parse(txt2025BirimSatisFiyati.Text);
                _urun.MaliyetFiyati = decimal.Parse(txtYurticiMaliyetBirimFiyati.Text);

                // Veritabanında güncelleme yapılır
                _db.Urunler.Update(_urun);
                _db.SaveChanges();

                // Bilgilendirme mesajı
                MessageBox.Show("Ürün başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close(); // Pencere kapanır
            }
            else
            {
                // Şifre yanlış veya kullanıcı iptal etti
                MessageBox.Show("Şifre hatalı veya işlem iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// İptal butonuna tıklandığında pencereyi kapatır.
        /// </summary>
        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

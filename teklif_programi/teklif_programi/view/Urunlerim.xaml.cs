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
using teklif_programi.Data;    // Veritabanı bağlantı sınıfı
using teklif_programi.Models; // Urun model sınıfı
using teklif_programi.Services; // Parola doğrulama servisis

namespace teklif_programi.view
{
    /// <summary>
    /// Urunlerim.xaml kullanıcı kontrolü.
    /// Ürün listeleme, arama, detay görüntüleme ve silme işlemlerini yapar.
    /// </summary>
    public partial class Urunlerim : UserControl
    {
        // Veritabanı bağlantısı için DbContext
        public TeklifDbContext _db = new TeklifDbContext();

        // Constructor
        public Urunlerim()
        {
            InitializeComponent(); // XAML bileşenlerini yükler
            UrunListele(); // İlk açılışta tüm ürünleri listele
        }

        /// <summary>
        /// Ürün listesini veritabanından çeker.
        /// Arama parametresi verilirse filtre uygular.
        /// </summary>
        private void UrunListele(string arama = "")
        {
            var urunler = string.IsNullOrWhiteSpace(arama)
                ? _db.Urunler.ToList() // Arama yoksa tüm ürünler
                : _db.Urunler
                      .Where(f => f.UrunAciklamasi.Contains(arama) || f.UrunKodu.Contains(arama) || f.Kategori.Contains(arama)) // Açıklama veya kodda arama kelimesi geçenler
                      .ToList();

            dataGridUrunler.ItemsSource = urunler; // DataGrid'e verileri bağla
        }

        /// <summary>
        /// Arama kutusuna yazıldığında listeyi filtreler.
        /// </summary>
        private void txtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            UrunListele(txtArama.Text.Trim());
        }

        /// <summary>
        /// "Maliyet" butonuna basıldığında ilgili ürünün maliyet penceresini açar.
        /// </summary>
        private void BtnMaliyet_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenUrun = button?.DataContext as Urun;

            if (secilenUrun != null)
            {
                var maliyetPencere = new UrunMaliyetWindow(secilenUrun);
                maliyetPencere.Owner = Window.GetWindow(this);
                bool? sonuc = maliyetPencere.ShowDialog();
                if (sonuc == true)
                {
                    UrunListele(txtArama.Text.Trim());
                }
            }
        }


        /// <summary>
        /// "Detay" butonuna basıldığında seçilen ürünün detay penceresini açar.
        /// </summary>
        private void BtnDetay_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenUrun = button?.DataContext as Urun; // Tıklanan satırdaki ürün

            if (secilenUrun != null)
            {
                var detayPencere = new UrunDetayWindow(secilenUrun); // Detay penceresini aç
                detayPencere.ShowDialog();
            }

            // Olası değişiklikleri listeye yansıt
            UrunListele(txtArama.Text.Trim());
        }

        /// <summary>
        /// "Sil" butonuna basıldığında ürünü siler.
        /// Önce şifre doğrulaması yapılır.
        /// </summary>
        private void BtnUrunSil_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenUrun = button?.DataContext as Urun; // Seçilen ürün

            if (secilenUrun == null)
            {
                MessageBox.Show("Silinecek ürün bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Şifre doğrulama penceresini aç
            var pwdDialog = new PasswordDialog();
            pwdDialog.Owner = Window.GetWindow(this);

            bool? result = pwdDialog.ShowDialog(); // Pencere sonucu

            if (result == true)
            {
                if (PasswordService.Verify(pwdDialog.EnteredPassword))
                {
                    // Silme işlemini onaylat
                    if (MessageBox.Show("Bu ürün kalıcı olarak silinecek. Emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        using (var db = new TeklifDbContext())
                        {
                            // Ürünü veritabanında bul
                            var urun = db.Urunler.FirstOrDefault(u => u.UrunId == secilenUrun.UrunId);

                            if (urun != null)
                            {
                                db.Urunler.Remove(urun); // Sil
                                db.SaveChanges(); // Kaydet
                                MessageBox.Show("Ürün başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }

                        // Listeyi güncelle
                        UrunListele(txtArama.Text.Trim());
                    }
                }
                else
                {
                    // Şifre yanlışsa iptal et
                    MessageBox.Show("Şifre yanlış. Silme işlemi iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

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
using teklif_programi.Data;     // Veritabanı erişimi için proje içindeki Data katmanı
using teklif_programi.Models;  // Personel gibi model sınıfları

namespace teklif_programi.view
{
    /// <summary>
    /// Personellerim.xaml için etkileşim mantığını içeren sınıf.
    /// Bu sayfa, personellerin listelenmesini, aranmasını, detaylarının görüntülenmesini
    /// ve silinmesini sağlar.
    /// </summary>
    public partial class Personellerim : UserControl
    {
        // Veritabanına bağlanmak için DbContext nesnesi
        public TeklifDbContext _db = new TeklifDbContext();

        // Constructor - Sayfa yüklendiğinde çalışır
        public Personellerim()
        {
            InitializeComponent(); // XAML tarafındaki bileşenleri başlatır
            PersonelListele();     // Sayfa açıldığında tüm personelleri listeler
        }

        /// <summary>
        /// Personelleri listeler.
        /// Eğer "arama" parametresi boşsa tüm personelleri getirir,
        /// değilse ad-soyad veya telefon numarasına göre filtreler.
        /// </summary>
        private void PersonelListele(string arama = "")
        {
            using (var db = new TeklifDbContext()) // Her sorguda yeni bir DbContext kullanılır
            {
                var personeller = string.IsNullOrWhiteSpace(arama)
                    ? db.Personeller.ToList() // Arama yoksa tüm personeller
                    : db.Personeller
                          .Where(f => f.AdSoyad.Contains(arama) || f.Telefon.Contains(arama)) // Filtreleme
                          .ToList();

                // DataGrid'e veriyi bağla
                dataGridPersonel.ItemsSource = personeller;
            }
        }

        /// <summary>
        /// Arama kutusundaki metin değiştiğinde çalışır.
        /// Girilen değere göre listeyi otomatik filtreler.
        /// </summary>
        private void txtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            PersonelListele(txtArama.Text.Trim());
        }

        /// <summary>
        /// Detay butonuna tıklandığında çalışır.
        /// Seçilen personelin detay penceresini açar.
        /// </summary>
        private void BtnDetay_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button; // Tıklanan buton
            var secilenPersonel = button?.DataContext as Personel; // O satıra bağlı personel

            if (secilenPersonel != null)
            {
                // Personel detay penceresini aç
                var detayPencere = new PersonelDetayWindow(secilenPersonel);
                bool? result = detayPencere.ShowDialog();

                if (result == true)
                {
                    // Eğer detay penceresinde güncelleme yapıldıysa listeyi yenile
                    PersonelListele(txtArama.Text.Trim());
                }
            }
        }

        /// <summary>
        /// Personel silme butonuna tıklandığında çalışır.
        /// Silme işlemi için şifre doğrulaması ve kullanıcı onayı ister.
        /// </summary>
        private void BtnPersonelSil_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenPersonel = button?.DataContext as Personel;

            if (secilenPersonel == null)
            {
                // Seçili personel yoksa hata mesajı göster
                MessageBox.Show("Silinecek personel bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Şifre girişi için özel bir dialog aç
            var pwdDialog = new PasswordDialog();
            pwdDialog.Owner = Window.GetWindow(this); // Mevcut pencereyi ana pencere olarak ayarla

            bool? result = pwdDialog.ShowDialog();

            if (result == true)
            {
                const string dogruSifre = "Liya2015"; // Silme için gerekli doğru şifre

                if (pwdDialog.EnteredPassword == dogruSifre)
                {
                    // Kullanıcıdan son onay alınır
                    if (MessageBox.Show("Bu personel kalıcı olarak silinecek. Emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        using (var db = new TeklifDbContext())
                        {
                            // Silinecek personeli veritabanında bul
                            var silinecek = db.Personeller.FirstOrDefault(p => p.PersonelId == secilenPersonel.PersonelId);

                            if (silinecek != null)
                            {
                                // Kayıt silinir ve değişiklikler kaydedilir
                                db.Personeller.Remove(silinecek);
                                db.SaveChanges();
                                MessageBox.Show("Personel başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }

                        // Silme sonrası liste yenilenir
                        PersonelListele(txtArama.Text.Trim());
                    }
                }
                else
                {
                    // Şifre yanlışsa hata mesajı
                    MessageBox.Show("Şifre yanlış. Silme işlemi iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

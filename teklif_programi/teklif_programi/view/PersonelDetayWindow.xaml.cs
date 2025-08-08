// Gerekli isim alanları: LINQ, WPF ve uygulama veri modelleri için
using System.Linq;
using System.Windows;
using teklif_programi.Data;
using teklif_programi.Models;

// WPF pencerelerinin isim alanı
namespace teklif_programi.view
{
    /// <summary>
    /// PersonelDetayWindow.xaml: Personel bilgilerini görüntüleyen ve güncelleyen WPF penceresi
    /// </summary>
    public partial class PersonelDetayWindow : Window
    {
        // _db: Veritabanı bağlantısı için DbContext
        private readonly TeklifDbContext _db = new();
        // _personel: Güncellenecek veya görüntülenecek personel nesnesi
        private readonly Personel _personel;

        // Kurucu: Seçilen personeli veritabanından çeker ve verileri doldurur
        public PersonelDetayWindow(Personel secilenPersonel)
        {
            InitializeComponent(); // WPF penceresini başlatır
            // Veritabanından personel bilgilerini çeker, bulunamazsa hata fırlatır
            _personel = _db.Personeller.FirstOrDefault(p => p.PersonelId == secilenPersonel.PersonelId)
                        ?? throw new InvalidOperationException("Personel verisi bulunamadı!");
            VeriDoldur(); // TextBox'lara verileri yükler
        }

        // VeriDoldur: Personel bilgilerini TextBox'lara aktarır
        private void VeriDoldur()
        {
            txtPersonelKodu.Text = _personel.PersonelId.ToString(); // Personel ID
            txtAdSoyad.Text = _personel.AdSoyad; // Ad soyad
            txtPozisyon.Text = _personel.Pozisyon; // Pozisyon
            txtTelefon.Text = _personel.Telefon; // Telefon
            txtSifre.Text = _personel.Sifre ?? string.Empty; // Şifre (boşsa varsayılan boş string)
        }

        // BtnIptal_Click: İptal butonuna tıklandığında pencereyi kapatır
        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // İşlem iptal edildi
            Close(); // Pencereyi kapatır
        }

        // BtnPersonelKaydet_Click: Kaydet butonuna tıklandığında personel bilgilerini günceller
        private void BtnPersonelKaydet_Click(object sender, RoutedEventArgs e)
        {
            var pwdDialog = new PasswordDialog { Owner = this }; // Şifre giriş penceresini modal olarak açar
            if (pwdDialog.ShowDialog() is true)
            {
                const string dogruSifre = "Liya2015"; // Sabit şifre
                if (pwdDialog.EnteredPassword == dogruSifre)
                {
                    // TextBox'lardan personel bilgilerini günceller
                    _personel.AdSoyad = txtAdSoyad.Text;
                    _personel.Pozisyon = txtPozisyon.Text;
                    _personel.Telefon = txtTelefon.Text;
                    if (!string.IsNullOrWhiteSpace(txtSifre.Text))
                        _personel.Sifre = txtSifre.Text; // Şifre boş değilse günceller
                    _db.Personeller.Update(_personel); // Veritabanında personeli günceller
                    _db.SaveChanges(); // Değişiklikleri kaydeder
                    MessageBox.Show("Personel bilgileri başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true; // İşlem başarılı
                    Close(); // Pencereyi kapatır
                }
                else
                {
                    MessageBox.Show("Şifre yanlış. Güncelleme iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
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
using System.Windows.Shapes;
using teklif_programi.Data;
using teklif_programi.Models;
using teklif_programi.Services;

// WPF pencerelerinin isim alanı
namespace teklif_programi.view
{
    // FirmaDetayWindow: Firma bilgilerini görüntüleyen ve güncelleyen WPF penceresi
    public partial class FirmaDetayWindow : Window
    {
        // _db: Veritabanı bağlantısı için DbContext
        private readonly TeklifDbContext _db = new TeklifDbContext();
        // _firma: Güncellenecek veya görüntülenecek müşteri nesnesi
        private Musteri _firma;

        // Kurucu: Seçilen firma bilgilerini alır ve TextBox'lara aktarır
        public FirmaDetayWindow(Musteri secilenFirma)
        {
            InitializeComponent(); // WPF penceresini başlatır
            _firma = secilenFirma;
            // TextBox'lara firma bilgilerini yükler
            txtFirmaAdi.Text = _firma.FirmaAdi;
            txtAdres.Text = _firma.FirmaAdresi;
            txtTelefon.Text = _firma.FirmaTelefonu;
            txtEmail.Text = _firma.FirmaEposta;
            txtVergiDairesi.Text = _firma.VergiDairesi;
            txtVergiNumarasi.Text = _firma.VergiNumarasi;
            txtilgiliKisi.Text = _firma.IlgiliKisi;
            txtilgiliKisiTelefon.Text = _firma.IlgiliKisiTelefonu;
        }

        // BtnKaydet_Click: Kaydet butonuna tıklandığında firma bilgilerini günceller
        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            var pwdDialog = new PasswordDialog { Owner = this }; // Şifre giriş penceresini modal olarak açar
            bool? result = pwdDialog.ShowDialog();
            if (result == true)
            {
                if (PasswordService.Verify(pwdDialog.EnteredPassword))
                {
                    // TextBox'lardan firma bilgilerini günceller
                    _firma.FirmaAdi = txtFirmaAdi.Text;
                    _firma.FirmaAdresi = txtAdres.Text;
                    _firma.FirmaTelefonu = txtTelefon.Text;
                    _firma.FirmaEposta = txtEmail.Text;
                    _firma.VergiDairesi = txtVergiDairesi.Text;
                    _firma.VergiNumarasi = txtVergiNumarasi.Text;
                    _firma.IlgiliKisi = txtilgiliKisi.Text;
                    _firma.IlgiliKisiTelefonu = txtilgiliKisiTelefon.Text;
                    _db.Musteriler.Update(_firma); // Veritabanında firmayı günceller
                    _db.SaveChanges(); // Değişiklikleri kaydeder
                    MessageBox.Show("Firma bilgileri başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close(); // Pencereyi kapatır
                }
                else
                {
                    MessageBox.Show("Şifre yanlış. Güncelleme iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // BtnIptal_Click: İptal butonuna tıklandığında pencereyi kapatır
        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
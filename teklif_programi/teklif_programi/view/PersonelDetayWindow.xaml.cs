using System.Linq;
using System.Windows;
using teklif_programi.Data;
using teklif_programi.Models;

namespace teklif_programi.view
{
    /// <summary>
    /// Interaction logic for PersonelDetayWindow.xaml
    /// </summary>
    public partial class PersonelDetayWindow : Window
    {
        private readonly TeklifDbContext _db = new();
        private readonly Personel _personel;

        public PersonelDetayWindow(Personel secilenPersonel)
        {
            InitializeComponent();

            // Veritabanından personel bilgilerini çekiyoruz (ID bazlı)
            _personel = _db.Personeller.FirstOrDefault(p => p.PersonelId == secilenPersonel.PersonelId)
                        ?? throw new InvalidOperationException("Personel verisi bulunamadı!");

            VeriDoldur();
        }

        private void VeriDoldur()
        {
            txtPersonelKodu.Text = _personel.PersonelId.ToString();
            txtAdSoyad.Text = _personel.AdSoyad;
            txtPozisyon.Text = _personel.Pozisyon;
            txtTelefon.Text = _personel.Telefon;
            txtSifre.Text = _personel.Sifre ?? string.Empty;
        }

        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnPersonelKaydet_Click(object sender, RoutedEventArgs e)
        {
            var pwdDialog = new PasswordDialog { Owner = this };
            if (pwdDialog.ShowDialog() is true)
            {
                const string dogruSifre = "Liya2015";

                if (pwdDialog.EnteredPassword == dogruSifre)
                {
                    _personel.AdSoyad = txtAdSoyad.Text;
                    _personel.Pozisyon = txtPozisyon.Text;
                    _personel.Telefon = txtTelefon.Text;

                    if (!string.IsNullOrWhiteSpace(txtSifre.Text))
                        _personel.Sifre = txtSifre.Text;

                    _db.Personeller.Update(_personel);
                    _db.SaveChanges();

                    MessageBox.Show("Personel bilgileri başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Şifre yanlış. Güncelleme iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

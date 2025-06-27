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

namespace teklif_programi.view
{
    /// <summary>
    /// Interaction logic for PersonelDetayWindow.xaml
    /// </summary>
    public partial class PersonelDetayWindow : Window
    {
        private readonly TeklifDbContext _db = new TeklifDbContext();
        private PersonelData _personel;

        public PersonelDetayWindow(PersonelData secilenPersonel)
        {
            InitializeComponent();

            // Veritabanından personel bilgilerini çekiyoruz (ID bazlı)
            _personel = _db.Personeller.FirstOrDefault(p => p.PersonelKoduID == secilenPersonel.PersonelKoduID);

            if (_personel != null)
            {
                VeriDoldur();
            }
            else
            {
                MessageBox.Show("Personel verisi bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void VeriDoldur()
        {
            txtPersonelKodu.Text = _personel.PersonelKoduID.ToString();
            txtAdSoyad.Text = _personel.AdSoyad;
            txtPozisyon.Text = _personel.Pozisyon;
            txtTelefon.Text = _personel.Telefon;

            // Şifre veritabanındaki gibi gözüksün
            txtSifre.Text = _personel.PersonelSifre ?? string.Empty;
        }


        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BtnPersonelKaydet_Click(object sender, RoutedEventArgs e)
        {
            // Şifre değişikliği için kullanıcıdan onay alalım
            var pwdDialog = new PasswordDialog();
            pwdDialog.Owner = this;
            bool? result = pwdDialog.ShowDialog();

            if (result == true)
            {
                const string dogruSifre = "Liya2015";

                if (pwdDialog.EnteredPassword == dogruSifre)
                {
                    // Güncellemeleri al
                    _personel.AdSoyad = txtAdSoyad.Text;
                    _personel.Pozisyon = txtPozisyon.Text;
                    _personel.Telefon = txtTelefon.Text;

                    // Şifre boş değilse güncelle
                    if (!string.IsNullOrWhiteSpace(txtSifre.Text))
                    {
                        _personel.PersonelSifre = txtSifre.Text;
                    }

                    _db.Personeller.Update(_personel);
                    _db.SaveChanges();

                    MessageBox.Show("Personel bilgileri başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                    this.DialogResult = true; // Ana pencereye başarılı güncelleme bilgisini ver
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Şifre yanlış. Güncelleme iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

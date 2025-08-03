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
        private Personel _personel;

        public PersonelDetayWindow(Personel secilenPersonel)
        {
            InitializeComponent();

            // Veritabanından personel bilgilerini çekiyoruz (ID bazlı)
            _personel = _db.Personeller.FirstOrDefault(p => p.personel_id == secilenPersonel.personel_id);

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
            txtPersonelKodu.Text = _personel.personel_id.ToString();
            txtAdSoyad.Text = _personel.ad_soyad;
            txtPozisyon.Text = _personel.pozisyon;
            txtTelefon.Text = _personel.telefon;

            // Şifre veritabanındaki gibi gözüksün
            txtSifre.Text = _personel.sifre ?? string.Empty;
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
                    _personel.ad_soyad = txtAdSoyad.Text;
                    _personel.pozisyon = txtPozisyon.Text;
                    _personel.telefon = txtTelefon.Text;

                    // Şifre boş değilse güncelle
                    if (!string.IsNullOrWhiteSpace(txtSifre.Text))
                    {
                        _personel.sifre = txtSifre.Text;
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

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
    public partial class FirmaDetayWindow : Window
    {
        private readonly TeklifDbContext _db = new TeklifDbContext();
        private Firma _firma;

        public FirmaDetayWindow(Firma secilenFirma)
        {
            InitializeComponent();
            _firma = secilenFirma;

            // TextBox'lara firma bilgilerini aktar
            txtFirmaAdi.Text = _firma.FirmaAdi;
            txtAdres.Text = _firma.Adres;
            txtTelefon.Text = _firma.Telefon;
            txtEmail.Text = _firma.Email;
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            var pwdDialog = new PasswordDialog();
            pwdDialog.Owner = this;  // Ana pencereyi sahibi yapar, modal olur

            bool? result = pwdDialog.ShowDialog();

            if (result == true)
            {
                const string dogruSifre = "Liya2015"; // Şifreni buraya koy

                if (pwdDialog.EnteredPassword == dogruSifre)
                {
                    _firma.FirmaAdi = txtFirmaAdi.Text;
                    _firma.Adres = txtAdres.Text;
                    _firma.Telefon = txtTelefon.Text;
                    _firma.Email = txtEmail.Text;

                    _db.Firmalar.Update(_firma);
                    _db.SaveChanges();

                    MessageBox.Show("Firma bilgileri başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Şifre yanlış. Güncelleme iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
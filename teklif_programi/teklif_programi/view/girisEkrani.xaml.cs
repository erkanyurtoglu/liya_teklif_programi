using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using teklif_programi.Data;
using teklif_programi.Helpers;

namespace teklif_programi.view
{
    /// <summary>
    /// Interaction logic for girisEkrani.xaml
    /// </summary>
    public partial class girisEkrani : Window
    {
        private TextBlock? _kullaniciPlaceholder;

        public girisEkrani()
        {
            InitializeComponent();

            txtKullanici.Loaded += (_, __) =>
            {
                _kullaniciPlaceholder = (TextBlock)txtKullanici.Template.FindName("PlaceholderText", txtKullanici);
                UpdatePlaceholder();
            };

            cmbGirisTipi.SelectionChanged += (_, __) => UpdatePlaceholder();
        }

        private void UpdatePlaceholder()
        {
            if (_kullaniciPlaceholder == null) return;
            _kullaniciPlaceholder.Text = cmbGirisTipi.SelectedIndex == 0 ? "Telefon" : "Kullanıcı Adı";
        }

        private async void btnGirisYap_Click_1(object sender, RoutedEventArgs e)
        {
            txtErrorMessage.Visibility = Visibility.Collapsed;
            try
            {
                using var context = new TeklifDbContext();
                var kullanici = txtKullanici.Text;
                var sifre = pwdSifre.Visibility == Visibility.Visible ? pwdSifre.Password : txtSifre.Text;

                bool girisBasarili;

                if (cmbGirisTipi.SelectedIndex == 0)
                {
                    var personel = await context.Personeller
                        .FirstOrDefaultAsync(p => p.Telefon == kullanici && p.Sifre == sifre);
                    girisBasarili = personel != null;
                    SessionManager.CurrentPersonel = personel;
                    SessionManager.CurrentAdmin = null;
                }
                else
                {
                    var admin = await context.Adminler
                        .FirstOrDefaultAsync(a => a.KullaniciAdi == kullanici && a.Sifre == sifre);
                    girisBasarili = admin != null;
                    SessionManager.CurrentAdmin = admin;
                    SessionManager.CurrentPersonel = null;
                }

                if (girisBasarili)
                {
                    var anaEkran = new anaEkran();
                    anaEkran.Show();
                    Close();
                }
                else
                {
                    txtErrorMessage.Text = "Giriş bilgileri hatalı.";
                    txtErrorMessage.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Giriş sırasında bir hata oluştu: {ex.Message}",
                    "Hata",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            if (pwdSifre.Visibility == Visibility.Visible)
            {
                txtSifre.Text = pwdSifre.Password;
                pwdSifre.Visibility = Visibility.Collapsed;
                txtSifre.Visibility = Visibility.Visible;
                btnTogglePassword.Content = "🔓";
                pwdPlaceholder.Visibility = Visibility.Collapsed;
                txtSifre.Focus();
                txtSifre.CaretIndex = txtSifre.Text.Length;
            }
            else
            {
                pwdSifre.Password = txtSifre.Text;
                txtSifre.Visibility = Visibility.Collapsed;
                pwdSifre.Visibility = Visibility.Visible;
                btnTogglePassword.Content = "🔐";
                pwdPlaceholder.Visibility = string.IsNullOrEmpty(pwdSifre.Password) ? Visibility.Visible : Visibility.Collapsed;
                pwdSifre.Focus();
            }
        }

        private void pwdSifre_PasswordChanged(object sender, RoutedEventArgs e)
        {
            pwdPlaceholder.Visibility = string.IsNullOrEmpty(pwdSifre.Password) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chkBeniHatirla_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
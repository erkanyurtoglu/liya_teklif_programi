using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using teklif_programi.Data;

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
                var sifre = txtSifre.Text;

                bool girisBasarili;

                if (cmbGirisTipi.SelectedIndex == 0)
                {
                    girisBasarili = await context.Personeller
                        .AnyAsync(p => p.Telefon == kullanici && p.Sifre == sifre);
                }
                else
                {
                    girisBasarili = await context.Adminler
                        .AnyAsync(a => a.KullaniciAdi == kullanici && a.Sifre == sifre);
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

        private void chkBeniHatirla_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
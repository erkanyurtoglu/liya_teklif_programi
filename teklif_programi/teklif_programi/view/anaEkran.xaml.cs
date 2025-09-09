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
using teklif_programi.Helpers;

namespace teklif_programi.view
{
    /// <summary>
    /// Interaction logic for anaEkran.xaml
    /// </summary>
    public partial class anaEkran : Window
    {
        public anaEkran()
        {
            InitializeComponent();

            if (SessionManager.CurrentPersonel != null)
            {
                btnPersonel.Visibility = Visibility.Collapsed;
                btnIstatistik.Visibility = Visibility.Collapsed;
                homeButtons.Columns = 2;
            }
            else
            {
                homeButtons.Columns = 3;
            }
        }

        private void ShowNav(StackPanel activeNav)
        {
            navBorder.Visibility = Visibility.Visible;
            customerNav.Visibility = Visibility.Collapsed;
            productNav.Visibility = Visibility.Collapsed;
            personNav.Visibility = Visibility.Collapsed;
            teklifNav.Visibility = Visibility.Collapsed;
            activeNav.Visibility = Visibility.Visible;
        }

        // Home buttons
        private void HomeMusteri_Click(object sender, RoutedEventArgs e)
        {
            ShowNav(customerNav);
            contentArea.Content = new Firmalarim();
        }

        private void HomeUrun_Click(object sender, RoutedEventArgs e)
        {
            ShowNav(productNav);
            contentArea.Content = new Urunlerim();
        }

        private void HomePersonel_Click(object sender, RoutedEventArgs e)
        {
            ShowNav(personNav);
            contentArea.Content = new Personellerim();
        }

        private void HomeTeklif_Click(object sender, RoutedEventArgs e)
        {
            ShowNav(teklifNav);
            contentArea.Content = new TeklifVer();
        }

        private void HomeIstatistik_Click(object sender, RoutedEventArgs e)
        {
            navBorder.Visibility = Visibility.Visible;
            customerNav.Visibility = Visibility.Collapsed;
            productNav.Visibility = Visibility.Collapsed;
            personNav.Visibility = Visibility.Collapsed;
            teklifNav.Visibility = Visibility.Collapsed;
            contentArea.Content = new Istatistikler();
        }


        private void btnAnaSayfa_Click(object sender, RoutedEventArgs e)
        {
            navBorder.Visibility = Visibility.Collapsed;
            customerNav.Visibility = Visibility.Collapsed;
            productNav.Visibility = Visibility.Collapsed;
            personNav.Visibility = Visibility.Collapsed;
            teklifNav.Visibility = Visibility.Collapsed;
            contentArea.Content = homeGrid;
        }


        // Navigation buttons

        private void btnFirmalarim_Click(object sender, RoutedEventArgs e) => contentArea.Content = new Firmalarim();
        private void btnFirmaEkle_Click(object sender, RoutedEventArgs e) => contentArea.Content = new FirmaEkle();
        private void btnUrunlerim_Click(object sender, RoutedEventArgs e) => contentArea.Content = new Urunlerim();
        private void btnUrunEkle_Click(object sender, RoutedEventArgs e) => contentArea.Content = new UrunEkle();
        private void btnPersonellerim_Click(object sender, RoutedEventArgs e) => contentArea.Content = new Personellerim();
        private void btnPersonelEkle_Click(object sender, RoutedEventArgs e) => contentArea.Content = new PersonelEkle();
        private void btnTeklifVer_Click(object sender, RoutedEventArgs e) => contentArea.Content = new TeklifVer();
        private void btnGecmisTekliflerim_Click(object sender, RoutedEventArgs e) => contentArea.Content = new GecmisTekliflerim();
        private void btnAlinanTekliflerim_Click(object sender, RoutedEventArgs e) => contentArea.Content = new AlinanTekliflerim();
        private void btnBitenTekliflerim_Click(object sender, RoutedEventArgs e) => contentArea.Content = new BitenTekliflerim();


        private void Logout()
        {
            var result = MessageBox.Show(
                "Oturumu kapatmak istediğinize emin misiniz?",
                "Çıkış Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            SessionManager.CurrentPersonel = null;
            SessionManager.CurrentAdmin = null;
            var login = new girisEkrani();
            login.Show();
            Close();
        }

        private void HomeCikis_Click(object sender, RoutedEventArgs e) => Logout();

        private void btnCikis_Click(object sender, RoutedEventArgs e) => Logout();
    }
}

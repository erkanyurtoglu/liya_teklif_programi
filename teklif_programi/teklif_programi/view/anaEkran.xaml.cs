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
            contentArea.Content = new Firmalarim(); // Varsayılan ekran
        }

        private void btnFirmalarim_Click(object sender, RoutedEventArgs e) => contentArea.Content = new Firmalarim();
        private void btnFirmaEkle_Click(object sender, RoutedEventArgs e) => contentArea.Content = new FirmaEkle();
        private void btnUrunlerim_Click(object sender, RoutedEventArgs e) => contentArea.Content = new Urunlerim();
        private void btnUrunEkle_Click(object sender, RoutedEventArgs e) => contentArea.Content = new UrunEkle();
        private void btnPersonellerim_Click(object sender, RoutedEventArgs e) => contentArea.Content = new Personellerim();
        private void btnPersonelEkle_Click(object sender, RoutedEventArgs e) => contentArea.Content = new PersonelEkle();
        private void btnTeklifVer_Click(object sender, RoutedEventArgs e) => contentArea.Content = new TeklifVer();
        private void btnGecmisTekliflerim_Click(object sender, RoutedEventArgs e) => contentArea.Content = new GecmisTekliflerim();
    }
}

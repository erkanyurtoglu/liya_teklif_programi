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
    /// Interaction logic for girisEkrani.xaml
    /// </summary>
    public partial class girisEkrani : Window
    {
        public girisEkrani()
        {
            InitializeComponent();
        }

        private void btnGirisYap_Click_1(object sender, RoutedEventArgs e)
        {
            anaEkran anaEkran = new anaEkran();
            anaEkran.Show();
        }

        private void chkBeniHatirla_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}

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
using teklif_programi.Models;

namespace teklif_programi.view
{
    /// <summary>
    /// Interaction logic for PasswordDialog.xaml
    /// </summary>
    public partial class PasswordDialog : Window
    {
        public string EnteredPassword { get; private set; }

        public PasswordDialog()
        {
            InitializeComponent();
        }

        private void BtnTamam_Click(object sender, RoutedEventArgs e)
        {
            EnteredPassword = passwordBox.Password;
            this.DialogResult = true;
        }

        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void BtnDetay_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenUrun = button?.DataContext as UrunData;

            if (secilenUrun != null)
            {
                var detayWindow = new UrunDetayWindow(secilenUrun);
                detayWindow.ShowDialog();
            }
        }


    }
}

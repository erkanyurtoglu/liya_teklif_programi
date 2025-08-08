using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions; // Regex kullanmak için
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input; // TextCompositionEventArgs için
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace teklif_programi.view
{
    /// <summary>
    /// TeklifDetayWindow.xaml için etkileşim mantığı.
    /// Bu pencere, teklif detaylarını görüntülemek veya düzenlemek için kullanılır.
    /// </summary>
    public partial class TeklifDetayWindow : Window
    {
        // Pencereyi başlatan constructor
        public TeklifDetayWindow()
        {
            InitializeComponent(); // XAML tarafındaki bileşenleri yükler
        }

        /// <summary>
        /// TextBox gibi alanlara yalnızca sayı ve ondalık nokta (.) girişine izin verir.
        /// Bu metod, PreviewTextInput olayı ile tetiklenir.
        /// </summary>
        private void OnlyAllowNumbers(object sender, TextCompositionEventArgs e)
        {
            // ^ = baştan başla, [^0-9.]+ = rakam VEYA nokta dışında bir karakter varsa eşleşir
            Regex regex = new Regex("[^0-9.]+");

            // Eğer girilen karakter regex ile eşleşirse (yani sayı değilse) engelle
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using teklif_programi.Models;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    public partial class AlinanTeklifDetayWindow : Window
    {
        private static readonly Regex _intRegex = new Regex(@"^\d*$", RegexOptions.Compiled);

        public AlinanTeklifDetayWindow(Teklif teklif)
        {
            InitializeComponent();
            DataContext = new TeklifDetayViewModel(teklif);
        }

        private void OnlyAllowNumbers(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            var proposed = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength)
                                  .Insert(tb.SelectionStart, e.Text);

            e.Handled = !_intRegex.IsMatch(proposed);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // İhtiyaç halinde doldurulabilir
        }

        private void SatisSozlesmesi_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TeklifDetayViewModel vm)
            {
                var window = new SatisSozlesmesiWindow(vm);
                window.ShowDialog();
            }
        }
    }
}
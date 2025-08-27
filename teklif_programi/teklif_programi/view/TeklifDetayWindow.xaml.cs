using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using teklif_programi.Models;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    public partial class TeklifDetayWindow : Window
    {
        public TeklifDetayWindow(Teklif teklif)
        {
            InitializeComponent();
            DataContext = new TeklifDetayViewModel(teklif);
        }

        // --- Giriş filtreleri ---

        private static readonly Regex _intRegex = new Regex(@"^\d*$", RegexOptions.Compiled);

        private void OnlyAllowNumbers(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            var proposed = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength)
                                  .Insert(tb.SelectionStart, e.Text);

            e.Handled = !_intRegex.IsMatch(proposed);
        }

        private void TextBox_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var text = (string)e.DataObject.GetData(DataFormats.Text);
                var tb = (TextBox)sender;
                var proposed = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength)
                                      .Insert(tb.SelectionStart, text);

                if (!_intRegex.IsMatch(proposed))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // ihtiyaca göre doldurabilirsin
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

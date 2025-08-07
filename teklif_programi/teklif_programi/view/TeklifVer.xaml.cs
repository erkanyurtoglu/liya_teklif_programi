using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    public partial class TeklifVer : UserControl
    {
        public TeklifVer()
        {
            InitializeComponent();
            DataContext = new TeklifVerViewModel();
        }

        private void OnlyAllowNumbers(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out _) && e.Text != ".";
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string newText = textBox.Text + e.Text;
                e.Handled = !decimal.TryParse(newText, out _);
            }
        }

        private void SatisSozlesmesi_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = (TeklifVerViewModel)DataContext;
            var satisSozlesmesiWindow = new SatisSozlesmesiWindow(viewModel);
            satisSozlesmesiWindow.ShowDialog();
        }

    }
}
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
            e.Handled = !decimal.TryParse(((TextBox)sender).Text + e.Text, out _);
        }

    }
}
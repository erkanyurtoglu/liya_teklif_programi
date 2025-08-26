using System.Windows;
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

        // Test için parametresiz kurucu (isteğe bağlı, production'da kaldırılabilir)
        public TeklifDetayWindow()
        {
            InitializeComponent();
            DataContext = new TeklifDetayViewModel(new Teklif { TeklifId = 1 }); // Test için sabit ID
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

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

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

        private void YeniUrunEkle_Click(object sender, RoutedEventArgs e)
        {
            var urunSecimWindow = new UrunSecimWindow();
            if (urunSecimWindow.ShowDialog() == true)
            {
                var viewModel = urunSecimWindow.DataContext as UrunSecimViewModel;
                if (viewModel?.SecilenUrun != null)
                {
                    var vm = DataContext as TeklifDetayViewModel;
                    if (vm != null)
                    {
                        vm.EkleUrun(viewModel.SecilenUrun, viewModel.YeniUrunAdet, viewModel.YeniUrunIndirimliFiyat);
                    }
                }
            }
        }
    }
}
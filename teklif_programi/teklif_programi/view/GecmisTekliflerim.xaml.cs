using System.Windows.Controls;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    public partial class GecmisTekliflerim : UserControl
    {
        public GecmisTekliflerim()
        {
            InitializeComponent();
            DataContext = new GecmisTekliflerViewModel();
        }
    }
}
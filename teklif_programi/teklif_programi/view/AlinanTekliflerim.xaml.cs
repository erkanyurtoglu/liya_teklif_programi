using System.Windows.Controls;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    /// <summary>
    /// AlinanTekliflerim.xaml için etkileşim mantığı
    /// </summary>
    public partial class AlinanTekliflerim : UserControl
    {
        public AlinanTekliflerim()
        {
            InitializeComponent();
            DataContext = new AlinanTekliflerViewModel();
        }
    }
}
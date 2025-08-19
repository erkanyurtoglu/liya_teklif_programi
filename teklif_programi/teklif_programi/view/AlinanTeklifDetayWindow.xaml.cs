using System.Windows;
using teklif_programi.Models;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    public partial class AlinanTeklifDetayWindow : Window
    {
        public AlinanTeklifDetayWindow(Teklif teklif)
        {
            InitializeComponent();
            DataContext = new AlinanTeklifDetayViewModel(teklif);
        }
    }
}
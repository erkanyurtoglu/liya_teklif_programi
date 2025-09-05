using System.Windows;
using System.Windows.Controls;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    public partial class Istatistikler : UserControl
    {
        public Istatistikler()
        {
            InitializeComponent();
            DataContext = new IstatistikViewModel();
        }
    }
}
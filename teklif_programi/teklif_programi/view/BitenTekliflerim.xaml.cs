using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    /// <summary>
    /// BitenTekliflerim.xaml için etkileşim mantığı
    /// </summary>
    public partial class BitenTekliflerim : UserControl
    {
        public BitenTekliflerim()
        {
            InitializeComponent();
            DataContext = new BitenTekliflerViewModel();
        }
    }
}

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using teklif_programi.Data;

namespace teklif_programi.view
{
    /// <summary>
    /// Interaction logic for Urunlerim.xaml
    /// </summary>
    public partial class Urunlerim : UserControl
    {
        public TeklifDbContext _db = new TeklifDbContext();

        public Urunlerim()
        {
            InitializeComponent();
            UrunListele();
        }

        private void UrunListele()
        {
            var urunler = _db.Urunler.ToList(); // Veritabanından çek
            dataGridUrunler.ItemsSource = urunler;    // DataGrid'e bağla (dgFirmalar senin x:Name)
        }
    }
}

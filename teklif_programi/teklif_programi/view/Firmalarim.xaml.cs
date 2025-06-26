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
    /// Interaction logic for Firmalarim.xaml
    /// </summary>
    public partial class Firmalarim : UserControl
    {
        public TeklifDbContext _db = new TeklifDbContext();

        public Firmalarim()
        {
            InitializeComponent();
            FirmaListele(); // Sayfa açılınca firmaları yükle
        }
        
        private void FirmaListele(string arama = "")
        {
            var firmalar = string.IsNullOrWhiteSpace(arama)
                ? _db.Firmalar.ToList()
                : _db.Firmalar
                      .Where(f => f.FirmaAdi.Contains(arama) || f.Telefon.Contains(arama))
                      .ToList();

            dgFirmalar.ItemsSource = firmalar;
        }

        private void txtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            FirmaListele(txtArama.Text.Trim());
        }
    }
}

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
using teklif_programi.Models;

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

        private void UrunListele(string arama = "")
        {
            var urunler = string.IsNullOrWhiteSpace(arama)
                ? _db.Urunler.ToList()
                : _db.Urunler
                      .Where(f => f.Aciklama.Contains(arama) || f.UrunKoduID.Contains(arama))
                      .ToList();

            dataGridUrunler.ItemsSource = urunler;
        }

        private void txtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            UrunListele(txtArama.Text.Trim());
        }

        private void BtnDetay_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenUrun = button?.DataContext as UrunData;

            if (secilenUrun != null)
            {
                var detayPencere = new UrunDetayWindow(secilenUrun);
                detayPencere.ShowDialog();
            }

            // Değişiklikleri listeye yansıt
            UrunListele(txtArama.Text.Trim());

        }
    }
}

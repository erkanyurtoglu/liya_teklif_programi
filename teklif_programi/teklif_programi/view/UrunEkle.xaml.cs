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
    /// Interaction logic for UrunEkle.xaml
    /// </summary>
    public partial class UrunEkle : UserControl
    {
        public TeklifDbContext _db = new TeklifDbContext();

        public UrunEkle()
        {
            InitializeComponent();
        }

        private void Kaydet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UrunData yeniUrun = new UrunData()
                {
                    UrunKoduID = txtUrunKodu.Text.Trim(),
                    Kategori = txtUrunKategori.Text.Trim(),
                    Aciklama = txtUrunAciklama.Text.Trim(),
                    Adet = int.Parse(txtUrunAdedi.Text.Trim()),
                    BirimSatisFiyati = decimal.Parse(txtBirimSatisFiyati.Text.Trim()),
                    SatisToplamFiyati = decimal.Parse(txtSatisToplamFiyati.Text.Trim()),
                    YurticiMaliyet = decimal.Parse(txtYurtiçiMaliyet.Text.Trim()),
                    ToplamFiyat = decimal.Parse(txtToplamFiyat.Text.Trim())
                };

                _db.Urunler.Add(yeniUrun);
                _db.SaveChanges();

                MessageBox.Show("Ürün başarıyla kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

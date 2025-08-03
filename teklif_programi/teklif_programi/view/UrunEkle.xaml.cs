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
                Urun yeniUrun = new Urun()
                {
                    urun_kodu = txtUrunKodu.Text.Trim(),
                    kategori = txtUrunKategori.Text.Trim(),
                    urun_aciklamasi = txtUrunAciklama.Text.Trim(),
                    birim_fiyat = decimal.Parse(txtBirimSatisFiyati.Text.Trim()),
                    maliyet_fiyati = decimal.Parse(txtYurticiMaliyet.Text.Trim()),
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

        private void Iptal_Click(object sender, RoutedEventArgs e)
        {
            txtUrunKodu.Text = "";
            txtUrunKategori.Text = "";
            txtUrunAciklama.Text = "";
            txtBirimSatisFiyati.Text = "";
            txtYurticiMaliyet.Text = "";
        }

    }
}

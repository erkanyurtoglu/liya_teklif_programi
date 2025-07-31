using System;
using System.Windows;
using System.Windows.Controls;
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
                if (!int.TryParse(txtUrunAdedi.Text.Trim(), out int adet) ||
                    !decimal.TryParse(txtBirimSatisFiyati.Text.Trim(), out decimal birimSatisFiyati) ||
                    !decimal.TryParse(txtYurtiçiMaliyet.Text.Trim(), out decimal yurticiMaliyet))
                {
                    MessageBox.Show("Lütfen geçerli bir adet, birim satış fiyatı ve yurtiçi maliyet girin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                UrunData yeniUrun = new UrunData
                {
                    UrunKoduID = txtUrunKodu.Text.Trim(),
                    Kategori = txtUrunKategori.Text.Trim(),
                    Aciklama = txtUrunAciklama.Text.Trim(),
                    Adet = adet,
                    BirimSatisFiyati = birimSatisFiyati,
                    YurticiMaliyet = yurticiMaliyet,
                    GenelIndirim = 0,
                    KdvOrani = 0
                };

                _db.Urunler.Add(yeniUrun);
                _db.SaveChanges();

                MessageBox.Show("Ürün başarıyla kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                txtUrunKodu.Text = "";
                txtUrunKategori.Text = "";
                txtUrunAciklama.Text = "";
                txtUrunAdedi.Text = "";
                txtBirimSatisFiyati.Text = "";
                txtYurtiçiMaliyet.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

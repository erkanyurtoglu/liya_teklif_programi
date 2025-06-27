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
using teklif_programi.Data;
using teklif_programi.Models;

namespace teklif_programi.view
{
    /// <summary>
    /// Interaction logic for UrunDetayWindow.xaml
    /// </summary>
    public partial class UrunDetayWindow : Window
    {
        private UrunData _urun;
        private readonly TeklifDbContext _db = new TeklifDbContext();

        public UrunDetayWindow(UrunData urun)
        {
            InitializeComponent();
            _urun = urun;

            // TextBox'lara bilgileri doldur
            txtUrunKodu.Text = _urun.UrunKoduID;
            txtKategori.Text = _urun.Kategori;
            txtAciklama.Text = _urun.Aciklama;
            txtAdet.Text = _urun.Adet.ToString();
            txt2025BirimSatisFiyati.Text = _urun.BirimSatisFiyati.ToString("F2");
            txt2025SatisToplamFiyati.Text = _urun.SatisToplamFiyati.ToString("F2");
            txtYurticiMaliyetBirimFiyati.Text = _urun.YurticiMaliyet.ToString("F2");
            txtToplamFiyat.Text = _urun.ToplamFiyat.ToString("F2");
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            var pwdWindow = new PasswordDialog();
            pwdWindow.Owner = this;

            if (pwdWindow.ShowDialog() == true && pwdWindow.EnteredPassword == "Liya2015")
            {
                // Güncelleme işlemi
                _urun.Kategori = txtKategori.Text;
                _urun.Aciklama = txtAciklama.Text;
                _urun.Adet = int.Parse(txtAdet.Text);
                _urun.BirimSatisFiyati = decimal.Parse(txt2025BirimSatisFiyati.Text);
                _urun.SatisToplamFiyati = decimal.Parse(txt2025SatisToplamFiyati.Text);
                _urun.YurticiMaliyet = decimal.Parse(txtYurticiMaliyetBirimFiyati.Text);
                _urun.ToplamFiyat = decimal.Parse(txtToplamFiyat.Text);

                _db.Urunler.Update(_urun);
                _db.SaveChanges();

                MessageBox.Show("Ürün başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Şifre hatalı veya işlem iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnIptal_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

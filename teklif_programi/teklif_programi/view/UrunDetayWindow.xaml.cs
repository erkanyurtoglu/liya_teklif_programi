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
        private Urun _urun;
        private readonly TeklifDbContext _db = new TeklifDbContext();

        public UrunDetayWindow(Urun urun)
        {
            InitializeComponent();
            _urun = urun;

            // TextBox'lara bilgileri doldur
            txtUrunKodu.Text = _urun.urun_kodu;
            txtKategori.Text = _urun.kategori;
            txtAciklama.Text = _urun.urun_aciklamasi;
            txt2025BirimSatisFiyati.Text = _urun.birim_fiyat.ToString("F2");
            txtYurticiMaliyetBirimFiyati.Text = _urun.maliyet_fiyati.ToString("F2");
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            var pwdWindow = new PasswordDialog();
            pwdWindow.Owner = this;

            if (pwdWindow.ShowDialog() == true && pwdWindow.EnteredPassword == "Liya2015")
            {
                // Güncelleme işlemi
                _urun.kategori = txtKategori.Text;
                _urun.urun_aciklamasi = txtAciklama.Text;
                _urun.birim_fiyat = decimal.Parse(txt2025BirimSatisFiyati.Text);
                _urun.maliyet_fiyati = decimal.Parse(txtYurticiMaliyetBirimFiyati.Text);

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

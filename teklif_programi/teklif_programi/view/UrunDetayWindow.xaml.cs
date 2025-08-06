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
            txtUrunKodu.Text = _urun.UrunKodu;
            txtKategori.Text = _urun.Kategori;
            txtAciklama.Text = _urun.UrunAciklamasi;
            txt2025BirimSatisFiyati.Text = _urun.BirimFiyat.ToString("F2");
            txtDolarBirimSatisFiyati.Text = _urun.FiyatUSD.ToString("F2");
            txtEuroBirimSatisFiyati.Text = _urun.FiyatEUR.ToString("F2");
            txtYurticiMaliyetBirimFiyati.Text = _urun.MaliyetFiyati.ToString("F2");
        }

        private void BtnKaydet_Click(object sender, RoutedEventArgs e)
        {
            var pwdWindow = new PasswordDialog();
            pwdWindow.Owner = this;

            if (pwdWindow.ShowDialog() == true && pwdWindow.EnteredPassword == "Liya2015")
            {
                // Güncelleme işlemi
                _urun.Kategori = txtKategori.Text;
                _urun.UrunAciklamasi = txtAciklama.Text;
                _urun.BirimFiyat = decimal.Parse(txt2025BirimSatisFiyati.Text);
                _urun.MaliyetFiyati = decimal.Parse(txtYurticiMaliyetBirimFiyati.Text);

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

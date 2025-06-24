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
    /// Interaction logic for FirmaEkle.xaml
    /// </summary>
    public partial class FirmaEkle : UserControl
    {
        public FirmaEkle()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) //kaydet butonu
        {
            // TextBox'lardan verileri aldım.
            string firmaAdi = txtFirmaAdi.Text.Trim(); // trim, baş ve sondaki boşlukları temizlesin diye koydum.
            string adres = txtAdres.Text.Trim();
            string telefon = txtTelefon.Text.Trim();
            string email = txtEmail.Text.Trim();


            // Zorunlu alan kontrolü yaaptım.
            if (string.IsNullOrEmpty(firmaAdi))
            {
                MessageBox.Show("Lütfen Firma Adını Giriniz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Yeni firma nesnesi oluşturdum.
            var firma = new Firma
            {
                FirmaAdi = firmaAdi,
                Adres = adres,
                Telefon = telefon,
                Email = email
            };

            // Veritabanına ekle
            using (var context = new TeklifDbContext())
            {
                try
                {
                    context.Firmalar.Add(firma);
                    context.SaveChanges();
                    MessageBox.Show("Firma başarıyla eklendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                    // TextBox'ları temizle
                    txtFirmaAdi.Text = "";
                    txtAdres.Text = "";
                    txtTelefon.Text = "";
                    txtEmail.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }



        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // İptal Butonu
        {
            // İptal butonuna tıklanırsa TextBox'ları temizle
            txtFirmaAdi.Text = "";
            txtAdres.Text = "";
            txtTelefon.Text = "";
            txtEmail.Text = "";
        }
    }
}

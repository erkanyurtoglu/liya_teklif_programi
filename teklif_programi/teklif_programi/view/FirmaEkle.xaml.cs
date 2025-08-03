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
            string vergiNumarasi = txtVergiNumarasi.Text.Trim();
            string vergiDairesi = txtVergiDairesi.Text.Trim();
            string ilgiliKisi = txtilgiliKisi.Text.Trim();
            string ilgiliKisiTelefon = txtilgiliKisiTelefonu.Text.Trim();


            // Zorunlu alan kontrolü yaaptım.
            if (string.IsNullOrEmpty(firmaAdi))
            {
                MessageBox.Show("Lütfen Firma Adını Giriniz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Yeni firma nesnesi oluşturdum.
            var firma = new Musteri
            {
                firma_adi = firmaAdi,
                firma_adresi = adres,
                firma_telefonu = telefon,
                firma_eposta = email,
                vergi_numarasi = vergiNumarasi,
                vergi_dairesi = vergiDairesi,
                ilgili_kisi = ilgiliKisi,
                ilgili_kisi_telefonu = ilgiliKisiTelefon
            };

            // Veritabanına ekle
            using (var context = new TeklifDbContext())
            {
                try
                {
                    context.Musteriler.Add(firma);
                    context.SaveChanges();
                    MessageBox.Show("Firma başarıyla eklendi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

                    // TextBox'ları temizle
                    txtFirmaAdi.Text = "";
                    txtAdres.Text = "";
                    txtTelefon.Text = "";
                    txtEmail.Text = "";
                    txtVergiNumarasi.Text = "";
                    txtVergiDairesi.Text = ""; 
                    txtilgiliKisi.Text = "";
                    txtilgiliKisiTelefonu.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata oluştu: {ex.Message}\n\n{ex.InnerException?.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);

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
            txtVergiNumarasi.Text = "";
            txtVergiDairesi.Text = "";
            txtilgiliKisi.Text = "";
            txtilgiliKisiTelefonu.Text = "";
        }
    }
}

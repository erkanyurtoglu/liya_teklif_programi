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

        private void BtnDetay_Click(object sender, RoutedEventArgs e)
        {
            var firma = (sender as Button)?.DataContext as Firma;
            if (firma != null)
            {
                var detayPencere = new FirmaDetayWindow(firma);
                detayPencere.ShowDialog();
                FirmaListele(); // Güncellemeden sonra listeyi yenile
            }
        }

        private void BtnSil_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenFirma = button?.DataContext as Firma;

            if (secilenFirma == null)
            {
                MessageBox.Show("Silinecek firma bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Şifre penceresini aç
            var pwdDialog = new PasswordDialog();
            pwdDialog.Owner = Window.GetWindow(this); // UserControl içinden ana pencereyi alır

            bool? result = pwdDialog.ShowDialog();

            if (result == true)
            {
                const string dogruSifre = "Liya2015";

                if (pwdDialog.EnteredPassword == dogruSifre)
                {
                    using (var db = new TeklifDbContext())
                    {
                        var firma = db.Firmalar.FirstOrDefault(f => f.FirmaKoduID == secilenFirma.FirmaKoduID);

                        if (firma != null)
                        {
                            if (MessageBox.Show("Firma kalıcı olarak silinecek. Emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                db.Firmalar.Remove(firma);
                                db.SaveChanges();
                                MessageBox.Show("Firma başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }

                    FirmaListele(txtArama.Text.Trim()); // Listeyi güncelle
                }
                else
                {
                    MessageBox.Show("Şifre yanlış. Silme işlemi iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


    }
}

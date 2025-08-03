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
                ? _db.Musteriler.ToList()
                : _db.Musteriler
                      .Where(f => f.firma_adi.Contains(arama) || f.firma_telefonu.Contains(arama))
                      .ToList();

            dgFirmalar.ItemsSource = firmalar;
        }

        private void txtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            FirmaListele(txtArama.Text.Trim());
        }

        private void BtnDetay_Click(object sender, RoutedEventArgs e)
        {
            var firma = (sender as Button)?.DataContext as Musteri;
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
            var secilenFirma = button?.DataContext as Musteri;

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
                        var firma = db.Musteriler.FirstOrDefault(f => f.musteri_id == secilenFirma.musteri_id);

                        if (firma != null)
                        {
                            if (MessageBox.Show("Firma kalıcı olarak silinecek. Emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                            {
                                db.Musteriler.Remove(firma);
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

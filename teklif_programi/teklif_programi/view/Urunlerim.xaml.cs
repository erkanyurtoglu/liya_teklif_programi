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
    /// Interaction logic for Urunlerim.xaml
    /// </summary>
    public partial class Urunlerim : UserControl
    {
        public TeklifDbContext _db = new TeklifDbContext();

        public Urunlerim()
        {
            InitializeComponent();
            UrunListele();
        }

        private void UrunListele(string arama = "")
        {
            var urunler = string.IsNullOrWhiteSpace(arama)
                ? _db.Urunler.ToList()
                : _db.Urunler
                      .Where(f => f.Aciklama.Contains(arama) || f.UrunKoduID.Contains(arama))
                      .ToList();

            dataGridUrunler.ItemsSource = urunler;
        }

        private void txtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            UrunListele(txtArama.Text.Trim());
        }

        private void BtnDetay_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenUrun = button?.DataContext as UrunData;

            if (secilenUrun != null)
            {
                var detayPencere = new UrunDetayWindow(secilenUrun);
                detayPencere.ShowDialog();
            }

            // Değişiklikleri listeye yansıt
            UrunListele(txtArama.Text.Trim());

        }

        private void BtnUrunSil_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenUrun = button?.DataContext as UrunData;

            if (secilenUrun == null)
            {
                MessageBox.Show("Silinecek ürün bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Şifre doğrulama penceresi açılır
            var pwdDialog = new PasswordDialog();
            pwdDialog.Owner = Window.GetWindow(this);

            bool? result = pwdDialog.ShowDialog();

            if (result == true)
            {
                const string dogruSifre = "Liya2015";

                if (pwdDialog.EnteredPassword == dogruSifre)
                {
                    if (MessageBox.Show("Bu ürün kalıcı olarak silinecek. Emin misiniz?", "Onay", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        using (var db = new TeklifDbContext())
                        {
                            var urun = db.Urunler.FirstOrDefault(u => u.UrunKoduID == secilenUrun.UrunKoduID);

                            if (urun != null)
                            {
                                db.Urunler.Remove(urun);
                                db.SaveChanges();
                                MessageBox.Show("Ürün başarıyla silindi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }

                        UrunListele(txtArama.Text.Trim());
                    }
                }
                else
                {
                    MessageBox.Show("Şifre yanlış. Silme işlemi iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}

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
    /// Interaction logic for Personellerim.xaml
    /// </summary>
    public partial class Personellerim : UserControl
    {
        public TeklifDbContext _db = new TeklifDbContext();

        public Personellerim()
        {
            InitializeComponent();
            PersonelListele();
        }
        private void PersonelListele(string arama = "")
        {
            using (var db = new TeklifDbContext())
            {
                var personeller = string.IsNullOrWhiteSpace(arama)
                    ? db.Personeller.ToList()
                    : db.Personeller
                          .Where(f => f.AdSoyad.Contains(arama) || f.Telefon.Contains(arama))
                          .ToList();

                dataGridPersonel.ItemsSource = personeller;
            }
        }

        private void txtArama_TextChanged(object sender, TextChangedEventArgs e)
        {
            PersonelListele(txtArama.Text.Trim());
        }

        private void BtnDetay_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var secilenPersonel = button?.DataContext as PersonelData;

            if (secilenPersonel != null)
            {
                var detayPencere = new PersonelDetayWindow(secilenPersonel);
                bool? result = detayPencere.ShowDialog();

                if (result == true)
                {
                    PersonelListele(txtArama.Text.Trim());  // sadece güncelleme yapıldıysa listeyi yenile
                }
            }
        }
    }
}

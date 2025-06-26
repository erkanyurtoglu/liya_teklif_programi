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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace teklif_programi.view
{
    /// <summary>
    /// Interaction logic for PersonelEkle.xaml
    /// </summary>
    public partial class PersonelEkle : UserControl
    {
        public TeklifDbContext _db = new TeklifDbContext();

        public PersonelEkle()
        {
            InitializeComponent();
        }

        private void Kaydet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAdSoyad.Text) ||
                string.IsNullOrWhiteSpace(txtPozisyon.Text) ||
                string.IsNullOrWhiteSpace(txtTelefon.Text) ||
                string.IsNullOrWhiteSpace(txtSifre.Text))
            {
                MessageBox.Show("Lütfen tüm alanları doldurunuz.", "Eksik Bilgi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var personel = new PersonelData
            {
                AdSoyad = txtAdSoyad.Text,
                Pozisyon = txtPozisyon.Text,
                Telefon = txtTelefon.Text,
                PersonelSifre = txtSifre.Text,
            };

            _db.Personeller.Add(personel);
            _db.SaveChanges();

            MessageBox.Show("Personel başarıyla eklendi.", "Kayıt Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);

            txtAdSoyad.Clear();
            txtPozisyon.Clear();
            txtTelefon.Clear();
            txtSifre.Clear();
        }
    }
}

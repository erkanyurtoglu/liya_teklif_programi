using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    /// <summary>
    /// TeklifVer.xaml kullanıcı kontrolü.
    /// Kullanıcı teklif oluştururken fiyat girişlerini, sözleşme açma gibi işlemleri yönetir.
    /// </summary>
    public partial class TeklifVer : UserControl
    {
        // Constructor: UserControl yüklendiğinde çalışır
        public TeklifVer()
        {
            InitializeComponent(); // XAML tarafındaki bileşenleri başlatır
            DataContext = new TeklifVerViewModel(); // ViewModel’i arayüze bağlar
        }

        /// <summary>
        /// Sadece sayı ve ondalık nokta (.) girişine izin verir.
        /// PreviewTextInput olayı ile bağlanır.
        /// </summary>
        private void OnlyAllowNumbers(object sender, TextCompositionEventArgs e)
        {
            // Girilen karakter sayı değilse ve "." değilse giriş engellenir
            e.Handled = !decimal.TryParse(e.Text, out _) && e.Text != ".";
        }

        /// <summary>
        /// Girilen değeri TextBox’un mevcut metniyle birleştirip,
        /// bunun geçerli bir decimal sayı olup olmadığını kontrol eder.
        /// Örn: "12" yazılıyken kullanıcı "3" girerse "123" olarak kontrol edilir.
        /// </summary>
        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Kullanıcının mevcut metnine yeni girilen karakteri ekle
                string newText = textBox.Text + e.Text;

                // Yeni metin geçerli bir decimal değilse giriş engellenir
                e.Handled = !decimal.TryParse(newText, out _);
            }
        }

        /// <summary>
        /// "Satış Sözleşmesi" butonuna tıklandığında çalışır.
        /// TeklifVerViewModel’den mevcut sözleşme verilerini alır
        /// ve SatisSozlesmesiWindow penceresini modal olarak açar.
        /// </summary>
        private void SatisSozlesmesi_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Mevcut DataContext’i TeklifVerViewModel tipine dönüştür
            var viewModel = (TeklifVerViewModel)DataContext;

            // Satış sözleşmesi penceresini oluştur, ViewModel’i aktar
            var satisSozlesmesiWindow = new SatisSozlesmesiWindow(viewModel);

            // Pencereyi modal (diğer işlemleri engelleyerek) aç
            satisSozlesmesiWindow.ShowDialog();
        }
    }
}

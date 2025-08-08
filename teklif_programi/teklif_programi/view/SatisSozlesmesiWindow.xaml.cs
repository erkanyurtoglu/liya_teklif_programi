using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    /// <summary>
    /// SatisSozlesmesiWindow.xaml arayüzünün mantığını içeren sınıf.
    /// Bu pencere, satış sözleşmesinin görüntülenmesini, düzenlenmesini
    /// ve varsayılan metne sıfırlanmasını sağlar.
    /// </summary>
    public partial class SatisSozlesmesiWindow : Window
    {
        // Satış sözleşmesi verilerini yöneten ViewModel
        private readonly SatisSozlesmesiViewModel _viewModel;

        // Teklif verme sürecindeki ViewModel (buradan sözleşme metni alınır / buraya kaydedilir)
        private readonly TeklifVerViewModel _teklifVerViewModel;

        // Pencere açıldığında saklanan varsayılan sözleşme metni (sıfırlama işlemi için)
        private readonly string _varsayilanSozlesmeMetni;

        /// <summary>
        /// Pencere oluşturulurken TeklifVerViewModel parametresi alır.
        /// Bu sayede sözleşme metni teklif süreciyle entegre olur.
        /// </summary>
        public SatisSozlesmesiWindow(TeklifVerViewModel teklifVerViewModel)
        {
            InitializeComponent(); // XAML bileşenlerini başlatır

            _viewModel = new SatisSozlesmesiViewModel();
            _teklifVerViewModel = teklifVerViewModel;
            DataContext = _viewModel; // Binding için ViewModel'i bağlar

            // Varsayılan metni sakla (sıfırlama butonu için)
            _varsayilanSozlesmeMetni = _viewModel.SozlesmeMetni;

            // Eğer TeklifVerViewModel’de önceden bir sözleşme metni varsa onu yükle
            if (!string.IsNullOrWhiteSpace(_teklifVerViewModel.SatisSozlesmesiMetni))
            {
                _viewModel.SozlesmeMetni = _teklifVerViewModel.SatisSozlesmesiMetni;
            }

            // Metni RichTextBox'a satır satır yükle
            LoadSozlesmeMetniToRichTextBox();
        }

        /// <summary>
        /// ViewModel’deki sözleşme metnini RichTextBox’a satır satır ekler.
        /// Her satır bir Paragraph olarak eklenir ve girinti/formatlama yapılır.
        /// </summary>
        private void LoadSozlesmeMetniToRichTextBox()
        {
            // Önce mevcut içerik temizlenir
            SatisSozlesmesiBox.Document.Blocks.Clear();

            // Metin satırlara ayrılır (boş satırlar atılır)
            string[] lines = _viewModel.SozlesmeMetni.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries
            );

            foreach (string line in lines)
            {
                // Her satır bir Paragraph olarak oluşturulur
                Paragraph paragraph = new Paragraph(new Run(line.Trim()))
                {
                    Margin = new Thickness(0, 5, 0, 5), // Satırlar arası boşluk
                    TextAlignment = TextAlignment.Left // Metin sola yaslı
                };

                // Eğer satır numaralı madde ise girinti uygula
                if (line.Trim().StartsWith("1.") || line.Trim().StartsWith("2.") || line.Trim().StartsWith("3.") ||
                    line.Trim().StartsWith("4.") || line.Trim().StartsWith("5.") || line.Trim().StartsWith("6.") ||
                    line.Trim().StartsWith("7.") || line.Trim().StartsWith("8."))
                {
                    paragraph.TextIndent = 20; // Numaralı maddeler için girinti
                }
                // Eğer satır "-" ile başlıyorsa (alt madde) daha fazla girinti uygula
                else if (line.Trim().StartsWith("-"))
                {
                    paragraph.TextIndent = 40;
                }

                // Paragraph’ı RichTextBox’a ekle
                SatisSozlesmesiBox.Document.Blocks.Add(paragraph);
            }
        }

        /// <summary>
        /// "Kapat" butonuna tıklandığında çalışır.
        /// Mevcut RichTextBox içeriğini alır ve ViewModel’e kaydeder,
        /// ardından pencereyi kapatır.
        /// </summary>
        private void Kapat_Click(object sender, RoutedEventArgs e)
        {
            // RichTextBox’taki tüm metni al
            TextRange textRange = new TextRange(
                SatisSozlesmesiBox.Document.ContentStart,
                SatisSozlesmesiBox.Document.ContentEnd
            );

            // ViewModel’deki sözleşme metnini güncelle
            _viewModel.SozlesmeMetni = textRange.Text.Trim();

            // Teklif süreci ViewModel’ine metni aktar
            _teklifVerViewModel.SatisSozlesmesiMetni = _viewModel.SozlesmeMetni;

            // Pencereyi kapat
            Close();
        }

        /// <summary>
        /// "Sıfırla" butonuna tıklandığında çalışır.
        /// Sözleşme metnini varsayılan haline getirir ve yeniden yükler.
        /// </summary>
        private void Sifirla_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SozlesmeMetni = _varsayilanSozlesmeMetni; // Varsayılan metin yüklenir
            LoadSozlesmeMetniToRichTextBox(); // RichTextBox’a yeniden aktarılır
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    public partial class SatisSozlesmesiWindow : Window
    {
        private readonly SatisSozlesmesiViewModel _viewModel;
        private readonly TeklifVerViewModel _teklifVerViewModel;
        private readonly string _varsayilanSozlesmeMetni;

        public SatisSozlesmesiWindow(TeklifVerViewModel teklifVerViewModel)
        {
            InitializeComponent();
            _viewModel = new SatisSozlesmesiViewModel();
            _teklifVerViewModel = teklifVerViewModel;
            DataContext = _viewModel;

            // Varsayılan metni sakla
            _varsayilanSozlesmeMetni = _viewModel.SozlesmeMetni;

            // Önce TeklifVerViewModel.SatisSozlesmesiMetni’ni kontrol et
            if (!string.IsNullOrWhiteSpace(_teklifVerViewModel.SatisSozlesmesiMetni))
            {
                _viewModel.SozlesmeMetni = _teklifVerViewModel.SatisSozlesmesiMetni;
            }

            // RichTextBox’a metni satır satır yükle
            LoadSozlesmeMetniToRichTextBox();
        }

        private void LoadSozlesmeMetniToRichTextBox()
        {
            SatisSozlesmesiBox.Document.Blocks.Clear();

            // Metni satır satır ayır
            string[] lines = _viewModel.SozlesmeMetni.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                Paragraph paragraph = new Paragraph(new Run(line.Trim()))
                {
                    Margin = new Thickness(0, 5, 0, 5), // Tutarlı satır aralığı
                    TextAlignment = TextAlignment.Left // Sol hizalama
                };

                // Numaralı maddeler için girinti ayarı (isteğe bağlı)
                if (line.Trim().StartsWith("1.") || line.Trim().StartsWith("2.") || line.Trim().StartsWith("3.") ||
                    line.Trim().StartsWith("4.") || line.Trim().StartsWith("5.") || line.Trim().StartsWith("6.") ||
                    line.Trim().StartsWith("7.") || line.Trim().StartsWith("8."))
                {
                    paragraph.TextIndent = 20; // Numaralandırılmış maddeler için girinti
                }
                else if (line.Trim().StartsWith("-"))
                {
                    paragraph.TextIndent = 40; // Alt maddeler için daha fazla girinti
                }

                SatisSozlesmesiBox.Document.Blocks.Add(paragraph);
            }
        }

        private void Kapat_Click(object sender, RoutedEventArgs e)
        {
            // RichTextBox’tan metni al ve ViewModel’e kaydet
            TextRange textRange = new TextRange(SatisSozlesmesiBox.Document.ContentStart, SatisSozlesmesiBox.Document.ContentEnd);
            _viewModel.SozlesmeMetni = textRange.Text.Trim();

            // TeklifVerViewModel’e metni aktar
            _teklifVerViewModel.SatisSozlesmesiMetni = _viewModel.SozlesmeMetni;

            Close();
        }

        private void Sifirla_Click(object sender, RoutedEventArgs e)
        {
            // Varsayılan metne geri dön
            _viewModel.SozlesmeMetni = _varsayilanSozlesmeMetni;
            LoadSozlesmeMetniToRichTextBox(); // Metni yeniden yükle
        }


    }
}
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

        public SatisSozlesmesiWindow(TeklifVerViewModel teklifVerViewModel)
        {
            InitializeComponent();
            _viewModel = new SatisSozlesmesiViewModel();
            _teklifVerViewModel = teklifVerViewModel;
            DataContext = _viewModel;

            // RichTextBox’a varsayılan metni yükle
            SatisSozlesmesiBox.Document.Blocks.Clear();
            SatisSozlesmesiBox.Document.Blocks.Add(new Paragraph(new Run(_viewModel.SozlesmeMetni)));
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
    }
}
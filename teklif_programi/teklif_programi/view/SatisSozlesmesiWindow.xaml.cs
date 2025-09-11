using System;
using System.Windows;
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
        private readonly TeklifVerViewModel? _teklifVerViewModel;
        // Teklif detay sürecindeki ViewModel
        private readonly TeklifDetayViewModel? _teklifDetayViewModel;

        // Pencere açıldığında saklanan varsayılan sözleşme metni (sıfırlama işlemi için)
        private readonly string _varsayilanSozlesmeMetni;

        public SatisSozlesmesiWindow(TeklifVerViewModel teklifVerViewModel)
        {
            InitializeComponent();

            _viewModel = new SatisSozlesmesiViewModel();
            _teklifVerViewModel = teklifVerViewModel;
            DataContext = _viewModel; // Binding için ViewModel'i bağlar

            if (!string.IsNullOrWhiteSpace(_teklifVerViewModel.SatisSozlesmesiMetni))
            {
                _viewModel.SozlesmeMetni = _teklifVerViewModel.SatisSozlesmesiMetni;
            }

            _varsayilanSozlesmeMetni = _viewModel.SozlesmeMetni;

            LoadSozlesmeMetniToRichTextBox();
        }

        public SatisSozlesmesiWindow(TeklifDetayViewModel teklifDetayViewModel)
        {
            InitializeComponent();

            _viewModel = new SatisSozlesmesiViewModel();
            _teklifDetayViewModel = teklifDetayViewModel;
            DataContext = _viewModel;

            if (!string.IsNullOrWhiteSpace(_teklifDetayViewModel.SatisSozlesmesiMetni))
            {
                _viewModel.SozlesmeMetni = _teklifDetayViewModel.SatisSozlesmesiMetni;
            }

            _varsayilanSozlesmeMetni = _viewModel.SozlesmeMetni;

            LoadSozlesmeMetniToRichTextBox();
        }

        private void LoadSozlesmeMetniToRichTextBox()
        {
            TextRange textRange = new TextRange(
                SatisSozlesmesiBox.Document.ContentStart,
                SatisSozlesmesiBox.Document.ContentEnd);
            textRange.Text = _viewModel.SozlesmeMetni;
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

             SatisSozlesmesiBox.Document.ContentEnd);
            _viewModel.SozlesmeMetni = textRange.Text;

            // Teklif süreci ViewModel’ine metni aktar
            if (_teklifVerViewModel != null)
                _teklifVerViewModel.SatisSozlesmesiMetni = _viewModel.SozlesmeMetni;
            else if (_teklifDetayViewModel != null)
                _teklifDetayViewModel.SatisSozlesmesiMetni = _viewModel.SozlesmeMetni;

            // Pencereyi kapat
            Close();
        }

        /// <summary>
        /// "Sıfırla" butonuna tıklandığında çalışır.
        /// Sözleşme metnini varsayılan haline getirir ve yeniden yükler.
        /// </summary>
        private void Sifirla_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SozlesmeMetni = _varsayilanSozlesmeMetni;
            LoadSozlesmeMetniToRichTextBox();
        }
    }
}
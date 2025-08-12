using System;
using System.IO;
using System.Text;
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

        public SatisSozlesmesiWindow(TeklifVerViewModel teklifVerViewModel)
        {
            InitializeComponent();

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

        private void LoadSozlesmeMetniToRichTextBox()
        {
            // Önce mevcut içerik temizlenir
            SatisSozlesmesiBox.Document.Blocks.Clear();

            if (string.IsNullOrWhiteSpace(_viewModel.SozlesmeMetni))
                return;
            {
                try
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(_viewModel.SozlesmeMetni);
                    using MemoryStream stream = new MemoryStream(bytes);
                    TextRange range = new TextRange(
                        SatisSozlesmesiBox.Document.ContentStart,
                        SatisSozlesmesiBox.Document.ContentEnd);
                    range.Load(stream, DataFormats.Xaml);
                }

                catch 
                {
                    string[] lines = _viewModel.SozlesmeMetni.Split(
                        new[] { Environment.NewLine },
                        StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        Paragraph paragraph = new Paragraph(new Run(line.Trim()))
                        {
                            Margin = new Thickness(0, 5, 0, 5),
                            TextAlignment = TextAlignment.Left
                        };

                        if (line.Trim().StartsWith("1.") || line.Trim().StartsWith("2.") || line.Trim().StartsWith("3.") ||
                            line.Trim().StartsWith("4.") || line.Trim().StartsWith("5.") || line.Trim().StartsWith("6.") ||
                            line.Trim().StartsWith("7.") || line.Trim().StartsWith("8."))
                        {
                            paragraph.TextIndent = 20;
                        }
                        else if (line.Trim().StartsWith("-"))
                        {
                            paragraph.TextIndent = 40;
                        }

                        SatisSozlesmesiBox.Document.Blocks.Add(paragraph);
                    }
                }
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
                SatisSozlesmesiBox.Document.ContentEnd); ;

            using MemoryStream stream = new MemoryStream();
            textRange.Save(stream, DataFormats.Xaml);
            _viewModel.SozlesmeMetni = Encoding.UTF8.GetString(stream.ToArray());

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
            _viewModel.SozlesmeMetni = _varsayilanSozlesmeMetni;
            LoadSozlesmeMetniToRichTextBox();
        }
    }
}

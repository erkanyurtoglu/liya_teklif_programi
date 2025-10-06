using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using teklif_programi.Models;
using teklif_programi.ViewModels;

namespace teklif_programi.view
{
    public partial class TeklifDetayWindow : Window
    {
        public TeklifDetayWindow(Teklif teklif)
        {
            InitializeComponent();
            DataContext = new TeklifDetayViewModel(teklif);
        }

        // --- Yardımcılar (TeklifVer ile aynı mantık) ---

        // Seçimi "incoming" ile değiştirip caret'i doğru yere koyar
        private static void ReplaceSelection(TextBox tb, string incoming)
        {
            int start = tb.SelectionStart;
            int length = tb.SelectionLength;
            tb.Text = tb.Text.Remove(start, length).Insert(start, incoming);
            tb.CaretIndex = start + incoming.Length;
        }

        // Mevcut metne incoming eklenince oluşacak metni önceden kur
        private static string BuildProposed(TextBox tb, string incoming)
        {
            int start = tb.SelectionStart;
            int length = tb.SelectionLength;
            return tb.Text.Remove(start, length).Insert(start, incoming);
        }

        private static CultureInfo GetCulture(TextBox? tb)
        {
            if (tb?.Language is XmlLanguage language)
            {
                try
                {
                    return language.GetEquivalentCulture();
                }
                catch (InvalidOperationException)
                {
                    // xml:lang değeri geçersizse varsayılan kültüre dön.
                }
            }

            return CultureInfo.CurrentCulture;
        }

        private static bool IsValidPartialDecimal(string text, CultureInfo culture)
        {
            if (text is null) return false;

            var sep = culture.NumberFormat.NumberDecimalSeparator;

            if (text.Length == 0) return true;   // boşken yazmaya izin
            if (text == sep) return true;        // sadece ayıraç ("," veya ".")

            // "12," gibi sonda ayıraca izin ver (baş kısmı tamamen rakam olmalı)
            if (text.EndsWith(sep))
            {
                var head = text[..^sep.Length];
                foreach (char c in head)
                    if (!char.IsDigit(c)) return false;
                return true;
            }

            // Normal kontrol (mevcut kültürle)
            return decimal.TryParse(text, NumberStyles.Number, culture, out _);
        }

        // --- Ondalık giriş: PreviewTextInput ('.' ve ',' kültür ayıracına çevrilir) ---
        private void DecimalInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox tb) return;

            CultureInfo culture = GetCulture(tb);
            string sep = culture.NumberFormat.NumberDecimalSeparator;
            string incoming = (e.Text == "." || e.Text == ",") ? sep : e.Text;

            string proposed = BuildProposed(tb, incoming);
            bool valid = IsValidPartialDecimal(proposed, culture);

            bool needsManual = incoming != e.Text; // tuşlanan karakteri sep'e çevirdik mi?

            if (!valid)
            {
                e.Handled = true; // engelle
                return;
            }

            if (needsManual)
            {
                e.Handled = true;       // normal akışı iptal et
                ReplaceSelection(tb, incoming); // kendimiz yazalım
            }
            else
            {
                e.Handled = false;      // normal akış
            }
        }

        // --- Ondalık giriş: Pasting (yapıştırmayı normalize et) ---
        private void DecimalInput_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not TextBox tb) return;

            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                CultureInfo culture = GetCulture(tb);
                string sep = culture.NumberFormat.NumberDecimalSeparator;
                string pasted = (string)e.DataObject.GetData(typeof(string));
                string normalized = pasted.Replace(".", sep).Replace(",", sep);

                string proposed = BuildProposed(tb, normalized);

                if (IsValidPartialDecimal(proposed, culture))
                {
                    e.CancelCommand();           // kendi yazımımız
                    ReplaceSelection(tb, normalized);
                }
                else
                {
                    e.CancelCommand();           // geçersiz -> iptal
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        // --- Ondalık giriş: TextChanged (stabil olduğunda kaynağı anında güncelle) ---
        private void DecimalInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox tb) return;

            CultureInfo culture = GetCulture(tb);
            string sep = culture.NumberFormat.NumberDecimalSeparator;
            string s = (tb.Text ?? string.Empty).Replace(".", sep).Replace(",", sep).Trim();

            // Geçici durumlarda (boş, tek ayıraç, sonda ayıraç) kaynağa yazma
            if (string.IsNullOrEmpty(s) || s == sep || s.EndsWith(sep))
                return;

            if (decimal.TryParse(s, NumberStyles.Number, culture, out _))
            {
                BindingExpression be = tb.GetBindingExpression(TextBox.TextProperty);
                be?.UpdateSource(); // ViewModel'i anında güncelle
            }
        }

        // --- Diğer ---
        private void SatisSozlesmesi_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is TeklifDetayViewModel vm)
            {
                var window = new SatisSozlesmesiWindow(vm);
                window.ShowDialog();
            }
        }

        // XAML: TextChanged="TextBox_TextChanged" için eksik handler
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Gerekli değil ama binding'i güvenceye almak istersen:
            if (sender is TextBox tb)
            {
                var be = tb.GetBindingExpression(TextBox.TextProperty);
                be?.UpdateSource();
            }
        }

    }
}

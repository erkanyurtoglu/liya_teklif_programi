using System;
using System.Globalization;
using System.Windows;
using teklif_programi.Models;

namespace teklif_programi.view
{
    public partial class ManuelUrunEkleWindow : Window
    {
        public Urun? ManualUrun { get; private set; }

        public ManuelUrunEkleWindow()
        {
            InitializeComponent();
        }

        private void Kaydet_Click(object sender, RoutedEventArgs e)
        {
            var urunKodu = txtUrunKodu.Text.Trim();
            var kategori = txtKategori.Text.Trim();
            var aciklamaTr = txtAciklamaTr.Text.Trim();
            var aciklamaEn = txtAciklamaEn.Text.Trim();

            if (string.IsNullOrWhiteSpace(urunKodu))
            {
                MessageBox.Show("Lütfen ürün kodunu giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUrunKodu.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(aciklamaTr))
            {
                MessageBox.Show("Lütfen ürün açıklamasını giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAciklamaTr.Focus();
                return;
            }

            if (!TryParseDecimal(txtFiyatTl.Text, out var fiyatTl))
            {
                MessageBox.Show("Lütfen geçerli bir TL fiyatı giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFiyatTl.Focus();
                return;
            }

            if (!TryParseDecimal(txtFiyatUsd.Text, out var fiyatUsd))
            {
                MessageBox.Show("Lütfen geçerli bir USD fiyatı giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFiyatUsd.Focus();
                return;
            }

            if (!TryParseDecimal(txtFiyatEur.Text, out var fiyatEur))
            {
                MessageBox.Show("Lütfen geçerli bir EUR fiyatı giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFiyatEur.Focus();
                return;
            }

            if (!TryParseDecimal(txtMaliyetTl.Text, out var maliyetTl))
            {
                MessageBox.Show("Lütfen geçerli bir TL maliyet giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtMaliyetTl.Focus();
                return;
            }

            ManualUrun = new Urun
            {
                UrunKodu = urunKodu,
                Kategori = kategori,
                UrunAciklamasi = aciklamaTr,
                UrunAciklamasiEn = aciklamaEn,
                BirimFiyat = fiyatTl,
                FiyatTL = fiyatTl,
                FiyatUSD = fiyatUsd,
                FiyatEUR = fiyatEur,
                MaliyetFiyati = maliyetTl,
                EklenmeTarihi = DateTime.Now
            };

            DialogResult = true;
            Close();
        }

        private void Iptal_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private static bool TryParseDecimal(string? input, out decimal value)
        {
            value = 0;
            var text = (input ?? string.Empty)
                .Replace("₺", string.Empty)
                .Replace("$", string.Empty)
                .Replace("€", string.Empty)
                .Replace("TL", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("USD", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("EUR", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace(" ", string.Empty)
                .Trim();

            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
                return true;

            var normalized = text.Replace(".", string.Empty).Replace(",", ".");
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }
    }
}
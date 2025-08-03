using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using teklif_programi.Data;
using teklif_programi.Models;

namespace teklif_programi.view
{
    /// <summary>
    /// Interaction logic for TeklifVer.xaml
    /// </summary>
    public partial class TeklifVer : UserControl
    {
        private TeklifDbContext _db = new TeklifDbContext();
        private List<UrunData> secilenUrunler = new List<UrunData>();

        public TeklifVer()
        {
            InitializeComponent();
            dataGridTeklifUrunler.ItemsSource = secilenUrunler;
        }

        private void BtnFirmaBilgisiGetir_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtFirmaKodu.Text.Trim(), out int firmaKodu))
            {
                MessageBox.Show("Lütfen geçerli bir Firma Kodu giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var firma = _db.Firmalar.FirstOrDefault(f => f.FirmaKoduID == firmaKodu);
            if (firma != null)
            {
                lblFirmaAdi.Text = firma.FirmaAdi;
            }
            else
            {
                MessageBox.Show("Firma bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                lblFirmaAdi.Text = "-";
            }
        }

        private void BtnUrunBilgisiGetir_Click(object sender, RoutedEventArgs e)
        {
            string urunKodu = txtUrunKodu.Text.Trim();

            if (string.IsNullOrEmpty(urunKodu))
            {
                MessageBox.Show("Lütfen geçerli bir Ürün Kodu giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var urun = _db.Urunler.FirstOrDefault(u => u.UrunKoduID == urunKodu);

            if (urun != null)
            {
                lblUrunAdi.Text = urun.Aciklama;
            }
            else
            {
                MessageBox.Show("Ürün bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                lblUrunAdi.Text = "-";
            }
        }

        private void BtnUrunEkle_Click(object sender, RoutedEventArgs e)
        {
            string urunKodu = txtUrunKodu.Text.Trim();

            if (string.IsNullOrEmpty(urunKodu))
            {
                MessageBox.Show("Lütfen ürün kodunu giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var urun = _db.Urunler.FirstOrDefault(u => u.UrunKoduID == urunKodu);

            if (urun == null)
            {
                MessageBox.Show("Ürün bulunamadı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (secilenUrunler.Any(x => x.UrunKoduID == urun.UrunKoduID))
            {
                MessageBox.Show("Bu ürün zaten listede var.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UrunData yeniUrun = new UrunData
            {
                UrunKoduID = urun.UrunKoduID,
                Kategori = urun.Kategori,
                Aciklama = urun.Aciklama,
                Adet = 1,
                BirimSatisFiyati = urun.BirimSatisFiyati,
                YurticiMaliyet = urun.YurticiMaliyet,
            };

            secilenUrunler.Add(yeniUrun);

            dataGridTeklifUrunler.ItemsSource = null;
            dataGridTeklifUrunler.ItemsSource = secilenUrunler;
        }

        private void BtnTeklifOlusturVePdfIndir_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirmaKodu.Text) || lblFirmaAdi.Text == "-" || secilenUrunler.Count == 0)
            {
                MessageBox.Show("Lütfen geçerli bir firma seçin ve en az bir ürün ekleyin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int firmaKodu = int.Parse(txtFirmaKodu.Text);
            int personelKodu = 1;

            var yeniTeklif = new Teklif
            {
                FirmaKoduID = firmaKodu,
                PersonelKoduID = personelKodu,
                TeklifTarihi = DateTime.Now,
                ToplamTutar = ToplamFiyatHesapla(secilenUrunler)
            };

            _db.Teklifler.Add(yeniTeklif);
            _db.SaveChanges();

            foreach (var urun in secilenUrunler)
            {
                var detay = new TeklifDetay
                {
                    TeklifNoID = yeniTeklif.TeklifNoID,
                    UrunKoduID = urun.UrunKoduID,
                    Adet = urun.Adet,
                    BirimFiyat = urun.BirimSatisFiyati,
                    ToplamFiyat = urun.BirimSatisFiyati * urun.Adet
                };
                _db.TeklifDetaylari.Add(detay);
            }

            _db.SaveChanges();

            string templatePath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\LiyaTeklifBelgesi.pdf";
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF dosyası (*.pdf)|*.pdf",
                FileName = $"Teklif_{lblFirmaAdi.Text}_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    PdfReader reader = new PdfReader(templatePath);
                    PdfStamper stamper = new PdfStamper(reader, new FileStream(saveFileDialog.FileName, FileMode.Create));

                    string fontPath = @"C:\Windows\Fonts\arial.ttf";
                    if (!File.Exists(fontPath))
                    {
                        MessageBox.Show("Arial font dosyası bulunamadı: " + fontPath, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    var firmaFont = new Font(baseFont, 10, Font.NORMAL, new BaseColor(128, 128, 128));
                    var tableHeaderFont = new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK);
                    var tableBodyFont = new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK);

                    // Sayfa 2 için canvas ve içerik başlat
                    int currentPage = 2;
                    PdfContentByte canvas = stamper.GetOverContent(currentPage);
                    ColumnText ct = new ColumnText(canvas);

                    // Firma Bilgileri ve başlık
                    Phrase firmaBilgileri = new Phrase();

                    // Kalın & siyah başlıklar, normal gri içerik
                    firmaBilgileri.Add(new Chunk("Firma Kodu: ", new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK)));
                    firmaBilgileri.Add(new Chunk(txtFirmaKodu.Text + "\n", new Font(baseFont, 10, Font.NORMAL, new BaseColor(80, 80, 80))));

                    firmaBilgileri.Add(new Chunk("Firma Ad: ", new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK)));
                    firmaBilgileri.Add(new Chunk(lblFirmaAdi.Text + "\n", new Font(baseFont, 10, Font.NORMAL, new BaseColor(80, 80, 80))));

                    firmaBilgileri.Add(new Chunk("Tarih: ", new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK)));
                    firmaBilgileri.Add(new Chunk(DateTime.Now.ToString("dd.MM.yyyy"), new Font(baseFont, 10, Font.NORMAL, new BaseColor(80, 80, 80))));

                    ct.SetSimpleColumn(firmaBilgileri, 50, 650, 550, 600, 15, Element.ALIGN_LEFT);
                    ct.Go();

                    Phrase urunBaslik = new Phrase("Teklif Edilen Ürünler", new Font(baseFont, 12, Font.BOLD, BaseColor.BLACK));
                    ct.SetSimpleColumn(urunBaslik, 50, 580, 550, 560, 15, Element.ALIGN_LEFT);
                    ct.Go();

                    // Tablo oluştur
                    PdfPTable table = new PdfPTable(6);
                    table.TotalWidth = 500f;
                    table.LockedWidth = true;
                    float[] widths = new float[] { 2f, 2f, 3f, 1f, 2f, 2f };
                    table.SetWidths(widths);

                    AddCellToHeader(table, "Ürün Kodu", tableHeaderFont, new BaseColor(240, 240, 240));
                    AddCellToHeader(table, "Kategori", tableHeaderFont, new BaseColor(240, 240, 240));
                    AddCellToHeader(table, "Açıklama", tableHeaderFont, new BaseColor(240, 240, 240));
                    AddCellToHeader(table, "Adet", tableHeaderFont, new BaseColor(240, 240, 240));
                    AddCellToHeader(table, "2025 Birim Satış Fiyatı", tableHeaderFont, new BaseColor(240, 240, 240));
                    AddCellToHeader(table, "Toplam Fiyat", tableHeaderFont, new BaseColor(240, 240, 240));

                    int rowCount = 0;
                    foreach (var urun in secilenUrunler)
                    {
                        BaseColor rowColor = rowCount % 2 == 0 ? BaseColor.WHITE : new BaseColor(240, 240, 240);
                        AddCellToBody(table, urun.UrunKoduID, tableBodyFont, rowColor);
                        AddCellToBody(table, urun.Kategori, tableBodyFont, rowColor);
                        AddCellToBody(table, urun.Aciklama, tableBodyFont, rowColor);
                        AddCellToBody(table, urun.Adet.ToString(), tableBodyFont, rowColor);
                        AddCellToBody(table, urun.BirimSatisFiyati.ToString("C2"), tableBodyFont, rowColor);
                        rowCount++;
                    }

                    ct = new ColumnText(canvas);
                    ct.AddElement(table);
                    ct.SetSimpleColumn(50, 540, 550, 200, 15, Element.ALIGN_LEFT);
                    int status = ct.Go();

                    // Sayfa taşma kontrolü
                    while (ColumnText.HasMoreText(status))
                    {
                        currentPage++;
                        stamper.InsertPage(currentPage, PageSize.A4);
                        PdfContentByte newCanvas = stamper.GetOverContent(currentPage);

                        PdfImportedPage templatePage = stamper.GetImportedPage(reader, 2); // Şablon tekrar kullan
                        newCanvas.AddTemplate(templatePage, 0, 0);

                        ct = new ColumnText(newCanvas);
                        ct.SetSimpleColumn(50, 800, 550, 200, 15, Element.ALIGN_LEFT);
                        status = ct.Go();

                        canvas = newCanvas; // Son sayfayı sakla
                    }

                    // Sadece SON sayfaya toplam fiyat yaz
                    canvas.BeginText(); 
                    canvas.SetFontAndSize(baseFont, 12);
                    canvas.SetColorFill(BaseColor.BLACK);
                    canvas.ShowTextAligned(
                        Element.ALIGN_RIGHT,
                        $"Toplam Teklif Tutarı: {ToplamFiyatHesapla(secilenUrunler).ToString("C2")}",
                        550f, 60f,
                        0
                    );
                    canvas.EndText();

                    stamper.Close();
                    reader.Close();

                    MessageBox.Show("Teklif PDF dosyası başarıyla oluşturuldu.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("PDF oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void AddCellToHeader(PdfPTable table, string text, iTextSharp.text.Font font, BaseColor backgroundColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = backgroundColor;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 5;
            cell.BorderColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
        }

        private void AddCellToBody(PdfPTable table, string text, iTextSharp.text.Font font, BaseColor backgroundColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.BackgroundColor = backgroundColor;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 5;
            cell.BorderColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
        }

        private decimal ToplamFiyatHesapla(List<UrunData> secilenUrunler)
        {
            return secilenUrunler.Sum(u => u.BirimSatisFiyati * u.Adet);
        }

        private void BtnAdetArttir_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var urun = button?.DataContext as UrunData;
            if (urun != null)
            {
                urun.Adet++;
                dataGridTeklifUrunler.Items.Refresh();
            }
        }

        private void BtnAdetAzalt_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var urun = button?.DataContext as UrunData;
            if (urun != null && urun.Adet > 1)  // adet en az 1 olmalı
            {
                urun.Adet--;
                dataGridTeklifUrunler.Items.Refresh();
            }
        }

        private void BtnUrunSil_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var urun = button?.DataContext as UrunData;
            if (urun != null)
            {
                secilenUrunler.Remove(urun);
                dataGridTeklifUrunler.Items.Refresh();
            }
        }

        private void AdetTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Sadece sayı girişi kabul et
            e.Handled = !int.TryParse(e.Text, out _);
        }


    }
}
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using teklif_programi.Data;
using teklif_programi.Models;
using Microsoft.Win32;


namespace teklif_programi.view
{
    public partial class TeklifDetayWindow : Window
    {
        private Teklif _teklif;
        private TeklifDbContext _db = new TeklifDbContext();
        private ObservableCollection<TeklifDetay> _detaylar;

        public TeklifDetayWindow(Teklif teklif)
        {
            InitializeComponent();
            _teklif = teklif;
            _detaylar = new ObservableCollection<TeklifDetay>(_teklif.TeklifDetaylari);

            Title = $"Teklif No: {_teklif.TeklifNoID} - Detaylar";
            lblFirma.Content = _teklif.Firma?.FirmaAdi ?? "Bilinmiyor";
            lblTarih.Content = _teklif.TeklifTarihi.ToString("dd.MM.yyyy");
            lblToplamTutar.Content = _teklif.ToplamTutar.ToString("C2");
            dataGridDetaylar.ItemsSource = _detaylar;
        }

        private void BtnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            var pwdDialog = new PasswordDialog();
            pwdDialog.Owner = this;
            if (pwdDialog.ShowDialog() == true && pwdDialog.EnteredPassword == "Liya2015")
            {
                foreach (var detay in _detaylar)
                {
                    if (int.TryParse(detay.Adet.ToString(), out int adet) && decimal.TryParse(detay.BirimFiyat.ToString().Replace("₺", "").Trim(), out decimal birimFiyat))
                    {
                        detay.ToplamFiyat = birimFiyat * adet;
                    }
                }
                _teklif.ToplamTutar = _detaylar.Sum(d => d.ToplamFiyat);
                _db.Teklifler.Update(_teklif);
                foreach (var detay in _detaylar)
                {
                    _db.TeklifDetaylari.Update(detay);
                }
                _db.SaveChanges();
                lblToplamTutar.Content = _teklif.ToplamTutar.ToString("C2");
                MessageBox.Show("Teklif başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show("Şifre yanlış. Güncelleme iptal edildi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPdfIndir_Click(object sender, RoutedEventArgs e)
        {
            string templatePath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\LiyaTeklifBelgesi.pdf";

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF dosyası (*.pdf)|*.pdf",
                FileName = $"Teklif_{_teklif.Firma.FirmaAdi}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    if (!File.Exists(templatePath))
                    {
                        MessageBox.Show("PDF şablon dosyası bulunamadı: " + templatePath, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    using (PdfReader reader = new PdfReader(templatePath))
                    using (PdfStamper stamper = new PdfStamper(reader, new FileStream(saveFileDialog.FileName, FileMode.Create)))
                    {
                        string fontPath = @"C:\Windows\Fonts\arial.ttf";
                        if (!File.Exists(fontPath))
                        {
                            MessageBox.Show("Arial font dosyası bulunamadı: " + fontPath, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        var infoFont = new Font(baseFont, 10, Font.NORMAL, new BaseColor(80, 80, 80));
                        var labelFont = new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK);

                        int currentPage = 2;
                        PdfContentByte canvas = stamper.GetOverContent(currentPage);
                        ColumnText ct = new ColumnText(canvas);

                        // Firma Bilgileri
                        Phrase firmaBilgileri = new Phrase();
                        firmaBilgileri.Add(new Chunk("Firma Ad: ", labelFont));
                        firmaBilgileri.Add(new Chunk(_teklif.Firma.FirmaAdi + "\n", infoFont));
                        firmaBilgileri.Add(new Chunk("\nMobil: ", labelFont));
                        firmaBilgileri.Add(new Chunk(_teklif.Firma.Telefon ?? "Bilgi Yok" + "\n", infoFont));
                        firmaBilgileri.Add(new Chunk("\nS.N.: ", labelFont));
                        firmaBilgileri.Add(new Chunk(_teklif.Firma.ilgiliKisi ?? "Bilgi Yok" + "\n", infoFont));
                        firmaBilgileri.Add(new Chunk("\nTelefon: ", labelFont));
                        firmaBilgileri.Add(new Chunk(_teklif.Firma.ilgiliKisiTelefon ?? "Bilgi Yok" + "\n", infoFont));
                        firmaBilgileri.Add(new Chunk("\ne-posta: ", labelFont));
                        firmaBilgileri.Add(new Chunk(_teklif.Firma.Email ?? "Bilgi Yok" + "\n", infoFont));
                        ct.SetSimpleColumn(firmaBilgileri, 50, 700, 300, 620, 15, Element.ALIGN_LEFT);
                        ct.Go();

                        // Tarih ve Talep No
                        Phrase tarihTalep = new Phrase();
                        tarihTalep.Add(new Chunk("Tarih: ", labelFont));
                        tarihTalep.Add(new Chunk(_teklif.TeklifTarihi.ToString("dd.MM.yyyy"), infoFont));
                        tarihTalep.Add(new Chunk("\nTalep No: ", labelFont));
                        tarihTalep.Add(new Chunk(_teklif.TeklifNoID.ToString(), infoFont));
                        ct.SetSimpleColumn(tarihTalep, 350, 700, 550, 620, 15, Element.ALIGN_LEFT);
                        ct.Go();

                        // Ürün Başlığı
                        Phrase urunBaslik = new Phrase("Teklif Edilen Ürünler", new Font(baseFont, 12, Font.BOLD, BaseColor.BLACK));
                        ct.SetSimpleColumn(urunBaslik, 50, 570, 550, 550, 15, Element.ALIGN_LEFT);
                        ct.Go();

                        // Ürün Tablosu
                        PdfPTable table = new PdfPTable(6);
                        table.TotalWidth = 500f;
                        table.LockedWidth = true;
                        float[] widths = { 2f, 7f, 1f, 2f, 2f, 2f };
                        table.SetWidths(widths);

                        AddCellToHeader(table, "Ürün Kodu", new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK), new BaseColor(240, 240, 240));
                        AddCellToHeader(table, "Özellikler", new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK), new BaseColor(240, 240, 240));
                        AddCellToHeader(table, "Adet", new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK), new BaseColor(240, 240, 240));
                        AddCellToHeader(table, "Birim Fiyatı", new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK), new BaseColor(240, 240, 240));
                        AddCellToHeader(table, "İskontolu Birim Fiyatı", new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK), new BaseColor(240, 240, 240));
                        AddCellToHeader(table, "Toplam Fiyat", new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK), new BaseColor(240, 240, 240));

                        int rowCount = 0;
                        foreach (var detay in _detaylar)
                        {
                            BaseColor rowColor = rowCount % 2 == 0 ? BaseColor.WHITE : new BaseColor(240, 240, 240);
                            AddCellToBody(table, detay.Urun.UrunKoduID, new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK), rowColor);
                            AddCellToBody(table, detay.Urun.Aciklama, new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK), rowColor);
                            AddCellToBody(table, detay.Adet.ToString(), new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK), rowColor);
                            AddCellToBody(table, detay.BirimFiyat.ToString("C2"), new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK), rowColor);
                            AddCellToBody(table, (detay.BirimFiyat * (1 - (detay.Urun.GenelIndirim / 100m))).ToString("C2"), new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK), rowColor);
                            AddCellToBody(table, detay.ToplamFiyat.ToString("C2"), new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK), rowColor);
                            rowCount++;
                        }

                        ct = new ColumnText(canvas);
                        ct.AddElement(table);
                        ct.SetSimpleColumn(50, 530, 550, 150, 15, Element.ALIGN_LEFT);
                        int status = ct.Go();

                        while (ColumnText.HasMoreText(status))
                        {
                            currentPage++;
                            stamper.InsertPage(currentPage, PageSize.A4);
                            PdfContentByte newCanvas = stamper.GetOverContent(currentPage);
                            PdfImportedPage templatePage = stamper.GetImportedPage(reader, 2);
                            newCanvas.AddTemplate(templatePage, 0, 0);

                            ct = new ColumnText(newCanvas);
                            ct.AddElement(table);
                            ct.SetSimpleColumn(50, 800, 550, 150, 15, Element.ALIGN_LEFT);
                            status = ct.Go();

                            canvas = newCanvas;
                        }

                        // Alt Toplamlar
                        canvas.BeginText();
                        canvas.SetFontAndSize(baseFont, 12);
                        canvas.SetColorFill(BaseColor.BLACK);
                        canvas.ShowTextAligned(Element.ALIGN_RIGHT, $"Toplam: {_teklif.ToplamTutar:C2}", 550f, 120f, 0);
                        canvas.EndText();
                    }

                    MessageBox.Show("Teklif PDF dosyası başarıyla oluşturuldu.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("PDF oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_Closed(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddCellToHeader(PdfPTable table, string text, iTextSharp.text.Font font, BaseColor backgroundColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = backgroundColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 5,
                BorderColor = BaseColor.LIGHT_GRAY
            };
            table.AddCell(cell);
        }

        private void AddCellToBody(PdfPTable table, string text, iTextSharp.text.Font font, BaseColor backgroundColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = backgroundColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 5,
                BorderColor = BaseColor.LIGHT_GRAY
            };
            table.AddCell(cell);
        }
    }
}
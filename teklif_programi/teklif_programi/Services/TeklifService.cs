using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using teklif_programi.Data;
using teklif_programi.Helpers;
using teklif_programi.Models;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace teklif_programi.Services
{
    public class TeklifService
    {
        private readonly TeklifDbContext _context = new();
        private static readonly float[] FirmaColumnWidths = { 2f, 2f };
        private static readonly float[] ProductColumnWidths = { 1f, 2f, 5f, 1f, 2f, 2f, 2f };
        private static readonly float[] TotalColumnWidths = { 3f, 2f };

        private static readonly string GirisSayfaPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\girisSayfa.pdf";
        private static readonly string TeklifSayfaPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\teklifSayfa.pdf";
        private static readonly string SozlesmeSayfaPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\sozlesmeSayfa.pdf";

        public void KaydetVePdfIndir(Musteri firma,
                                     IEnumerable<TeklifUrunModel> urunler,
                                     decimal genelIndirimOrani,
                                     decimal kdvOrani,
                                     string currency,
                                     string ilgiliKisi,
                                     string ilgiliKisiTelefonu,
                                     string ilgiliKisiEposta,
                                     string sozlesmeMetni)
        {
            ArgumentNullException.ThrowIfNull(firma);
            ArgumentNullException.ThrowIfNull(urunler);
            if (!urunler.Any()) throw new ArgumentException("En az bir ürün seçilmelidir.");

            using var transaction = _context.Database.BeginTransaction();

            var teklif = new Teklif
            {
                MusteriId = firma.MusteriId,
                PersonelId = 2,
                OlusturmaTarihi = DateTime.Now,
                GenelIndirimOrani = genelIndirimOrani,
                KdvOrani = kdvOrani,
                ParaBirimi = currency,
                IlgiliKisi = ilgiliKisi,
                IlgiliKisiTelefonu = ilgiliKisiTelefonu,
                IlgiliKisiEposta = ilgiliKisiEposta
            };

            _context.Teklifler.Add(teklif);
            _context.SaveChanges();

            foreach (var urun in urunler)
            {
                _context.TeklifUrunleri.Add(new TeklifUrun
                {
                    TeklifId = teklif.TeklifId,
                    UrunId = urun.UrunId,
                    Adet = urun.Adet,
                    BirimFiyat = urun.BirimFiyat,
                    IndirimliBirimFiyat = urun.IndirimliFiyat,
                    ToplamTutar = urun.Toplam
                });
            }

            var toplamFiyat = TeklifHesaplayici.HesaplaToplamFiyat(urunler);
            var kdvTutari = TeklifHesaplayici.HesaplaKdv(toplamFiyat, kdvOrani);
            var genelToplam = TeklifHesaplayici.HesaplaGenelToplam(toplamFiyat, kdvOrani);

            _context.TeklifToplamlari.Add(new TeklifToplam
            {
                TeklifId = teklif.TeklifId,
                IndirimliToplam = toplamFiyat,
                KdvTutari = kdvTutari,
                GenelToplam = genelToplam
            });

            _context.SaveChanges();
            transaction.Commit();

            EventHub.RaiseTeklifGuncellendi(teklif.TeklifId);

            SaveFileDialog saveFileDialog = new()
            {
                Filter = "PDF Dosyaları (*.pdf)|*.pdf",
                FileName = $"Teklif_{firma.FirmaAdi}_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using var writer = new PdfWriter(saveFileDialog.FileName);
                using var pdf = new PdfDocument(writer);
                using var doc = new Document(pdf, PageSize.A4);

                // iText 9.2.0 için font tanımlama
                PdfFont regularFont = PdfFontFactory.CreateFont(
                    @"C:\Windows\Fonts\arial.ttf",
                    PdfEncodings.IDENTITY_H,
                    PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED
                );

                PdfFont boldFont = PdfFontFactory.CreateFont(
                    @"C:\Windows\Fonts\arialbd.ttf",
                    PdfEncodings.IDENTITY_H,
                    PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED
                );


                PdfFormXObject? girisBackground = null, teklifBackground = null, sozlesmeBackground = null;


                try
                {
                    if (File.Exists(GirisSayfaPath))
                    {
                        using var girisPdf = new PdfDocument(new PdfReader(GirisSayfaPath));
                        if (girisPdf.GetNumberOfPages() > 0)
                        {
                            girisBackground = girisPdf.GetPage(1).CopyAsFormXObject(pdf);
                        }
                    }

                    if (File.Exists(TeklifSayfaPath))
                    {
                        using var teklifPdf = new PdfDocument(new PdfReader(TeklifSayfaPath));
                        if (teklifPdf.GetNumberOfPages() > 0)
                        {
                            teklifBackground = teklifPdf.GetPage(1).CopyAsFormXObject(pdf);
                        }
                    }

                    if (File.Exists(SozlesmeSayfaPath))
                    {
                        using var sozlesmePdf = new PdfDocument(new PdfReader(SozlesmeSayfaPath));
                        if (sozlesmePdf.GetNumberOfPages() > 0)
                        {
                            sozlesmeBackground = sozlesmePdf.GetPage(1).CopyAsFormXObject(pdf);
                        }
                    }

                    PdfPage girisPage = pdf.AddNewPage();
                    if (girisBackground != null)
                    {
                        PdfCanvas canvas = new PdfCanvas(girisPage);
                        canvas.AddXObjectAt(girisBackground, 0, 0);
                        canvas.Release();
                    }

                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    PdfPage teklifPage = pdf.GetLastPage();
                    if (teklifBackground != null)
                    {
                        PdfCanvas canvas = new PdfCanvas(teklifPage);
                        canvas.AddXObjectAt(teklifBackground, 0, 0);
                        canvas.Release();
                    }

                    doc.Add(new Paragraph("\n\n\n\n")
                        .SetMarginTop(50f)
                        .SetMarginBottom(0f));

                    doc.Add(new Paragraph("Teklif Edilen Ürünler")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(boldFont));

                    Table table = new Table(ProductColumnWidths).UseAllAvailableWidth();
                    AddCellToHeader(table, "No", boldFont);
                    AddCellToHeader(table, "Ürün Kodu", boldFont);
                    AddCellToHeader(table, "Açıklama", boldFont);
                    AddCellToHeader(table, "Adet", boldFont);
                    AddCellToHeader(table, "Birim Satış Fiyatı", boldFont);
                    AddCellToHeader(table, $"İndirimli Birim Satış Fiyatı(%{genelIndirimOrani})", boldFont);
                    AddCellToHeader(table, "Toplam Fiyat", boldFont);

                    int rowCount = 0;
                    int urunNo = 1;
                    foreach (var urun in urunler)
                    {
                        Color rowColor = rowCount % 2 == 0 ? ColorConstants.WHITE : new DeviceRgb(245, 245, 245);
                        AddCellToBody(table, urunNo.ToString(), regularFont, rowColor);
                        AddCellToBody(table, urun.UrunKodu, regularFont, rowColor);
                        AddCellToBody(table, urun.UrunAciklamasi, regularFont, rowColor);
                        AddCellToBody(table, urun.Adet.ToString(), regularFont, rowColor);
                        AddCellToBody(table, FormatPrice(urun.BirimFiyat, currency), regularFont, rowColor);
                        AddCellToBody(table, FormatPrice(urun.IndirimliFiyat, currency), regularFont, rowColor);
                        AddCellToBody(table, FormatPrice(urun.Toplam, currency), regularFont, rowColor);
                        rowCount++;
                        urunNo++;
                    }
                    doc.Add(table);

                    Table toplamTable = new Table(TotalColumnWidths).SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                    toplamTable.AddCell(CreateRightAlignedHeaderCell($"İndirimli Toplam(%{genelIndirimOrani}):", boldFont));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(toplamFiyat, currency), regularFont));
                    toplamTable.AddCell(CreateRightAlignedHeaderCell($"KDV (%{kdvOrani}):", boldFont));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(kdvTutari, currency), regularFont));
                    toplamTable.AddCell(CreateRightAlignedHeaderCell("Genel Toplam:", boldFont));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(genelToplam, currency), regularFont));
                    doc.Add(toplamTable);

                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    PdfPage sozlesmePage = pdf.GetLastPage();
                    if (sozlesmeBackground != null)
                    {
                        PdfCanvas canvas = new PdfCanvas(sozlesmePage);
                        canvas.AddXObjectAt(sozlesmeBackground, 0, 0);
                        canvas.Release();
                    }

                    doc.Add(new Paragraph("\n\n\n\n")
                        .SetMarginTop(50f)
                        .SetMarginBottom(0f));

                    doc.Add(new Paragraph("Satış Sözleşmesi")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(boldFont));
                    doc.Add(new Paragraph(sozlesmeMetni)
                        .SetFont(regularFont));

                    doc.Close();

                    MessageBox.Show("Teklif başarıyla kaydedildi ve PDF oluşturuldu!",
                                    "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PDF oluşturulurken bir hata oluştu: {ex.Message}",
                                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static string FormatPrice(decimal price, string currency)
        {
            return price.ToString("C2", GetCultureByCurrency(currency));
        }

        private static CultureInfo GetCultureByCurrency(string currency) => currency switch
        {
            "USD" => new CultureInfo("en-US"),
            "EUR" => new CultureInfo("en-IE"),
            _ => new CultureInfo("tr-TR"),
        };

        private static void AddCellToHeader(Table table, string text, PdfFont font)
        {
            table.AddHeaderCell(new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPadding(5)
                .SetBorder(Border.NO_BORDER));
        }

        private static void AddCellToBody(Table table, string text, PdfFont font, Color backgroundColor)
        {
            table.AddCell(new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetBackgroundColor(backgroundColor)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPadding(5)
                .SetBorder(Border.NO_BORDER));
        }

        private static Cell CreateRightAlignedHeaderCell(string text, PdfFont font)
        {
            return new Cell().Add(new Paragraph(text).SetFont(font))
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetBorder(Border.NO_BORDER)
                .SetPaddingRight(5);
        }

        private static Cell CreateLeftAlignedBodyCell(string text, PdfFont font)
        {
            return new Cell().Add(new Paragraph(text).SetFont(font))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetBorder(Border.NO_BORDER);
        }
    }
}
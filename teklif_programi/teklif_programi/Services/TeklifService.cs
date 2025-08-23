using iText.IO.Font.Constants;
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
    /// <summary>
    ///     Veritabanına teklif kaydetme ve PDF oluşturma işlemlerini yöneten servis.
    ///     Bu sınıf, <see cref="TeklifVerViewModel"/> içerisindeki karmaşık mantığı
    ///     ayrı bir katmanda toplamak için oluşturulmuştur.
    /// </summary>
    public class TeklifService
    {
        private readonly TeklifDbContext _context = new();
        private static readonly float[] FirmaColumnWidths = { 2f, 2f };
        private static readonly float[] ProductColumnWidths = { 1f, 2f, 5f, 1f, 2f, 2f, 2f };
        private static readonly float[] TotalColumnWidths = { 3f, 2f };
        private static readonly PdfFont BoldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        // Arka plan PDF dosya yolları
        private static readonly string GirisSayfaPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\girisSayfa.pdf";
        private static readonly string TeklifSayfaPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\teklifSayfa.pdf";
        private static readonly string SozlesmeSayfaPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\sozlesmeSayfa.pdf";

        /// <summary>
        ///     Seçilen firma ve ürünlere göre teklifi kaydeder ve kullanıcıdan alınan
        ///     dosya yoluna teklif PDF'i oluşturur.
        /// </summary>
        /// <param name="firma">Teklifin oluşturulduğu firma.</param>
        /// <param name="urunler">Teklifte yer alan ürünler.</param>
        /// <param name="genelIndirimOrani">Genel indirim yüzdesi.</param>
        /// <param name="kdvOrani">KDV oranı.</param>
        /// <param name="currency">Teklif para birimi.</param>
        /// <param name="ilgiliKisi">Teklifte belirtilen ilgili kişi.</param>
        /// <param name="ilgiliKisiTelefonu">İlgili kişinin telefon numarası.</param>
        /// <param name="ilgiliKisiEposta">İlgili kişinin e-posta adresi.</param>
        /// <param name="sozlesmeMetni">Satış sözleşmesi metni.</param>
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

            // Önce teklifi veritabanına kaydet
            using var transaction = _context.Database.BeginTransaction();

            var teklif = new Teklif
            {
                MusteriId = firma.MusteriId,
                PersonelId = 2, // TODO: Oturumdaki kullanıcı bilgisi
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

            // Hesaplamalar merkezi TeklifHesaplayici üzerinden yapılır
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

            // Ardından PDF oluştur
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

                // Arka plan PDF'lerini yükle
                PdfXObject girisBackground = null, teklifBackground = null, sozlesmeBackground = null;

                try
                {
                    // Giriş sayfası arka planı
                    if (File.Exists(GirisSayfaPath))
                    {
                        using var girisPdf = new PdfDocument(new PdfReader(GirisSayfaPath));
                        if (girisPdf.GetNumberOfPages() > 0)
                        {
                            girisBackground = girisPdf.GetPage(1).CopyAsFormXObject(pdf);
                        }
                    }

                    // Teklif sayfası arka planı
                    if (File.Exists(TeklifSayfaPath))
                    {
                        using var teklifPdf = new PdfDocument(new PdfReader(TeklifSayfaPath));
                        if (teklifPdf.GetNumberOfPages() > 0)
                        {
                            teklifBackground = teklifPdf.GetPage(1).CopyAsFormXObject(pdf);
                        }
                    }

                    // Sözleşme sayfası arka planı
                    if (File.Exists(SozlesmeSayfaPath))
                    {
                        using var sozlesmePdf = new PdfDocument(new PdfReader(SozlesmeSayfaPath));
                        if (sozlesmePdf.GetNumberOfPages() > 0)
                        {
                            sozlesmeBackground = sozlesmePdf.GetPage(1).CopyAsFormXObject(pdf);
                        }
                    }

                    // İlk sayfa: Giriş sayfası (girisSayfa.pdf)
                    PdfPage girisPage = pdf.AddNewPage();
                    if (girisBackground != null)
                    {
                        PdfCanvas canvas = new PdfCanvas(girisPage, true);
                        canvas.AddXObjectAt(girisBackground, 0, 0);
                        canvas.Release();
                    }

                    // Yeni sayfa: Teklif bilgileri ve ürün tablosu (teklifSayfa.pdf)
                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    PdfPage teklifPage = pdf.GetLastPage();
                    if (teklifBackground != null)
                    {
                        PdfCanvas canvas = new PdfCanvas(teklifPage, true);
                        canvas.AddXObjectAt(teklifBackground, 0, 0);
                        canvas.Release();
                    }

                    // Üstte boşluk eklemek için boş bir alan bırak
                    doc.Add(new Paragraph("\n\n\n\n")
                        .SetMarginTop(50f)
                        .SetMarginBottom(0f));

                    doc.Add(new Paragraph("Teklif Edilen Ürünler")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(BoldFont));

                    Table table = new Table(ProductColumnWidths).UseAllAvailableWidth();
                    AddCellToHeader(table, "No");
                    AddCellToHeader(table, "Ürün Kodu");
                    AddCellToHeader(table, "Açıklama");
                    AddCellToHeader(table, "Adet");
                    AddCellToHeader(table, "Birim Satış Fiyatı");
                    AddCellToHeader(table, $"İndirimli Birim Satış Fiyatı(%{genelIndirimOrani})");
                    AddCellToHeader(table, "Toplam Fiyat");

                    int rowCount = 0;
                    int urunNo = 1;
                    foreach (var urun in urunler)
                    {
                        Color rowColor = rowCount % 2 == 0 ? ColorConstants.WHITE : new DeviceRgb(245, 245, 245);
                        AddCellToBody(table, urunNo.ToString(), rowColor);
                        AddCellToBody(table, urun.UrunKodu, rowColor);
                        AddCellToBody(table, urun.UrunAciklamasi, rowColor);
                        AddCellToBody(table, urun.Adet.ToString(), rowColor);
                        AddCellToBody(table, FormatPrice(urun.BirimFiyat, currency), rowColor);
                        AddCellToBody(table, FormatPrice(urun.IndirimliFiyat, currency), rowColor);
                        AddCellToBody(table, FormatPrice(urun.Toplam, currency), rowColor);
                        rowCount++;
                        urunNo++;
                    }
                    doc.Add(table);

                    Table toplamTable = new Table(TotalColumnWidths).SetHorizontalAlignment(HorizontalAlignment.RIGHT);
                    toplamTable.AddCell(CreateRightAlignedHeaderCell($"İndirimli Toplam(%{genelIndirimOrani}):"));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(toplamFiyat, currency)));
                    toplamTable.AddCell(CreateRightAlignedHeaderCell($"KDV (%{kdvOrani}):"));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(kdvTutari, currency)));
                    toplamTable.AddCell(CreateRightAlignedHeaderCell("Genel Toplam:"));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(genelToplam, currency)));
                    doc.Add(toplamTable);

                    // Yeni sayfa: Sözleşme (sozlesmeSayfa.pdf)
                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    PdfPage sozlesmePage = pdf.GetLastPage();
                    if (sozlesmeBackground != null)
                    {
                        PdfCanvas canvas = new PdfCanvas(sozlesmePage, true);
                        canvas.AddXObjectAt(sozlesmeBackground, 0, 0);
                        canvas.Release();
                    }

                    // Üstte boşluk eklemek için boş bir alan bırak
                    doc.Add(new Paragraph("\n\n\n\n")
                        .SetMarginTop(50f)
                        .SetMarginBottom(0f));

                    doc.Add(new Paragraph("Satış Sözleşmesi")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(BoldFont));
                    doc.Add(new Paragraph(sozlesmeMetni));

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

        private static void AddCellToHeader(Table table, string text)
        {
            table.AddHeaderCell(new Cell()
                .Add(new Paragraph(text))
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPadding(5)
                .SetBorder(Border.NO_BORDER));
        }

        private static void AddCellToBody(Table table, string text, Color backgroundColor)
        {
            table.AddCell(new Cell()
                .Add(new Paragraph(text))
                .SetBackgroundColor(backgroundColor)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPadding(5)
                .SetBorder(Border.NO_BORDER));
        }

        private static Cell CreateRightAlignedHeaderCell(string text)
        {
            return new Cell().Add(new Paragraph(text))
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetBorder(Border.NO_BORDER)
                .SetPaddingRight(5);
        }

        private static Cell CreateLeftAlignedBodyCell(string text)
        {
            return new Cell().Add(new Paragraph(text))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetBorder(Border.NO_BORDER);
        }
    }
}
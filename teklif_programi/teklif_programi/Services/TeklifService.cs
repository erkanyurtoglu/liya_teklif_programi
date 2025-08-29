using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Draw;
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
        private static readonly float[] InfoColumnWidths = { 1f, 1f }; // Sol ve sağ sütunlar için hizalamayı düzenlemek

        private static readonly string GirisSayfaTrPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\girisSayfa.pdf";
        private static readonly string TeklifSayfaTrPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\teklifSayfa.pdf";
        private static readonly string SozlesmeSayfaTrPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\sozlesmeSayfa.pdf";
        private static readonly string GirisSayfaEnPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\girisSayfaEnglish.pdf";
        private static readonly string TeklifSayfaEnPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\teklifSayfaEnglish.pdf";
        private static readonly string SozlesmeSayfaEnPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\sozlesmeSayfaEnglish.pdf";
        private static readonly string UretimListesiPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\uretimListesi.pdf";

        public void KaydetVePdfIndir(Musteri firma,
                                     IEnumerable<TeklifUrunModel> urunler,
                                     decimal genelIndirimOrani,
                                     decimal kdvOrani,
                                     decimal paketlemeUcreti,
                                     decimal tasimaUcreti,
                                     string currency,
                                     string teslimatSekli,
                                     string teslimatYeri,
                                     string ilgiliKisi,
                                     string ilgiliKisiTelefonu,
                                     string ilgiliKisiEposta,
                                     string sozlesmeMetni,
                                     string selectedLanguage)
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
                IlgiliKisiEposta = ilgiliKisiEposta,
                TeslimatSekli = teslimatSekli,
                TeslimatYeri = teslimatYeri,
                Dil = selectedLanguage
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
            var genelToplam = TeklifHesaplayici.HesaplaGenelToplam(toplamFiyat, kdvOrani, paketlemeUcreti, tasimaUcreti);

            _context.TeklifToplamlari.Add(new TeklifToplam
            {
                TeklifId = teklif.TeklifId,
                IndirimliToplam = toplamFiyat,
                KdvTutari = kdvTutari,
                PaketlemeUcreti = paketlemeUcreti,
                TasimaUcreti = tasimaUcreti,
                GenelToplam = genelToplam
            });

            _context.SaveChanges();

            var personel = _context.Personeller.FirstOrDefault(p => p.PersonelId == teklif.PersonelId);

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
                doc.SetMargins(20f, 5f, 30f, 5f);

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

                doc.SetFont(regularFont);

                PdfFormXObject? girisBackground = null, teklifBackground = null, sozlesmeBackground = null;

                var isEnglish = selectedLanguage.Equals("EN", StringComparison.OrdinalIgnoreCase);
                var girisPath = isEnglish ? GirisSayfaEnPath : GirisSayfaTrPath;
                var teklifPath = isEnglish ? TeklifSayfaEnPath : TeklifSayfaTrPath;
                var sozlesmePath = isEnglish ? SozlesmeSayfaEnPath : SozlesmeSayfaTrPath;

                try
                {
                    if (File.Exists(girisPath))
                    {
                        using var girisPdf = new PdfDocument(new PdfReader(girisPath));
                        if (girisPdf.GetNumberOfPages() > 0)
                        {
                            girisBackground = girisPdf.GetPage(1).CopyAsFormXObject(pdf);
                        }
                    }

                    if (File.Exists(teklifPath))
                    {
                        using var teklifPdf = new PdfDocument(new PdfReader(teklifPath));
                        if (teklifPdf.GetNumberOfPages() > 0)
                        {
                            teklifBackground = teklifPdf.GetPage(1).CopyAsFormXObject(pdf);
                        }
                    }

                    if (File.Exists(sozlesmePath))
                    {
                        using var sozlesmePdf = new PdfDocument(new PdfReader(sozlesmePath));
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

                    Table infoTable = new Table(new float[] { 3.5f, 1f })
                        .SetWidth(UnitValue.CreatePercentValue(80)) // sayfa genişliğinin %80’i
                        .SetHorizontalAlignment(HorizontalAlignment.RIGHT) 
                        .SetMarginTop(40f);


                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Company Name:" : "Firma Adı:",
                        firma.FirmaAdi,
                        boldFont,
                        regularFont));
                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Date:" : "Teklif Tarihi:",
                        teklif.OlusturmaTarihi.ToString("dd.MM.yyyy"),
                        boldFont,
                        regularFont));

                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Contact Person:" : "İlgili Kişi:",
                        ilgiliKisi,
                        boldFont,
                        regularFont));
                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Quote No:" : "Teklif No:",
                        teklif.TeklifId.ToString(),
                        boldFont,
                        regularFont));


                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Phone:" : "Telefon:",
                        ilgiliKisiTelefonu,
                        boldFont,
                        regularFont));
                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Prepared By:" : "Teklifi Yapan:",
                        personel?.AdSoyad ?? string.Empty,
                        boldFont,
                        regularFont));


                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Email:" : "E-posta:",
                        ilgiliKisiEposta,
                        boldFont,
                        regularFont));
                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Personnel Phone:" : "Personel Telefon:",
                        personel?.Telefon ?? string.Empty,
                        boldFont,
                        regularFont));

                    doc.Add(infoTable);

                    var topSeparator = new LineSeparator(new SolidLine(0.5f))
                        .SetWidth(UnitValue.CreatePercentValue(100));
                    doc.Add(topSeparator);

                    doc.Add(new Paragraph(isEnglish ? "Offered Products" : "Teklif Edilen Ürünler")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(boldFont)
                        .SetFontSize(11));

                    var bottomSeparator = new LineSeparator(new SolidLine(0.5f))
                        .SetWidth(UnitValue.CreatePercentValue(100));
                    doc.Add(bottomSeparator);

                    // --- ÜRÜN TABLOSU: indirim sütunu dinamik ---
                    bool showDiscountCol = genelIndirimOrani > 0;

                    // No, Ürün Kodu, Açıklama, Adet, Birim Fiyat
                    var productWidths = new List<float> { 1f, 2f, 5f, 1f, 2f };
                    if (showDiscountCol)
                        productWidths.Add(2f);               // İndirimli Birim
                    productWidths.Add(2f);                   // Toplam

                    Table table = new Table(productWidths.ToArray())
                        .UseAllAvailableWidth()
                        .SetMarginTop(5f);

                    // Header'lar
                    AddCellToHeader(table, "No", boldFont);
                    AddCellToHeader(table, isEnglish ? "Product Code" : "Ürün Kodu", boldFont);
                    AddCellToHeader(table, isEnglish ? "Description" : "Açıklama", boldFont);
                    AddCellToHeader(table, isEnglish ? "Quantity" : "Adet", boldFont);
                    AddCellToHeader(table, isEnglish ? "Unit Price" : "Birim Satış Fiyatı", boldFont);
                    if (showDiscountCol)
                    {
                        AddCellToHeader(table,
                            isEnglish
                                ? $"Discounted Unit Price(%{genelIndirimOrani})"
                                : $"İndirimli Birim Satış Fiyatı(%{genelIndirimOrani})",
                            boldFont);
                    }
                    AddCellToHeader(table, isEnglish ? "Total Price" : "Toplam Fiyat", boldFont);

                    // Satırlar
                    int rowCount = 0, urunNo = 1;
                    foreach (var urun in urunler)
                    {
                        Color rowColor = rowCount % 2 == 0 ? ColorConstants.WHITE : new DeviceRgb(245, 245, 245);

                        AddCellToBody(table, urunNo.ToString(), regularFont, rowColor);
                        AddCellToBody(table, urun.UrunKodu, regularFont, rowColor);
                        AddCellToBody(table, urun.UrunAciklamasi, regularFont, rowColor);
                        AddCellToBody(table, urun.Adet.ToString(), regularFont, rowColor);
                        AddCellToBody(table, FormatPrice(urun.BirimFiyat, currency), regularFont, rowColor);

                        if (showDiscountCol)
                            AddCellToBody(table, FormatPrice(urun.IndirimliFiyat, currency), regularFont, rowColor);

                        AddCellToBody(table, FormatPrice(urun.Toplam, currency), regularFont, rowColor);

                        rowCount++; urunNo++;
                    }
                    doc.Add(table);


                    // --- Toplamlar bölümü ---
                    Table toplamTable = new Table(TotalColumnWidths)
                        .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                        .SetMarginTop(40f);

                    // İndirimli Toplam 
                    if(genelIndirimOrani > 0)
                    {
                        toplamTable.AddCell(CreateRightAlignedHeaderCell(
                                isEnglish ? $"Discounted Total(%{genelIndirimOrani}):" : $"İndirimli Toplam(%{genelIndirimOrani}):",
                                boldFont));
                        toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(toplamFiyat, currency), regularFont));
                    }

                    // KDV 
                    if(kdvOrani > 0)
                    {
                        toplamTable.AddCell(CreateRightAlignedHeaderCell(
                        isEnglish ? $"VAT (%{kdvOrani}):" : $"KDV (%{kdvOrani}):",
                        boldFont));
                        toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(kdvTutari, currency), regularFont));
                    }

                    // Teslimat bilgileri
                    if (!string.IsNullOrWhiteSpace(teslimatSekli) || !string.IsNullOrWhiteSpace(teslimatYeri))
                    {
                        bool first = true;
                        if (!string.IsNullOrWhiteSpace(teslimatSekli))
                        {
                            var cell1 = CreateRightAlignedHeaderCell(
                                isEnglish ? "Delivery Method:" : "Teslimat Şekli:",
                                boldFont);
                            var cell2 = CreateLeftAlignedBodyCell(teslimatSekli, regularFont);
                            cell1.SetBorderTop(new SolidBorder(ColorConstants.BLACK, 0.5f));
                            cell2.SetBorderTop(new SolidBorder(ColorConstants.BLACK, 0.5f));
                            toplamTable.AddCell(cell1);
                            toplamTable.AddCell(cell2);
                            first = false;
                        }
                        if (!string.IsNullOrWhiteSpace(teslimatYeri))
                        {
                            var cell1 = CreateRightAlignedHeaderCell(
                                isEnglish ? "Delivery Place:" : "Teslimat Yeri:",
                                boldFont);
                            var cell2 = CreateLeftAlignedBodyCell(teslimatYeri, regularFont);
                            if (first)
                            {
                                cell1.SetBorderTop(new SolidBorder(ColorConstants.BLACK, 0.5f));
                                cell2.SetBorderTop(new SolidBorder(ColorConstants.BLACK, 0.5f));
                            }
                            toplamTable.AddCell(cell1);
                            toplamTable.AddCell(cell2);
                        }
                    }


                    // Paketleme
                    if (paketlemeUcreti > 0)
                    {
                        toplamTable.AddCell(CreateRightAlignedHeaderCell(
                                isEnglish ? "Packaging Fee:" : "Paketleme Ücreti:",
                                boldFont));
                        toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(paketlemeUcreti, currency), regularFont));
                    }

                    // Taşıma varsa ekle
                    if (tasimaUcreti > 0)
                    {
                        toplamTable.AddCell(CreateRightAlignedHeaderCell(
                                isEnglish ? "Transport Fee:" : "Taşıma Ücreti:",
                                boldFont));
                        toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(tasimaUcreti, currency), regularFont));
                    }



                    // GENEL TOPLAM (SADECE burada üst çizgi var)
                    toplamTable.AddCell(CreateRightAlignedHeaderCell(
                            isEnglish ? "Grand Total:" : "Genel Toplam:",
                            boldFont)
                        .SetBorderTop(new SolidBorder(ColorConstants.BLACK, 0.5f)));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(
                            FormatPrice(genelToplam, currency), regularFont)
                        .SetBorderTop(new SolidBorder(ColorConstants.BLACK, 0.5f)));

                    doc.Add(toplamTable);



                    doc.SetMargins(80f, 30f, 40f, 30f);
                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    PdfPage sozlesmePage = pdf.GetLastPage();
                    if (sozlesmeBackground != null)
                    {
                        PdfCanvas canvas = new PdfCanvas(sozlesmePage);
                        canvas.AddXObjectAt(sozlesmeBackground, 0, 0);
                        canvas.Release();
                    }

                    doc.Add(new Paragraph(sozlesmeMetni)
                        .SetFont(regularFont)
                        .SetFontSize(10));

                    doc.Close();

                    transaction.Commit();
                    EventHub.RaiseTeklifGuncellendi(teklif.TeklifId);

                    MessageBox.Show("Teklif başarıyla kaydedildi ve PDF oluşturuldu!",
                                    "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PDF oluşturulurken bir hata oluştu: {ex.Message}",
                                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                transaction.Rollback();
                _context.ChangeTracker.Clear();
                MessageBox.Show("İşlem iptal edildi, teklif kaydedilmedi.",
                                "İptal", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void PdfIndir(Teklif teklif,
                             IEnumerable<TeklifUrunModel> urunler,
                             string sozlesmeMetni,
                             string selectedLanguage)
        {
            ArgumentNullException.ThrowIfNull(teklif);
            ArgumentNullException.ThrowIfNull(urunler);
            if (!urunler.Any()) throw new ArgumentException("En az bir ürün seçilmelidir.");

            var firma = teklif.Musteri ?? _context.Musteriler.First(m => m.MusteriId == teklif.MusteriId);
            var personel = _context.Personeller.FirstOrDefault(p => p.PersonelId == teklif.PersonelId);

            var genelIndirimOrani = teklif.GenelIndirimOrani;
            var kdvOrani = teklif.KdvOrani;
            var currency = teklif.ParaBirimi;
            var teslimatSekli = teklif.TeslimatSekli ?? string.Empty;
            var teslimatYeri = teklif.TeslimatYeri ?? string.Empty;
            var ilgiliKisi = teklif.IlgiliKisi;
            var ilgiliKisiTelefonu = teklif.IlgiliKisiTelefonu;
            var ilgiliKisiEposta = teklif.IlgiliKisiEposta;

            var paketlemeUcreti = teklif.TeklifToplam?.PaketlemeUcreti ?? 0;
            var tasimaUcreti = teklif.TeklifToplam?.TasimaUcreti ?? 0;

            var toplamFiyat = TeklifHesaplayici.HesaplaToplamFiyat(urunler);
            var kdvTutari = TeklifHesaplayici.HesaplaKdv(toplamFiyat, kdvOrani);
            var genelToplam = TeklifHesaplayici.HesaplaGenelToplam(toplamFiyat, kdvOrani, paketlemeUcreti, tasimaUcreti);

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
                doc.SetMargins(20f, 5f, 30f, 5f);

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

                doc.SetFont(regularFont);

                PdfFormXObject? girisBackground = null, teklifBackground = null, sozlesmeBackground = null;

                var isEnglish = selectedLanguage.Equals("EN", StringComparison.OrdinalIgnoreCase);
                var girisPath = isEnglish ? GirisSayfaEnPath : GirisSayfaTrPath;
                var teklifPath = isEnglish ? TeklifSayfaEnPath : TeklifSayfaTrPath;
                var sozlesmePath = isEnglish ? SozlesmeSayfaEnPath : SozlesmeSayfaTrPath;

                try
                {
                    if (File.Exists(girisPath))
                    {
                        using var girisPdf = new PdfDocument(new PdfReader(girisPath));
                        if (girisPdf.GetNumberOfPages() > 0)
                        {
                            girisBackground = girisPdf.GetPage(1).CopyAsFormXObject(pdf);
                        }
                    }

                    if (File.Exists(teklifPath))
                    {
                        using var teklifPdf = new PdfDocument(new PdfReader(teklifPath));
                        if (teklifPdf.GetNumberOfPages() > 0)
                        {
                            teklifBackground = teklifPdf.GetPage(1).CopyAsFormXObject(pdf);
                        }
                    }

                    if (File.Exists(sozlesmePath))
                    {
                        using var sozlesmePdf = new PdfDocument(new PdfReader(sozlesmePath));
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

                    Table infoTable = new Table(new float[] { 3.5f, 1f })
                        .SetWidth(UnitValue.CreatePercentValue(80))
                        .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                        .SetMarginTop(40f);

                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Company Name:" : "Firma Adı:",
                        firma.FirmaAdi,
                        boldFont,
                        regularFont));
                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Date:" : "Teklif Tarihi:",
                        teklif.OlusturmaTarihi.ToString("dd.MM.yyyy"),
                        boldFont,
                        regularFont));

                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Contact Person:" : "İlgili Kişi:",
                        ilgiliKisi,
                        boldFont,
                        regularFont));
                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Quote No:" : "Teklif No:",
                        teklif.TeklifId.ToString(),
                        boldFont,
                        regularFont));

                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Phone:" : "Telefon:",
                        ilgiliKisiTelefonu,
                        boldFont,
                        regularFont));
                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Prepared By:" : "Teklifi Yapan:",
                        personel?.AdSoyad ?? string.Empty,
                        boldFont,
                        regularFont));

                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Email:" : "E-posta:",
                        ilgiliKisiEposta,
                        boldFont,
                        regularFont));
                    infoTable.AddCell(CreateInfoCell(
                        isEnglish ? "Personnel Phone:" : "Personel Telefon:",
                        personel?.Telefon ?? string.Empty,
                        boldFont,
                        regularFont));

                    doc.Add(infoTable);

                    var topSeparator = new LineSeparator(new SolidLine(0.5f))
                        .SetWidth(UnitValue.CreatePercentValue(100));
                    doc.Add(topSeparator);

                    doc.Add(new Paragraph(isEnglish ? "Offered Products" : "Teklif Edilen Ürünler")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(boldFont)
                        .SetFontSize(11));

                    var bottomSeparator = new LineSeparator(new SolidLine(0.5f))
                        .SetWidth(UnitValue.CreatePercentValue(100));
                    doc.Add(bottomSeparator);

                    // --- ÜRÜN TABLOSU: indirim sütunu dinamik ---
                    bool showDiscountCol = genelIndirimOrani > 0;

                    // No, Ürün Kodu, Açıklama, Adet, Birim Fiyat
                    var productWidths = new List<float> { 1f, 2f, 5f, 1f, 2f };
                    if (showDiscountCol) productWidths.Add(2f);   // İndirimli Birim
                    productWidths.Add(2f);                        // Toplam

                    Table table = new Table(productWidths.ToArray())
                        .UseAllAvailableWidth()
                        .SetMarginTop(5f);

                    // Header
                    AddCellToHeader(table, "No", boldFont);
                    AddCellToHeader(table, isEnglish ? "Product Code" : "Ürün Kodu", boldFont);
                    AddCellToHeader(table, isEnglish ? "Description" : "Açıklama", boldFont);
                    AddCellToHeader(table, isEnglish ? "Quantity" : "Adet", boldFont);
                    AddCellToHeader(table, isEnglish ? "Unit Price" : "Birim Satış Fiyatı", boldFont);
                    if (showDiscountCol)
                    {
                        AddCellToHeader(table,
                            isEnglish ? $"Discounted Unit Price(%{genelIndirimOrani})"
                                      : $"İndirimli Birim Satış Fiyatı(%{genelIndirimOrani})",
                            boldFont);
                    }
                    AddCellToHeader(table, isEnglish ? "Total Price" : "Toplam Fiyat", boldFont);

                    // Rows
                    int rowCount = 0, urunNo = 1;
                    foreach (var urun in urunler)
                    {
                        Color rowColor = rowCount % 2 == 0 ? ColorConstants.WHITE : new DeviceRgb(245, 245, 245);

                        AddCellToBody(table, urunNo.ToString(), regularFont, rowColor);
                        AddCellToBody(table, urun.UrunKodu, regularFont, rowColor);
                        AddCellToBody(table, urun.UrunAciklamasi, regularFont, rowColor);
                        AddCellToBody(table, urun.Adet.ToString(), regularFont, rowColor);
                        AddCellToBody(table, FormatPrice(urun.BirimFiyat, currency), regularFont, rowColor);

                        if (showDiscountCol)
                            AddCellToBody(table, FormatPrice(urun.IndirimliFiyat, currency), regularFont, rowColor);

                        AddCellToBody(table, FormatPrice(urun.Toplam, currency), regularFont, rowColor);

                        rowCount++; urunNo++;
                    }
                    doc.Add(table);


                    // --- Toplamlar bölümü ---
                    Table toplamTable = new Table(TotalColumnWidths)
                        .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                        .SetMarginTop(40f);

                    // İndirimli Toplam 
                    if (genelIndirimOrani > 0)
                    {
                        toplamTable.AddCell(CreateRightAlignedHeaderCell(
                            isEnglish ? $"Discounted Total(%{genelIndirimOrani}):"
                                      : $"İndirimli Toplam(%{genelIndirimOrani}):",
                            boldFont));
                        toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(toplamFiyat, currency), regularFont));
                    }

                    if (kdvOrani > 0)
                    {
                        toplamTable.AddCell(CreateRightAlignedHeaderCell(
                            isEnglish ? $"VAT (%{kdvOrani}):" : $"KDV (%{kdvOrani}):",
                            boldFont));
                        toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(kdvTutari, currency), regularFont));
                    }

                    if (!string.IsNullOrWhiteSpace(teslimatSekli) || !string.IsNullOrWhiteSpace(teslimatYeri))
                    {
                        bool first = true;
                        if (!string.IsNullOrWhiteSpace(teslimatSekli))
                        {
                            var cell1 = CreateRightAlignedHeaderCell(
                                isEnglish ? "Delivery Method:" : "Teslimat Şekli:",
                                boldFont);
                            var cell2 = CreateLeftAlignedBodyCell(teslimatSekli, regularFont);
                            cell1.SetBorderTop(new SolidBorder(ColorConstants.BLACK, 0.5f));
                            cell2.SetBorderTop(new SolidBorder(ColorConstants.BLACK, 0.5f));
                            toplamTable.AddCell(cell1);
                            toplamTable.AddCell(cell2);
                            first = false;
                        }
                        if (!string.IsNullOrWhiteSpace(teslimatYeri))
                        {
                            var cell1 = CreateRightAlignedHeaderCell(
                                isEnglish ? "Delivery Place:" : "Teslimat Yeri:",
                                boldFont);
                            var cell2 = CreateLeftAlignedBodyCell(teslimatYeri, regularFont);
                            if (first)
                            {
                                cell1.SetBorderTop(new SolidBorder(ColorConstants.BLACK, 0.5f));
                                cell2.SetBorderTop(new SolidBorder(ColorConstants.BLACK, 0.5f));
                            }
                            toplamTable.AddCell(cell1);
                            toplamTable.AddCell(cell2);
                        }
                    }


                    if (paketlemeUcreti > 0)
                    {
                        toplamTable.AddCell(CreateRightAlignedHeaderCell(
                            isEnglish ? "Packaging Fee:" : "Paketleme Ücreti:",
                            boldFont));
                        toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(paketlemeUcreti, currency), regularFont));
                    }

                    if (tasimaUcreti > 0)
                    {
                        toplamTable.AddCell(CreateRightAlignedHeaderCell(
                            isEnglish ? "Transport Fee:" : "Taşıma Ücreti:",
                            boldFont));
                        toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(tasimaUcreti, currency), regularFont));
                    }



                    // GENEL TOPLAM (SADECE burada üst çizgi var)
                    toplamTable.AddCell(CreateRightAlignedHeaderCell(
                            isEnglish ? "Grand Total:" : "Genel Toplam:",
                            boldFont)
                        .SetBorderTop(new SolidBorder(ColorConstants.BLACK, 0.5f)));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(
                            FormatPrice(genelToplam, currency), regularFont)
                        .SetBorderTop(new SolidBorder(ColorConstants.BLACK, 0.5f)));

                    doc.Add(toplamTable);


                    doc.SetMargins(80f, 30f, 40f, 30f);
                    doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    PdfPage sozlesmePage = pdf.GetLastPage();
                    if (sozlesmeBackground != null)
                    {
                        PdfCanvas canvas = new PdfCanvas(sozlesmePage);
                        canvas.AddXObjectAt(sozlesmeBackground, 0, 0);
                        canvas.Release();
                    }

                    doc.Add(new Paragraph(sozlesmeMetni)
                        .SetFont(regularFont)
                        .SetFontSize(10));

                    doc.Close();

                    MessageBox.Show("PDF başarıyla oluşturuldu!",
                                    "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"PDF oluşturulurken bir hata oluştu: {ex.Message}",
                                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            else
            {
                MessageBox.Show("İşlem iptal edildi, PDF kaydedilmedi.",
                                "İptal", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void UretimListesiPdfIndir(Teklif teklif, IEnumerable<TeklifUrunModel> urunler)
        {
            ArgumentNullException.ThrowIfNull(teklif);
            ArgumentNullException.ThrowIfNull(urunler);
            if (!urunler.Any()) throw new ArgumentException("En az bir ürün seçilmelidir.");

            try
            {
                using var writer = new PdfWriter(UretimListesiPath);
                using var pdf = new PdfDocument(writer);
                using var doc = new Document(pdf, PageSize.A4);
                doc.SetMargins(20f, 20f, 20f, 20f);

                PdfFont regularFont = PdfFontFactory.CreateFont(
                    @"C:\\Windows\\Fonts\\arial.ttf",
                    PdfEncodings.IDENTITY_H,
                    PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED
                );

                PdfFont boldFont = PdfFontFactory.CreateFont(
                    @"C:\\Windows\\Fonts\\arialbd.ttf",
                    PdfEncodings.IDENTITY_H,
                    PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED
                );

                doc.SetFont(regularFont);

                doc.Add(new Paragraph("Üretim Listesi")
                    .SetFont(boldFont)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10f));

                Table table = new Table(new float[] { 1f, 2f, 5f, 1f, 2f, 3f })
                    .UseAllAvailableWidth();

                Cell CreateHeader(string text) => new Cell()
                    .Add(new Paragraph(text).SetFont(boldFont).SetFontSize(9))
                    .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetPadding(5)
                    .SetBorder(Border.NO_BORDER);

                Cell CreateBody(string text, TextAlignment alignment = TextAlignment.LEFT) => new Cell()
                    .Add(new Paragraph(text).SetFont(regularFont).SetFontSize(9))
                    .SetBackgroundColor(ColorConstants.WHITE)
                    .SetTextAlignment(alignment)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetPadding(5)
                    .SetBorder(Border.NO_BORDER);

                table.AddHeaderCell(CreateHeader("Sıra No"));
                table.AddHeaderCell(CreateHeader("Ürün Kodu"));
                table.AddHeaderCell(CreateHeader("Açıklama"));
                table.AddHeaderCell(CreateHeader("Adet"));
                table.AddHeaderCell(CreateHeader("Tamamlandı"));
                table.AddHeaderCell(CreateHeader("Not"));

                int sira = 1;
                foreach (var u in urunler)
                {
                    table.AddCell(CreateBody(sira.ToString()));
                    table.AddCell(CreateBody(u.UrunKodu));
                    table.AddCell(CreateBody(u.UrunAciklamasi));
                    table.AddCell(CreateBody(u.Adet.ToString()));
                    table.AddCell(CreateBody(u.Tamamlandi ? "☑" : "☐", TextAlignment.CENTER));
                    table.AddCell(CreateBody(u.UretimNotu));
                    sira++;
                }

                doc.Add(table);
                doc.Close();

                MessageBox.Show("Üretim listesi PDF oluşturuldu!",
                                "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Üretim listesi oluşturulurken bir hata oluştu: {ex.Message}",
                                "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private static Cell CreateInfoCell(string label, string value, PdfFont boldFont, PdfFont regularFont)
        {
            var paragraph = new Paragraph()
                .SetFontSize(9)
                .Add(new Text(label).SetFont(boldFont))
                .Add(" ")
                .Add(new Text(value).SetFont(regularFont));

            return new Cell()
                .Add(paragraph)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT);
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
                .Add(new Paragraph(text).SetFont(font).SetFontSize(9))
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPadding(5)
                .SetBorder(Border.NO_BORDER));
        }

        private static void AddCellToBody(Table table, string text, PdfFont font, Color backgroundColor)
        {
            table.AddCell(new Cell()
                .Add(new Paragraph(text).SetFont(font).SetFontSize(9))
                .SetBackgroundColor(backgroundColor)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPadding(5)
                .SetBorder(Border.NO_BORDER));
        }

        private static Cell CreateRightAlignedHeaderCell(string text, PdfFont font)
        {
            return new Cell().Add(new Paragraph(text).SetFont(font).SetFontSize(10))
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetBorder(Border.NO_BORDER)
                .SetPaddingRight(5);
        }

        private static Cell CreateLeftAlignedBodyCell(string text, PdfFont font)
        {
            return new Cell().Add(new Paragraph(text).SetFont(font).SetFontSize(10))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetBorder(Border.NO_BORDER);
        }
    }
}
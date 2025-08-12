using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using teklif_programi.Data;
using teklif_programi.Models;

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

        /// <summary>
        ///     Seçilen firma ve ürünlere göre teklifi kaydeder ve kullanıcıdan alınan
        ///     dosya yoluna teklif PDF'i oluşturur.
        /// </summary>
        /// <param name="firma">Teklifin oluşturulduğu firma.</param>
        /// <param name="urunler">Teklifte yer alan ürünler.</param>
        /// <param name="genelIndirimOrani">Genel indirim yüzdesi.</param>
        /// <param name="kdvOrani">KDV oranı.</param>
        /// <param name="currency">Teklif para birimi.</param>
        /// <param name="sozlesmeMetni">Satış sözleşmesi metni.</param>
        public void KaydetVePdfIndir(Musteri firma,
                                      IEnumerable<TeklifUrunModel> urunler,
                                      decimal genelIndirimOrani,
                                      decimal kdvOrani,
                                      string currency,
                                      string sozlesmeMetni)
        {
            if (firma == null) throw new ArgumentNullException(nameof(firma));
            if (urunler == null || !urunler.Any()) throw new ArgumentException("En az bir ürün seçilmelidir.");

            // Önce teklifi veritabanına kaydet
            using var transaction = _context.Database.BeginTransaction();

            var teklif = new Teklif
            {
                MusteriId = firma.MusteriId,
                PersonelId = 2, // TODO: Oturumdaki kullanıcı bilgisi
                OlusturmaTarihi = DateTime.Now,
                GenelIndirimOrani = genelIndirimOrani,
                KdvOrani = kdvOrani,
                ParaBirimi = currency
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

            var toplamFiyat = urunler.Sum(u => u.Toplam);
            var kdvTutari = toplamFiyat * (kdvOrani / 100);
            var genelToplam = toplamFiyat + kdvTutari;

            _context.TeklifToplamlari.Add(new TeklifToplam
            {
                TeklifId = teklif.TeklifId,
                IndirimliToplam = toplamFiyat,
                KdvTutari = kdvTutari,
                GenelToplam = genelToplam
            });

            _context.SaveChanges();
            transaction.Commit();

            // Ardından PDF oluştur
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "PDF Dosyaları (*.pdf)|*.pdf",
                FileName = $"Teklif_{firma.FirmaAdi}_{DateTime.Now:yyyyMMdd}.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string templatePath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\LiyaTeklifBelgesi.pdf";
                PdfReader reader = new PdfReader(templatePath);
                using FileStream fs = new(saveFileDialog.FileName, FileMode.Create);
                PdfStamper stamper = new PdfStamper(reader, fs);

                string fontPath = @"C:\Windows\Fonts\arial.ttf";
                if (!File.Exists(fontPath))
                {
                    MessageBox.Show("Arial font dosyası bulunamadı: " + fontPath,
                                    "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    stamper.Close();
                    reader.Close();
                    return;
                }

                BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font firmaFont = new(baseFont, 12, Font.NORMAL, new BaseColor(128, 128, 128));
                Font headerFont = new(baseFont, 12, Font.BOLD, BaseColor.BLACK);
                Font bodyFont = new(baseFont, 11, Font.NORMAL, BaseColor.BLACK);
                Font urunBaslikFont = new(baseFont, 14, Font.BOLD, BaseColor.BLACK);
                Font sozlesmeBaslikFont = new(baseFont, 16, Font.BOLD, BaseColor.BLACK);
                Font sozlesmeFont = new(baseFont, 12, Font.NORMAL, BaseColor.BLACK);

                int currentPage = 2;
                PdfContentByte canvas = stamper.GetOverContent(currentPage);

                PdfPTable firmaTable = new PdfPTable(2) { TotalWidth = 240f, DefaultCell = { Border = 0 } };
                firmaTable.SetWidths(new float[] { 2f, 2f });
                firmaTable.AddCell(CreateRightAlignedHeaderCell("Firma Adı:", headerFont));
                firmaTable.AddCell(CreateLeftAlignedBodyCell(firma.FirmaAdi, bodyFont));
                firmaTable.AddCell(CreateRightAlignedHeaderCell("Firma Adresi:", headerFont));
                firmaTable.AddCell(CreateLeftAlignedBodyCell(firma.FirmaAdresi, bodyFont));
                firmaTable.AddCell(CreateRightAlignedHeaderCell("Firma Telefonu:", headerFont));
                firmaTable.AddCell(CreateLeftAlignedBodyCell(firma.FirmaTelefonu ?? "Belirtilmemiş", bodyFont));
                firmaTable.AddCell(CreateRightAlignedHeaderCell("Firma E-Posta:", headerFont));
                firmaTable.AddCell(CreateLeftAlignedBodyCell(firma.FirmaEposta ?? "Belirtilmemiş", bodyFont));
                firmaTable.WriteSelectedRows(0, -1, 40f, 750f, canvas);

                Phrase urunBaslik = new Phrase("Teklif Edilen Ürünler", urunBaslikFont);
                ColumnText ctUrunBaslik = new ColumnText(canvas);
                ctUrunBaslik.SetSimpleColumn(urunBaslik, 22.5f, 560f, 572.5f, 580f, 15, Element.ALIGN_CENTER);
                ctUrunBaslik.Go();

                PdfPTable table = new PdfPTable(7) { TotalWidth = 550f, LockedWidth = true };
                table.SetWidths(new float[] { 1f, 2f, 5f, 1f, 2f, 2f, 2f });
                AddCellToHeader(table, "No", headerFont, new BaseColor(240, 240, 240));
                AddCellToHeader(table, "Ürün Kodu", headerFont, new BaseColor(240, 240, 240));
                AddCellToHeader(table, "Açıklama", headerFont, new BaseColor(240, 240, 240));
                AddCellToHeader(table, "Adet", headerFont, new BaseColor(240, 240, 240));
                AddCellToHeader(table, "Birim Satış Fiyatı", headerFont, new BaseColor(240, 240, 240));
                AddCellToHeader(table, $"İndirimli Birim Satış Fiyatı(%{genelIndirimOrani})", headerFont, new BaseColor(240, 240, 240));
                AddCellToHeader(table, "Toplam Fiyat", headerFont, new BaseColor(240, 240, 240));

                int rowCount = 0;
                int urunNo = 1;
                foreach (var urun in urunler)
                {
                    BaseColor rowColor = rowCount % 2 == 0 ? BaseColor.WHITE : new BaseColor(245, 245, 245);
                    AddCellToBody(table, urunNo.ToString(), bodyFont, rowColor);
                    AddCellToBody(table, urun.UrunKodu, bodyFont, rowColor);
                    AddCellToBody(table, urun.UrunAciklamasi, bodyFont, rowColor);
                    AddCellToBody(table, urun.Adet.ToString(), bodyFont, rowColor);
                    AddCellToBody(table, FormatPrice(urun.BirimFiyat, currency), bodyFont, rowColor);
                    AddCellToBody(table, FormatPrice(urun.IndirimliFiyat, currency), bodyFont, rowColor);
                    AddCellToBody(table, FormatPrice(urun.Toplam, currency), bodyFont, rowColor);
                    rowCount++;
                    urunNo++;
                }
                table.WriteSelectedRows(0, -1, 22.5f, 520, canvas);

                PdfPTable toplamTable = new PdfPTable(2) { TotalWidth = 240f, DefaultCell = { Border = 0 }, HorizontalAlignment = Element.ALIGN_RIGHT };
                toplamTable.SetWidths(new float[] { 3f, 2f });
                toplamTable.AddCell(CreateRightAlignedHeaderCell($"İndirimli Toplam(%{genelIndirimOrani}):", headerFont));
                toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(toplamFiyat, currency), headerFont));
                toplamTable.AddCell(CreateRightAlignedHeaderCell($"KDV (%{kdvOrani}):", headerFont));
                toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(kdvTutari, currency), headerFont));
                toplamTable.AddCell(CreateRightAlignedHeaderCell("Genel Toplam:", headerFont));
                toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(genelToplam, currency), headerFont));
                toplamTable.WriteSelectedRows(0, -1, 352.5f, 150, canvas);

                // Sözleşme sayfası
                string sozlesmePdfPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\satisSozlesmesi.pdf";
                using var sozlesmeReader = new PdfReader(sozlesmePdfPath);
                int lastPage = reader.NumberOfPages;
                stamper.InsertPage(lastPage + 1, sozlesmeReader.GetPageSizeWithRotation(1));
                PdfContentByte overContent = stamper.GetOverContent(lastPage + 1);
                PdfContentByte underContent = stamper.GetUnderContent(lastPage + 1);
                PdfImportedPage page = stamper.GetImportedPage(sozlesmeReader, 1);
                underContent.AddTemplate(page, 0, 0);
                ColumnText ctSozlesme = new ColumnText(overContent);
                ctSozlesme.SetSimpleColumn(new Phrase(sozlesmeMetni, sozlesmeFont), 40f, 50f, 550f, 700f, 18, Element.ALIGN_LEFT);
                ctSozlesme.Go();

                stamper.Close();
                reader.Close();

                MessageBox.Show("Teklif başarıyla kaydedildi ve PDF oluşturuldu!",
                                "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private static void AddCellToHeader(PdfPTable table, string text, Font font, BaseColor backgroundColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = backgroundColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 5,
                Border = PdfPCell.NO_BORDER
            };
            table.AddCell(cell);
        }

        private static void AddCellToBody(PdfPTable table, string text, Font font, BaseColor backgroundColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = backgroundColor,
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 5,
                Border = PdfPCell.NO_BORDER
            };
            table.AddCell(cell);
        }

        private static PdfPCell CreateRightAlignedHeaderCell(string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = 0, PaddingRight = 5 };
            return cell;
        }

        private static PdfPCell CreateLeftAlignedBodyCell(string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = 0 };
            return cell;
        }
    }
}
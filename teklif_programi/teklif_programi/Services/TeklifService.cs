using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using teklif_programi.Data;
using teklif_programi.Models;
using teklif_programi.Helpers;

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

                Table firmaTable = new Table(FirmaColumnWidths);
                firmaTable.AddCell(CreateRightAlignedHeaderCell("Firma Adı:"));
                firmaTable.AddCell(CreateLeftAlignedBodyCell(firma.FirmaAdi));
                firmaTable.AddCell(CreateRightAlignedHeaderCell("Firma Adresi:"));
                firmaTable.AddCell(CreateLeftAlignedBodyCell(firma.FirmaAdresi));
                firmaTable.AddCell(CreateRightAlignedHeaderCell("Firma Telefonu:"));
                firmaTable.AddCell(CreateLeftAlignedBodyCell(firma.FirmaTelefonu ?? "Belirtilmemiş"));
                firmaTable.AddCell(CreateRightAlignedHeaderCell("Firma E-Posta:"));
                firmaTable.AddCell(CreateLeftAlignedBodyCell(firma.FirmaEposta ?? "Belirtilmemiş"));
                doc.Add(firmaTable);

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

                doc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                doc.Add(new Paragraph("Satış Sözleşmesi")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFont(BoldFont));
                doc.Add(new Paragraph(sozlesmeMetni));

                doc.Close();

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
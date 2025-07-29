using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using teklif_programi.Data;
using teklif_programi.Models;

namespace teklif_programi.view
{
    public partial class TeklifVer : UserControl
    {
        private TeklifDbContext _db = new TeklifDbContext();
        private List<UrunData> secilenUrunler = new List<UrunData>();

        public TeklifVer()
        {
            InitializeComponent();
            dataGridUrunSepeti.ItemsSource = secilenUrunler;
            dataGridUrunListesi.ItemsSource = _db.Urunler.ToList();
            UrunListele();
        }

        private void txtFirmaKodu_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = txtFirmaKodu.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                lblFirmaAdi.Text = "-";
                return;
            }

            // Önce FirmaKoduID ile arama yapmayı dene
            if (int.TryParse(searchText, out int firmaKodu))
            {
                var firma = _db.Firmalar.FirstOrDefault(f => f.FirmaKoduID == firmaKodu);
                lblFirmaAdi.Text = firma != null ? firma.FirmaAdi : "-";
            }
            else
            {
                // FirmaKoduID bir sayı değilse, FirmaAdi ile arama yap
                var firma = _db.Firmalar.FirstOrDefault(f => f.FirmaAdi.ToLower().Contains(searchText.ToLower()));
                lblFirmaAdi.Text = firma != null ? firma.FirmaAdi : "-";
            }   
        }


        private void BtnSepeteEkle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is UrunData urunData)
            {
                AddUrunToSepet(urunData);
            }
            else
            {
                MessageBox.Show("Ürün seçilemedi.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddUrunToSepet(UrunData urunData)
        {
            var sepettekiUrun = secilenUrunler.FirstOrDefault(u => u.UrunKoduID == urunData.UrunKoduID);
            if (sepettekiUrun != null)
            {
                sepettekiUrun.Adet++; // INotifyPropertyChanged ile SatisToplamFiyati ve ToplamFiyat güncellenir
            }
            else
            {
                secilenUrunler.Add(new UrunData
                {
                    UrunKoduID = urunData.UrunKoduID,
                    Kategori = urunData.Kategori,
                    Aciklama = urunData.Aciklama,
                    BirimSatisFiyati = urunData.BirimSatisFiyati,
                    YurticiMaliyet = urunData.YurticiMaliyet,
                    Adet = 1,
                    SatisToplamFiyati = urunData.BirimSatisFiyati,
                    ToplamFiyat = urunData.BirimSatisFiyati
                });
            }
            dataGridUrunSepeti.Items.Refresh();
        }

        private void BtnAdetArttir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is UrunData urun)
            {
                urun.Adet++; // INotifyPropertyChanged otomatik günceller
                dataGridUrunSepeti.Items.Refresh();
            }
        }

        private void BtnAdetAzalt_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is UrunData urun)
            {
                if (urun.Adet > 1)
                {
                    urun.Adet--; // INotifyPropertyChanged otomatik günceller
                    dataGridUrunSepeti.Items.Refresh();
                }
            }
        }

        private void BtnUrunSil_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is UrunData urun)
            {
                secilenUrunler.Remove(urun);
                dataGridUrunSepeti.Items.Refresh();
            }
        }

        private void AdetTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void BtnTeklifOlusturVePdfIndir_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirmaKodu.Text) || lblFirmaAdi.Text == "-" || secilenUrunler.Count == 0)
            {
                MessageBox.Show("Lütfen geçerli bir firma seçin ve en az bir ürün ekleyin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int firmaKodu;
            if (!int.TryParse(txtFirmaKodu.Text, out firmaKodu))
            {
                MessageBox.Show("Firma kodu geçerli bir sayı olmalıdır.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int personelKodu = 1; // Sabit personel kodu, dinamik yapılabilir

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
                    ToplamFiyat = urun.SatisToplamFiyati
                };
                _db.TeklifDetaylari.Add(detay);
            }

            _db.SaveChanges();

            string templatePath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\LiyaTeklifBelgesi.pdf";

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF dosyası (*.pdf)|*.pdf",
                FileName = $"Teklif_{lblFirmaAdi.Text}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf" // 20250729_101700 gibi
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
                        var tableHeaderFont = new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK);
                        var tableBodyFont = new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK);

                        int currentPage = 2;
                        PdfContentByte canvas = stamper.GetOverContent(currentPage);
                        ColumnText ct = new ColumnText(canvas);

                        Phrase firmaBilgileri = new Phrase();
                        firmaBilgileri.Add(new Chunk("Firma Kodu: ", new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK)));
                        firmaBilgileri.Add(new Chunk(txtFirmaKodu.Text + "\n", new Font(baseFont, 10, Font.NORMAL, new BaseColor(80, 80, 80))));

                        firmaBilgileri.Add(new Chunk("Firma Ad: ", new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK)));
                        firmaBilgileri.Add(new Chunk(lblFirmaAdi.Text + "\n", new Font(baseFont, 10, Font.NORMAL, new BaseColor(80, 80, 80))));

                        firmaBilgileri.Add(new Chunk("Tarih: ", new Font(baseFont, 10, Font.BOLD, BaseColor.BLACK)));
                        firmaBilgileri.Add(new Chunk(DateTime.Now.ToString("dd.MM.yyyy HH:mm"), new Font(baseFont, 10, Font.NORMAL, new BaseColor(80, 80, 80))));

                        ct.SetSimpleColumn(firmaBilgileri, 50, 650, 550, 600, 15, Element.ALIGN_LEFT);
                        ct.Go();

                        Phrase urunBaslik = new Phrase("Teklif Edilen Ürünler", new Font(baseFont, 12, Font.BOLD, BaseColor.BLACK));
                        ct.SetSimpleColumn(urunBaslik, 50, 580, 550, 560, 15, Element.ALIGN_LEFT);
                        ct.Go();

                        PdfPTable table = new PdfPTable(6);
                        table.TotalWidth = 500f;
                        table.LockedWidth = true;
                        float[] widths = { 2f, 2f, 3f, 1f, 2f, 2f };
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
                            AddCellToBody(table, urun.SatisToplamFiyati.ToString("C2"), tableBodyFont, rowColor);
                            rowCount++;
                        }

                        ct = new ColumnText(canvas);
                        ct.AddElement(table);
                        ct.SetSimpleColumn(50, 540, 550, 200, 15, Element.ALIGN_LEFT);
                        int status = ct.Go();

                        while (ColumnText.HasMoreText(status))
                        {
                            currentPage++;
                            stamper.InsertPage(currentPage, PageSize.A4);
                            PdfContentByte newCanvas = stamper.GetOverContent(currentPage);

                            PdfImportedPage templatePage = stamper.GetImportedPage(reader, 2);
                            newCanvas.AddTemplate(templatePage, 0, 0);

                            ct = new ColumnText(newCanvas);
                            ct.SetSimpleColumn(50, 800, 550, 200, 15, Element.ALIGN_LEFT);
                            status = ct.Go();

                            canvas = newCanvas;
                        }

                        canvas.BeginText();
                        canvas.SetFontAndSize(baseFont, 12);
                        canvas.SetColorFill(BaseColor.BLACK);
                        canvas.ShowTextAligned(
                            Element.ALIGN_RIGHT,
                            $"Toplam Teklif Tutarı: {ToplamFiyatHesapla(secilenUrunler):C2}",
                            550f, 60f,
                            0
                        );
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

        private decimal ToplamFiyatHesapla(List<UrunData> secilenUrunler)
        {
            return secilenUrunler.Sum(u => u.SatisToplamFiyati);
        }

        private void txtUrunFiltrele_TextChanged(object sender, TextChangedEventArgs e)
        {
            UrunListele(txtUrunFiltrele.Text.Trim());
        }

        private void UrunListele(string arama = "")
        {
            var urunler = string.IsNullOrWhiteSpace(arama)
                ? _db.Urunler.ToList()
                : _db.Urunler
                    .Where(u => u.UrunKoduID.ToLower().Contains(arama.ToLower()) ||
                                u.Aciklama.ToLower().Contains(arama.ToLower()))
                    .ToList();

            dataGridUrunListesi.ItemsSource = urunler;
        }

        private void dataGridUrunListesi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
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

            // Burada Urun entity'sinden UrunData oluşturuyoruz:
            UrunData yeniUrun = new UrunData
            {
                UrunKoduID = urun.UrunKoduID,
                Kategori = urun.Kategori,
                Aciklama = urun.Aciklama,
                Adet = 1, // Başlangıçta 1, istersen kullanıcıdan al
                BirimSatisFiyati = urun.BirimSatisFiyati,
                SatisToplamFiyati = urun.BirimSatisFiyati * 1,
                YurticiMaliyet = urun.YurticiMaliyet,
                ToplamFiyat = urun.BirimSatisFiyati * 1
            };

            secilenUrunler.Add(yeniUrun);

            dataGridTeklifUrunler.ItemsSource = null; // listeyi resetle
            dataGridTeklifUrunler.ItemsSource = secilenUrunler;
        }

        private void BtnTeklifOlusturVePdfIndir_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFirmaKodu.Text) || lblFirmaAdi.Text == "-" || secilenUrunler.Count == 0)
            {
                MessageBox.Show("Lütfen geçerli bir firma seçin ve en az bir ürün ekleyin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Teklif tablosuna yeni kayıt
            int firmaKodu = int.Parse(txtFirmaKodu.Text);
            int personelKodu = 1; // Giriş yapan personel ID’si buraya gelmeli

            var yeniTeklif = new Teklif
            {
                FirmaKoduID = firmaKodu,
                PersonelKoduID = personelKodu,
                TeklifTarihi = DateTime.Now,
                ToplamTutar = ToplamFiyatHesapla(secilenUrunler)
            };

            _db.Teklifler.Add(yeniTeklif);
            _db.SaveChanges(); // burada TeklifNoID oluşur

            // 2. TeklifDetaylari tablosuna ürünleri kaydet
            foreach (var urun in secilenUrunler)
            {
                var detay = new TeklifDetay
                {
                    TeklifNoID = yeniTeklif.TeklifNoID, // yeni oluşan teklifin ID’si
                    UrunKoduID = urun.UrunKoduID,
                    Adet = urun.Adet,
                    BirimFiyat = urun.BirimSatisFiyati,
                    ToplamFiyat = urun.BirimSatisFiyati * urun.Adet
                };
                _db.TeklifDetaylari.Add(detay);
            }

            _db.SaveChanges(); // detayları kaydet


            // SaveFileDialog ile kullanıcıdan kaydetme yeri ve ismi al
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF dosyası (*.pdf)|*.pdf";
            saveFileDialog.FileName = $"Teklif_{lblFirmaAdi.Text}_{DateTime.Now:yyyyMMdd}.pdf";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    Document document = new Document(PageSize.A4, 50, 50, 25, 25);
                    PdfWriter.GetInstance(document, new FileStream(saveFileDialog.FileName, FileMode.Create));
                    document.Open();

                    // Başlık
                    var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD, BaseColor.BLUE);
                    Paragraph title = new Paragraph("VOLISSE Teklif Belgesi", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    title.SpacingAfter = 20f;
                    document.Add(title);

                    // Firma bilgileri
                    var firmaFont = FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.BLACK);
                    Paragraph firmaBilgileri = new Paragraph($"Firma Kodu: {txtFirmaKodu.Text}\nFirma Adı: {lblFirmaAdi.Text}\nTarih: {DateTime.Now.ToShortDateString()}", firmaFont);
                    firmaBilgileri.SpacingAfter = 20f;
                    document.Add(firmaBilgileri);

                    // Ürün tablosu
                    PdfPTable table = new PdfPTable(6);
                    table.WidthPercentage = 100;
                    float[] widths = new float[] { 15f, 25f, 10f, 15f, 15f, 20f };
                    table.SetWidths(widths);

                    // Tablo başlıkları
                    AddCellToHeader(table, "Ürün Kodu");
                    AddCellToHeader(table, "Açıklama");
                    AddCellToHeader(table, "Adet");
                    AddCellToHeader(table, "Birim Fiyatı");
                    AddCellToHeader(table, "Toplam Fiyat");
                    AddCellToHeader(table, "Kategori");

                    // Ürünleri tabloya ekle
                    foreach (var urun in secilenUrunler)
                    {
                        AddCellToBody(table, urun.UrunKoduID);
                        AddCellToBody(table, urun.Aciklama);
                        AddCellToBody(table, urun.Adet.ToString());
                        AddCellToBody(table, urun.BirimSatisFiyati.ToString("C2"));
                        AddCellToBody(table, (urun.BirimSatisFiyati * urun.Adet).ToString("C2"));
                        AddCellToBody(table, urun.Kategori);
                    }

                    document.Add(table);

                    // Toplam fiyat
                    Paragraph toplamFiyatParagraf = new Paragraph($"\nToplam Teklif Tutarı: {ToplamFiyatHesapla(secilenUrunler).ToString("C2")}", firmaFont);
                    toplamFiyatParagraf.Alignment = Element.ALIGN_RIGHT;
                    toplamFiyatParagraf.SpacingBefore = 20f;
                    document.Add(toplamFiyatParagraf);

                    document.Close();

                    MessageBox.Show("Teklif PDF dosyası başarıyla oluşturuldu.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("PDF oluşturulurken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

        private decimal ToplamFiyatHesapla(List<UrunData> secilenUrunler)
        {
            return secilenUrunler.Sum(u => u.BirimSatisFiyati * u.Adet);
        }

        private void AddCellToHeader(PdfPTable table, string text)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text));
            cell.BackgroundColor = BaseColor.GRAY;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 5;
            table.AddCell(cell);
        }

        private void AddCellToBody(PdfPTable table, string text)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Padding = 5;
            table.AddCell(cell);
        }
    }
}

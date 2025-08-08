// Gerekli isim alanları: MVVM, veritabanı, PDF oluşturma ve UI için
using CommunityToolkit.Mvvm.Input;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using teklif_programi.Data;
using teklif_programi.Models;

// ViewModel sınıflarının isim alanı
namespace teklif_programi.ViewModels
{
    // TeklifDetayViewModel: Teklif detaylarını yöneten ve UI ile bağlayan ViewModel
    public class TeklifDetayViewModel : INotifyPropertyChanged
    {
        // _context: Veritabanı bağlantısı için DbContext
        private readonly TeklifDbContext _context;
        // _teklif: Güncel teklif nesnesi
        private Teklif _teklif;
        // _teklifUrunler: Teklifteki ürünlerin listesi
        private ObservableCollection<TeklifUrunModel> _teklifUrunler = new();
        // _teklifToplam: Teklifin toplam bilgileri
        private TeklifToplam? _teklifToplam;
        // _durumlar: Teklif durumlarının listesi
        private ObservableCollection<string> _durumlar = new();

        // Kurucu: Teklif nesnesini alır, DbContext ve durumlar başlatılır, detaylar yüklenir
        public TeklifDetayViewModel(Teklif teklif)
        {
            _context = new TeklifDbContext();
            _teklif = teklif ?? throw new ArgumentNullException(nameof(teklif));
            TeklifUrunler = new ObservableCollection<TeklifUrunModel>();
            Durumlar = new ObservableCollection<string> { "Beklemede", "Kabul Edildi", "Reddedildi" };
            YukleTeklifDetaylari(); // Teklif detaylarını yükler
            KaydetCommand = new RelayCommand(Kaydet, CanKaydet); // Kaydet komutu
            PdfIndirCommand = new RelayCommand(PdfIndir, CanPdfIndir); // PDF indirme komutu
        }

        // Teklif: Güncel teklif nesnesi, UI ile bağlı
        public Teklif Teklif
        {
            get => _teklif;
            set { _teklif = value; OnPropertyChanged(); }
        }

        // TeklifUrunler: Teklifteki ürünlerin ObservableCollection’ı, UI ile bağlı
        public ObservableCollection<TeklifUrunModel> TeklifUrunler
        {
            get => _teklifUrunler;
            set { _teklifUrunler = value; OnPropertyChanged(); }
        }

        // TeklifToplam: Teklifin toplam bilgileri, UI ile bağlı
        public TeklifToplam? TeklifToplam
        {
            get => _teklifToplam;
            set { _teklifToplam = value; OnPropertyChanged(); }
        }

        // Durumlar: Teklif durumlarının ObservableCollection’ı, UI ile bağlı
        public ObservableCollection<string> Durumlar
        {
            get => _durumlar;
            set { _durumlar = value; OnPropertyChanged(); }
        }

        // KaydetCommand: Değişiklikleri kaydetmek için komut
        public RelayCommand KaydetCommand { get; }
        // PdfIndirCommand: Teklif detaylarını PDF olarak indirmek için komut
        public RelayCommand PdfIndirCommand { get; }

        // YukleTeklifDetaylari: Teklif ürünlerini ve toplamlarını veritabanından yükler
        private void YukleTeklifDetaylari()
        {
            try
            {
                var urunler = _context.TeklifUrunleri
                    .Include(tu => tu.Urun)
                    .Where(tu => tu.TeklifId == Teklif.TeklifId)
                    .ToList(); // Teklife ait ürünleri çeker
                TeklifUrunler.Clear();
                foreach (var urun in urunler)
                {
                    var model = new TeklifUrunModel
                    {
                        UrunId = urun.UrunId,
                        UrunKodu = urun.Urun.UrunKodu,
                        UrunAciklamasi = urun.Urun.UrunAciklamasi,
                        Adet = urun.Adet,
                        BirimFiyat = urun.BirimFiyat,
                        IndirimliFiyat = urun.IndirimliBirimFiyat
                    };
                    model.OnBirimFiyatDegisti += TeklifUrunDegisti; // Ürün değişim olayını bağlar
                    TeklifUrunler.Add(model);
                }
                // Teklif toplamını çeker veya varsayılan değer oluşturur
                TeklifToplam = _context.TeklifToplamlari.FirstOrDefault(tt => tt.TeklifId == Teklif.TeklifId)
                    ?? new TeklifToplam { TeklifId = Teklif.TeklifId, IndirimliToplam = 0, KdvTutari = 0, GenelToplam = 0 };
                HesaplaToplamlar(); // Toplamları hesaplar
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Teklif detayları yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // TeklifUrunDegisti: Ürün fiyat/adet değiştiğinde toplamları günceller
        private void TeklifUrunDegisti(object? sender, EventArgs e)
        {
            HesaplaToplamlar();
        }

        // HesaplaToplamlar: Teklif toplamlarını hesaplar (indirimli toplam, KDV, genel toplam)
        private void HesaplaToplamlar()
        {
            if (TeklifToplam == null) return;
            TeklifToplam.IndirimliToplam = TeklifUrunler.Sum(u => u.Toplam);
            TeklifToplam.KdvTutari = TeklifToplam.IndirimliToplam * (Teklif.KdvOrani / 100);
            TeklifToplam.GenelToplam = TeklifToplam.IndirimliToplam + TeklifToplam.KdvTutari;
            OnPropertyChanged(nameof(TeklifToplam)); // UI’yi günceller
        }

        // CanKaydet: Kaydet komutunun çalışabilirliğini kontrol eder
        private bool CanKaydet() => Teklif != null;

        // Kaydet: Teklif ve ürün değişikliklerini veritabanına kaydeder
        private void Kaydet()
        {
            if (Teklif == null) return;
            try
            {
                using var transaction = _context.Database.BeginTransaction();
                var dbTeklif = _context.Teklifler.Find(Teklif.TeklifId);
                if (dbTeklif != null)
                {
                    dbTeklif.Durum = Teklif.Durum;
                    dbTeklif.MusteriNotu = Teklif.MusteriNotu; // Teklif durum ve notunu günceller
                }
                foreach (var urunModel in TeklifUrunler)
                {
                    var dbUrun = _context.TeklifUrunleri
                        .FirstOrDefault(tu => tu.TeklifId == Teklif.TeklifId && tu.UrunId == urunModel.UrunId);
                    if (dbUrun != null)
                    {
                        dbUrun.Adet = urunModel.Adet;
                        dbUrun.BirimFiyat = urunModel.BirimFiyat;
                        dbUrun.IndirimliBirimFiyat = urunModel.IndirimliFiyat;
                        dbUrun.ToplamTutar = urunModel.Toplam; // Ürün detaylarını günceller
                    }
                }
                var dbToplam = _context.TeklifToplamlari
                    .FirstOrDefault(tt => tt.TeklifId == Teklif.TeklifId);
                if (dbToplam != null)
                {
                    if (TeklifToplam != null)
                    {
                        dbToplam.IndirimliToplam = TeklifToplam.IndirimliToplam;
                        dbToplam.KdvTutari = TeklifToplam.KdvTutari;
                        dbToplam.GenelToplam = TeklifToplam.GenelToplam; // Toplamları günceller
                    }
                }
                else
                {
                    _context.TeklifToplamlari.Add(new TeklifToplam
                    {
                        TeklifId = Teklif.TeklifId,
                        IndirimliToplam = TeklifToplam?.IndirimliToplam ?? 0,
                        KdvTutari = TeklifToplam?.KdvTutari ?? 0,
                        GenelToplam = TeklifToplam?.GenelToplam ?? 0 // Yeni toplam kaydı ekler
                    });
                }
                _context.SaveChanges(); // Veritabanına kaydeder
                transaction.Commit(); // İşlemi tamamlar
                MessageBox.Show("Değişiklikler kaydedildi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Değişiklikler kaydedilirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // CanPdfIndir: PDF indirme komutunun çalışabilirliğini kontrol eder
        private bool CanPdfIndir() => Teklif != null;

        // PdfIndir: Teklif detaylarını PDF olarak kaydeder
        private void PdfIndir()
        {
            if (Teklif == null) return;
            try
            {
                SaveFileDialog saveFileDialog = new()
                {
                    Filter = "PDF Dosyaları (*.pdf)|*.pdf",
                    FileName = $"Teklif_{Teklif.Musteri.FirmaAdi}_{Teklif.OlusturmaTarihi:yyyyMMdd}.pdf"
                }; // PDF dosya adı oluşturur
                if (saveFileDialog.ShowDialog() != true) return;
                string dosyaYolu = saveFileDialog.FileName;
                using var fs = new FileStream(dosyaYolu, FileMode.Create);
                Document pdfDoc = new Document(PageSize.A4, 50, 50, 50, 50); // A4 boyutunda PDF oluşturur
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, fs);
                pdfDoc.Open();
                var titleFont = FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD);
                pdfDoc.Add(new Paragraph($"Teklif Detayları - {Teklif.Musteri.FirmaAdi}", titleFont)); // Başlık ekler
                pdfDoc.Add(new Paragraph($"Tarih: {Teklif.OlusturmaTarihi:dd.MM.yyyy}")); // Tarih ekler
                pdfDoc.Add(new Paragraph($"Durum: {Teklif.Durum}")); // Durum ekler
                pdfDoc.Add(new Paragraph($"Müşteri Notu: {Teklif.MusteriNotu ?? "-"}")); // Not ekler
                pdfDoc.Add(new Paragraph("\n"));
                PdfPTable table = new PdfPTable(6) { WidthPercentage = 100 }; // 6 sütunlu tablo
                table.SetWidths(new float[] { 2f, 5f, 1f, 2f, 2f, 2f }); // Sütun genişlikleri
                AddCellToHeader(table, "Ürün Kodu"); // Tablo başlıkları
                AddCellToHeader(table, "Açıklama");
                AddCellToHeader(table, "Adet");
                AddCellToHeader(table, "Birim Fiyat");
                AddCellToHeader(table, $"İndirimli Fiyat (%{Teklif.GenelIndirimOrani})");
                AddCellToHeader(table, "Toplam");
                foreach (var urun in TeklifUrunler)
                {
                    AddCellToBody(table, urun.UrunKodu); // Ürün detaylarını tabloya ekler
                    AddCellToBody(table, urun.UrunAciklamasi);
                    AddCellToBody(table, urun.Adet.ToString());
                    AddCellToBody(table, urun.BirimFiyat.ToString("C2"));
                    AddCellToBody(table, urun.IndirimliFiyat.ToString("C2"));
                    AddCellToBody(table, urun.Toplam.ToString("C2"));
                }
                pdfDoc.Add(table); // Tabloyu PDF’ye ekler
                pdfDoc.Add(new Paragraph("\n"));
                if (TeklifToplam != null)
                {
                    pdfDoc.Add(new Paragraph($"İndirimli Toplam: {TeklifToplam.IndirimliToplam:C2}")); // Toplamları ekler
                    pdfDoc.Add(new Paragraph($"KDV Tutarı: {TeklifToplam.KdvTutari:C2}"));
                    pdfDoc.Add(new Paragraph($"Genel Toplam: {TeklifToplam.GenelToplam:C2}"));
                }
                pdfDoc.Close(); // PDF’yi kapatır
                MessageBox.Show("PDF başarıyla oluşturuldu.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF oluşturulurken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // AddCellToHeader: PDF tablosuna başlık hücresi ekler
        private static void AddCellToHeader(PdfPTable table, string text)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text))
            {
                BackgroundColor = new BaseColor(240, 240, 240),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5
            };
            table.AddCell(cell);
        }

        // AddCellToBody: PDF tablosuna veri hücresi ekler
        private void AddCellToBody(PdfPTable table, string text)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5
            };
            table.AddCell(cell);
        }

        // PropertyChanged: UI veri bağlama için özellik değişim olayı
        public event PropertyChangedEventHandler? PropertyChanged;
        // OnPropertyChanged: Özellik değiştiğinde UI’yi günceller
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}
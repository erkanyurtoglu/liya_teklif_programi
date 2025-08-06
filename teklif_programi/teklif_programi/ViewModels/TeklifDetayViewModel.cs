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

namespace teklif_programi.ViewModels
{
    public class TeklifDetayViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context;
        private Teklif _teklif;
        private ObservableCollection<TeklifUrunModel> _teklifUrunler;
        private TeklifToplam? _teklifToplam;
        private ObservableCollection<string> _durumlar;

        public TeklifDetayViewModel(Teklif teklif)
        {
            _context = new TeklifDbContext();
            _teklif = teklif ?? throw new ArgumentNullException(nameof(teklif));
            TeklifUrunler = new ObservableCollection<TeklifUrunModel>();
            Durumlar = new ObservableCollection<string> { "Beklemede", "Kabul Edildi", "Reddedildi" };

            YukleTeklifDetaylari();

            KaydetCommand = new RelayCommand(Kaydet, CanKaydet);
            PdfIndirCommand = new RelayCommand(PdfIndir, CanPdfIndir);
        }

        public Teklif Teklif
        {
            get => _teklif;
            set { _teklif = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TeklifUrunModel> TeklifUrunler
        {
            get => _teklifUrunler;
            set { _teklifUrunler = value; OnPropertyChanged(); }
        }

        public TeklifToplam? TeklifToplam
        {
            get => _teklifToplam;
            set { _teklifToplam = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Durumlar
        {
            get => _durumlar;
            set { _durumlar = value; OnPropertyChanged(); }
        }

        public RelayCommand KaydetCommand { get; }
        public RelayCommand PdfIndirCommand { get; }

        private void YukleTeklifDetaylari()
        {
            try
            {
                var urunler = _context.TeklifUrunleri
                    .Include(tu => tu.Urun)
                    .Where(tu => tu.TeklifId == Teklif.TeklifId)
                    .ToList();

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
                    model.OnBirimFiyatDegisti += TeklifUrunDegisti;
                    TeklifUrunler.Add(model);
                }

                TeklifToplam = _context.TeklifToplamlari.FirstOrDefault(tt => tt.TeklifId == Teklif.TeklifId);
                if (TeklifToplam == null)
                {
                    TeklifToplam = new TeklifToplam
                    {
                        TeklifId = Teklif.TeklifId,
                        IndirimliToplam = 0,
                        KdvTutari = 0,
                        GenelToplam = 0
                    };
                }

                HesaplaToplamlar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Teklif detayları yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Ürün adet/fiyat değiştiğinde toplamları güncelle
        private void TeklifUrunDegisti(object? sender, EventArgs e)
        {
            HesaplaToplamlar();
        }

        private void HesaplaToplamlar()
        {
            if (TeklifToplam == null) return;


            TeklifToplam.IndirimliToplam = TeklifUrunler.Sum(u => u.Toplam);
            TeklifToplam.KdvTutari = TeklifToplam.IndirimliToplam * (Teklif.KdvOrani / 100);
            TeklifToplam.GenelToplam = TeklifToplam.IndirimliToplam + TeklifToplam.KdvTutari;

            OnPropertyChanged(nameof(TeklifToplam));
        }

        private bool CanKaydet() => Teklif != null;
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
                    dbTeklif.MusteriNotu = Teklif.MusteriNotu;
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
                        dbUrun.ToplamTutar = urunModel.Toplam;
                    }
                }

                var dbToplam = _context.TeklifToplamlari
                    .FirstOrDefault(tt => tt.TeklifId == Teklif.TeklifId);

                if (dbToplam != null)
                {
                    dbToplam.IndirimliToplam = TeklifToplam.IndirimliToplam;
                    dbToplam.KdvTutari = TeklifToplam.KdvTutari;
                    dbToplam.GenelToplam = TeklifToplam.GenelToplam;
                }
                else
                {
                    // Yoksa yeni oluştur
                    _context.TeklifToplamlari.Add(new TeklifToplam
                    {
                        TeklifId = Teklif.TeklifId,
                        IndirimliToplam = TeklifToplam.IndirimliToplam,
                        KdvTutari = TeklifToplam.KdvTutari,
                        GenelToplam = TeklifToplam.GenelToplam
                    });
                }

                _context.SaveChanges();
                transaction.Commit();

                MessageBox.Show("Değişiklikler kaydedildi!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Değişiklikler kaydedilirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanPdfIndir() => Teklif != null;
        private void PdfIndir()
        {
            if (Teklif == null) return;

            try
            {
                // Pdf oluşturma kodun burada...
                // Önceki PdfIndir metodunuzu buraya aynen alabilirsiniz.
                // TeklifUrunler ve TeklifToplam en güncel haliyle olacak.

                // Örnek kısım:
                SaveFileDialog saveFileDialog = new()
                {
                    Filter = "PDF Dosyaları (*.pdf)|*.pdf",
                    FileName = $"Teklif_{Teklif.Musteri.FirmaAdi}_{Teklif.OlusturmaTarihi:yyyyMMdd}.pdf"
                };

                if (saveFileDialog.ShowDialog() != true) return;

                string dosyaYolu = saveFileDialog.FileName;

                using var fs = new FileStream(dosyaYolu, FileMode.Create);
                Document pdfDoc = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, fs);
                pdfDoc.Open();

                // Başlık
                var titleFont = FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD);
                pdfDoc.Add(new Paragraph($"Teklif Detayları - {Teklif.Musteri.FirmaAdi}", titleFont));

                pdfDoc.Add(new Paragraph($"Tarih: {Teklif.OlusturmaTarihi:dd.MM.yyyy}"));
                pdfDoc.Add(new Paragraph($"Durum: {Teklif.Durum}"));
                pdfDoc.Add(new Paragraph($"Müşteri Notu: {Teklif.MusteriNotu ?? "-"}"));
                pdfDoc.Add(new Paragraph("\n"));

                // Ürün tablosu
                PdfPTable table = new PdfPTable(6);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 5f, 1f, 2f, 2f, 2f });

                // Başlık hücreleri
                AddCellToHeader(table, "Ürün Kodu");
                AddCellToHeader(table, "Açıklama");
                AddCellToHeader(table, "Adet");
                AddCellToHeader(table, "Birim Fiyat");
                AddCellToHeader(table, $"İndirimli Fiyat (%{Teklif.GenelIndirimOrani})");
                AddCellToHeader(table, "Toplam");

                foreach (var urun in TeklifUrunler)
                {
                    AddCellToBody(table, urun.UrunKodu);
                    AddCellToBody(table, urun.UrunAciklamasi);
                    AddCellToBody(table, urun.Adet.ToString());
                    AddCellToBody(table, urun.BirimFiyat.ToString("C2"));
                    AddCellToBody(table, urun.IndirimliFiyat.ToString("C2"));
                    AddCellToBody(table, urun.Toplam.ToString("C2"));
                }

                pdfDoc.Add(table);
                pdfDoc.Add(new Paragraph("\n"));

                // Toplamlar
                pdfDoc.Add(new Paragraph($"İndirimli Toplam: {TeklifToplam.IndirimliToplam:C2}"));
                pdfDoc.Add(new Paragraph($"KDV Tutarı: {TeklifToplam.KdvTutari:C2}"));
                pdfDoc.Add(new Paragraph($"Genel Toplam: {TeklifToplam.GenelToplam:C2}"));

                pdfDoc.Close();

                MessageBox.Show("PDF başarıyla oluşturuldu.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF oluşturulurken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCellToHeader(PdfPTable table, string text)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text))
            {
                BackgroundColor = new BaseColor(240, 240, 240),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5
            };
            table.AddCell(cell);
        }

        private void AddCellToBody(PdfPTable table, string text)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5
            };
            table.AddCell(cell);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}

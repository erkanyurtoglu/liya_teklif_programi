using teklif_programi.Helpers;
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
using System.Globalization;

namespace teklif_programi.ViewModels
{
    public class TeklifDetayViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context;
        private Teklif _teklif;
        private ObservableCollection<TeklifUrunModel> _teklifUrunler = new();
        private TeklifToplam? _teklifToplam;
        private ObservableCollection<string> _durumlar = new();
        private ObservableCollection<string> _paraBirimiListe = new();

        public TeklifDetayViewModel(Teklif teklif)
        {
            _context = new TeklifDbContext();
            _teklif = teklif ?? throw new ArgumentNullException(nameof(teklif));
            TeklifUrunler = new ObservableCollection<TeklifUrunModel>();
            Durumlar = new ObservableCollection<string> { "Beklemede", "Kabul Edildi", "Reddedildi" };
            ParaBirimiListe = new ObservableCollection<string> { "TL", "USD", "EUR" };

            if (string.IsNullOrEmpty(_teklif.ParaBirimi))
            {
                _teklif.ParaBirimi = "TL";
            }

            YukleTeklifDetaylari();
            KaydetCommand = new RelayCommand(Kaydet, CanKaydet);
            PdfIndirCommand = new RelayCommand(PdfIndir, CanPdfIndir);
        }

        public Teklif Teklif
        {
            get => _teklif;
            set
            {
                _teklif = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PersonelAdiSoyadi));
                OnPropertyChanged(nameof(SelectedCurrency));
                RecalculateAll();
            }
        }

        public ObservableCollection<TeklifUrunModel> TeklifUrunler
        {
            get => _teklifUrunler;
            set
            {
                _teklifUrunler = value;
                OnPropertyChanged();
            }
        }

        public TeklifToplam? TeklifToplam
        {
            get => _teklifToplam;
            set
            {
                _teklifToplam = value;
                OnPropertyChanged();
                UpdateToplamlarText();
            }
        }

        public ObservableCollection<string> Durumlar
        {
            get => _durumlar;
            set { _durumlar = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> ParaBirimiListe
        {
            get => _paraBirimiListe;
            set { _paraBirimiListe = value; OnPropertyChanged(); }
        }

        public string SelectedCurrency
        {
            get => _teklif.ParaBirimi ?? "TL";
            set
            {
                if (_teklif.ParaBirimi != value)
                {
                    _teklif.ParaBirimi = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Teklif));
                    RecalculateAll();
                }
            }
        }

        public string IndirimliToplamText { get; set; }
        public string KdvTutariText { get; set; }
        public string GenelToplamText { get; set; }

        public RelayCommand KaydetCommand { get; }
        public RelayCommand PdfIndirCommand { get; }

        public string PersonelAdiSoyadi => Teklif?.Personel != null
            ? $"{Teklif.Personel.AdSoyad}"
            : "Personel bilgisi yok";

        private void YukleTeklifDetaylari()
        {
            try
            {
                var urunler = _context.TeklifUrunleri
                    .Include(tu => tu.Urun)
                    .Where(tu => tu.TeklifId == Teklif.TeklifId)
                    .ToList() ?? new System.Collections.Generic.List<TeklifUrun>();

                TeklifUrunler.Clear();
                foreach (var urun in urunler)
                {
                    var model = new TeklifUrunModel
                    {
                        UrunId = urun.UrunId,
                        UrunKodu = urun.Urun?.UrunKodu ?? "Bilinmiyor",
                        UrunAciklamasi = urun.Urun?.UrunAciklamasi ?? "Bilinmiyor",
                        Adet = urun.Adet,
                        BirimFiyat = urun.BirimFiyat,
                        IndirimliFiyat = urun.IndirimliBirimFiyat,
                        FiyatTL = urun.Urun?.FiyatTL ?? 0,
                        FiyatUSD = urun.Urun?.FiyatUSD ?? 0,
                        FiyatEUR = urun.Urun?.FiyatEUR ?? 0,
                        BirimFiyatText = FormatPrice(urun.BirimFiyat),
                        IndirimliFiyatText = FormatPrice(urun.IndirimliBirimFiyat),
                        ToplamText = FormatPrice(urun.Adet * urun.IndirimliBirimFiyat)
                    };
                    model.OnBirimFiyatDegisti += Model_OnBirimFiyatDegisti;
                    model.PropertyChanged += Model_PropertyChanged; // Adet ve IndirimliFiyat değişikliklerini yakala
                    TeklifUrunler.Add(model);
                }

                TeklifToplam = _context.TeklifToplamlari.FirstOrDefault(tt => tt.TeklifId == Teklif.TeklifId)
                    ?? new TeklifToplam { TeklifId = Teklif.TeklifId, IndirimliToplam = 0, KdvTutari = 0, GenelToplam = 0 };

                var teklifWithRelations = _context.Teklifler
                    .Include(t => t.Personel)
                    .Include(t => t.Musteri)
                    .FirstOrDefault(t => t.TeklifId == Teklif.TeklifId);

                if (teklifWithRelations != null)
                {
                    Teklif = teklifWithRelations;
                }

                RecalculateAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Teklif detayları yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Model_OnBirimFiyatDegisti(object? sender, EventArgs e)
        {
            if (sender is TeklifUrunModel model)
            {
                model.BirimFiyatText = FormatPrice(model.BirimFiyat);
                model.IndirimliFiyatText = FormatPrice(model.IndirimliFiyat);
                model.ToplamText = FormatPrice(model.Toplam);
            }
            UpdateToplamlarText();
        }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TeklifUrunModel && (e.PropertyName == nameof(TeklifUrunModel.Adet) || e.PropertyName == nameof(TeklifUrunModel.IndirimliFiyat)))
            {
                UpdateToplamlarText();
            }
        }

        private void RecalculateAll()
        {
            foreach (var urun in TeklifUrunler)
            {
                var dbUrun = _context.Urunler.FirstOrDefault(u => u.UrunId == urun.UrunId);
                if (dbUrun != null)
                {
                    urun.BirimFiyat = GetFiyatByCurrency(dbUrun, Teklif.ParaBirimi ?? "TL");
                    urun.IndirimliFiyat = urun.BirimFiyat * (1 - Teklif.GenelIndirimOrani / 100);
                }
                else
                {
                    urun.BirimFiyat = 0;
                    urun.IndirimliFiyat = 0;
                    urun.BirimFiyatText = FormatPrice(0);
                    urun.IndirimliFiyatText = FormatPrice(0);
                    urun.ToplamText = FormatPrice(0);
                }
            }
            UpdateToplamlarText();
        }

        private void UpdateToplamlarText()
        {
            if (TeklifToplam != null)
            {
                TeklifToplam.IndirimliToplam = TeklifUrunler.Sum(u => u.Toplam);
                TeklifToplam.KdvTutari = TeklifToplam.IndirimliToplam * (Teklif.KdvOrani / 100);
                TeklifToplam.GenelToplam = TeklifToplam.IndirimliToplam + TeklifToplam.KdvTutari;

                IndirimliToplamText = FormatPrice(TeklifToplam.IndirimliToplam);
                KdvTutariText = FormatPrice(TeklifToplam.KdvTutari);
                GenelToplamText = FormatPrice(TeklifToplam.GenelToplam);

                OnPropertyChanged(nameof(TeklifToplam));
                OnPropertyChanged(nameof(IndirimliToplamText));
                OnPropertyChanged(nameof(KdvTutariText));
                OnPropertyChanged(nameof(GenelToplamText));
            }
        }

        private decimal GetFiyatByCurrency(Urun urun, string currency)
        {
            return currency switch
            {
                "USD" => urun.FiyatUSD,
                "EUR" => urun.FiyatEUR,
                _ => urun.FiyatTL
            };
        }

        private string FormatPrice(decimal price)
        {
            return price.ToString("C2", GetCultureByCurrency(Teklif.ParaBirimi ?? "TL"));
        }

        private CultureInfo GetCultureByCurrency(string currency)
        {
            return currency switch
            {
                "USD" => new CultureInfo("en-US"),
                "EUR" => new CultureInfo("en-IE"),
                _ => new CultureInfo("tr-TR"),
            };
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
                    dbTeklif.ParaBirimi = Teklif.ParaBirimi;
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
                    else
                    {
                        _context.TeklifUrunleri.Add(new TeklifUrun
                        {
                            TeklifId = Teklif.TeklifId,
                            UrunId = urunModel.UrunId,
                            Adet = urunModel.Adet,
                            BirimFiyat = urunModel.BirimFiyat,
                            IndirimliBirimFiyat = urunModel.IndirimliFiyat,
                            ToplamTutar = urunModel.Toplam
                        });
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
                        dbToplam.GenelToplam = TeklifToplam.GenelToplam;
                    }
                }
                else
                {
                    _context.TeklifToplamlari.Add(new TeklifToplam
                    {
                        TeklifId = Teklif.TeklifId,
                        IndirimliToplam = TeklifToplam?.IndirimliToplam ?? 0,
                        KdvTutari = TeklifToplam?.KdvTutari ?? 0,
                        GenelToplam = TeklifToplam?.GenelToplam ?? 0
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
                var titleFont = FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD);
                pdfDoc.Add(new Paragraph($"Teklif Detayları - {Teklif.Musteri.FirmaAdi}", titleFont));
                pdfDoc.Add(new Paragraph($"Tarih: {Teklif.OlusturmaTarihi:dd.MM.yyyy}"));
                pdfDoc.Add(new Paragraph($"Durum: {Teklif.Durum}"));
                pdfDoc.Add(new Paragraph($"Müşteri Notu: {Teklif.MusteriNotu ?? "-"}"));
                pdfDoc.Add(new Paragraph($"Para Birimi: {Teklif.ParaBirimi ?? "TL"}"));
                pdfDoc.Add(new Paragraph("\n"));
                PdfPTable table = new PdfPTable(6) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 2f, 5f, 1f, 2f, 2f, 2f });
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
                    AddCellToBody(table, urun.BirimFiyatText);
                    AddCellToBody(table, urun.IndirimliFiyatText);
                    AddCellToBody(table, urun.ToplamText);
                }
                pdfDoc.Add(table);
                pdfDoc.Add(new Paragraph("\n"));
                if (TeklifToplam != null)
                {
                    pdfDoc.Add(new Paragraph($"İndirimli Toplam: {IndirimliToplamText}"));
                    pdfDoc.Add(new Paragraph($"KDV Tutarı: {KdvTutariText}"));
                    pdfDoc.Add(new Paragraph($"Genel Toplam: {GenelToplamText}"));
                }
                pdfDoc.Close();
                MessageBox.Show("PDF başarıyla oluşturuldu.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF oluşturulurken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

        private void AddCellToBody(PdfPTable table, string text)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5
            };
            table.AddCell(cell);
        }

        public void EkleUrun(Urun urun, int adet, decimal indirimliFiyat)
        {
            try
            {
                var yeniUrun = new TeklifUrunModel
                {
                    UrunId = urun.UrunId,
                    UrunKodu = urun.UrunKodu,
                    UrunAciklamasi = urun.UrunAciklamasi,
                    Adet = adet,
                    BirimFiyat = GetFiyatByCurrency(urun, Teklif.ParaBirimi ?? "TL"),
                    IndirimliFiyat = indirimliFiyat,
                    FiyatTL = urun.FiyatTL,
                    FiyatUSD = urun.FiyatUSD,
                    FiyatEUR = urun.FiyatEUR,
                    BirimFiyatText = FormatPrice(GetFiyatByCurrency(urun, Teklif.ParaBirimi ?? "TL")),
                    IndirimliFiyatText = FormatPrice(indirimliFiyat),
                    ToplamText = FormatPrice(adet * indirimliFiyat)
                };
                yeniUrun.OnBirimFiyatDegisti += Model_OnBirimFiyatDegisti;
                yeniUrun.PropertyChanged += Model_PropertyChanged; // Yeni ürün için de değişiklikleri yakala
                TeklifUrunler.Add(yeniUrun);

                var dbUrun = new TeklifUrun
                {
                    TeklifId = Teklif.TeklifId,
                    UrunId = urun.UrunId,
                    Adet = adet,
                    BirimFiyat = yeniUrun.BirimFiyat,
                    IndirimliBirimFiyat = indirimliFiyat,
                    ToplamTutar = yeniUrun.Toplam
                };
                _context.TeklifUrunleri.Add(dbUrun);
                _context.SaveChanges();
                UpdateToplamlarText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürün eklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
        }
    }
}
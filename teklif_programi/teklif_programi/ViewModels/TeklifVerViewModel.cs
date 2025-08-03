#nullable enable

using CommunityToolkit.Mvvm.Input;
using iTextSharp.text;
using iTextSharp.text.pdf;
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
    public class TeklifVerViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context = new();

        private string _firmaArama = string.Empty;
        private Musteri? _firmaBilgisi;
        private string _urunArama = string.Empty;

        public TeklifVerViewModel()
        {
            UrunleriYukle();
            SepeteEkleCommand = new RelayCommand<Urun>(SepeteEkle, CanSepeteEkle);
            SepettenCikarCommand = new RelayCommand<TeklifUrunModel>(SepettenCikar);
            KaydetVePdfIndirCommand = new RelayCommand(KaydetVePdfIndir, CanKaydetVePdfIndir);
        }

        public string FirmaArama
        {
            get => _firmaArama;
            set
            {
                if (_firmaArama != value)
                {
                    _firmaArama = value;
                    OnPropertyChanged();

                    int.TryParse(_firmaArama, out int idArama);
                    var musteriler = _context.Musteriler.ToList();
                    FirmaBilgisi = musteriler.FirstOrDefault(f =>
                        f.firma_adi.Contains(_firmaArama, StringComparison.OrdinalIgnoreCase) ||
                        f.musteri_id == idArama ||
                        (!string.IsNullOrEmpty(f.firma_telefonu) && f.firma_telefonu.Contains(_firmaArama)));

                    if (FirmaBilgisi == null)
                        MessageBox.Show("Firma bulunamadı: " + _firmaArama);
                    else
                        MessageBox.Show("Firma bulundu: " + FirmaBilgisi.firma_adi);

                    OnPropertyChanged(nameof(FirmaBilgisi));
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public Musteri? FirmaBilgisi
        {
            get => _firmaBilgisi;
            set
            {
                if (_firmaBilgisi != value)
                {
                    _firmaBilgisi = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public string UrunArama
        {
            get => _urunArama;
            set
            {
                if (_urunArama != value)
                {
                    _urunArama = value;
                    OnPropertyChanged();
                    UrunleriFiltrele();
                }
            }
        }

        public ObservableCollection<Urun> TumUrunler { get; set; } = new();
        public ObservableCollection<Urun> FiltrelenmisUrunler { get; set; } = new();
        public ObservableCollection<TeklifUrunModel> SecilenUrunler { get; set; } = new();

        private void UrunleriYukle()
        {
            try
            {
                TumUrunler = new ObservableCollection<Urun>(_context.Urunler.ToList());
                FiltrelenmisUrunler = new ObservableCollection<Urun>(TumUrunler);
                OnPropertyChanged(nameof(FiltrelenmisUrunler));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürünler yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UrunleriFiltrele()
        {
            if (string.IsNullOrWhiteSpace(UrunArama))
            {
                FiltrelenmisUrunler = new ObservableCollection<Urun>(TumUrunler);
            }
            else
            {
                var filtreli = TumUrunler.Where(u =>
                    u.urun_kodu.Contains(UrunArama, StringComparison.OrdinalIgnoreCase) ||
                    u.urun_aciklamasi.Contains(UrunArama, StringComparison.OrdinalIgnoreCase)).ToList();
                FiltrelenmisUrunler = new ObservableCollection<Urun>(filtreli);
            }
            OnPropertyChanged(nameof(FiltrelenmisUrunler));
        }

        public RelayCommand<Urun> SepeteEkleCommand { get; }
        public RelayCommand<TeklifUrunModel> SepettenCikarCommand { get; }
        public RelayCommand KaydetVePdfIndirCommand { get; }

        private bool CanSepeteEkle(Urun? urun) => urun != null;
        private void SepeteEkle(Urun? urun)
        {
            if (urun == null) return;
            var mevcutUrun = SecilenUrunler.FirstOrDefault(u => u.urun_id == urun.urun_id);
            if (mevcutUrun != null)
            {
                mevcutUrun.adet++;
                HesaplaIndirimliFiyat(mevcutUrun);
            }
            else
            {
                var model = new TeklifUrunModel
                {
                    urun_id = urun.urun_id,
                    urun_kodu = urun.urun_kodu,
                    urun_aciklamasi = urun.urun_aciklamasi,
                    birim_fiyat = urun.birim_fiyat,
                    adet = 1
                };
                HesaplaIndirimliFiyat(model);
                SecilenUrunler.Add(model);
            }
            OnPropertyChanged(nameof(SecilenUrunler));
            OnPropertyChanged(nameof(ToplamFiyat));
            OnPropertyChanged(nameof(KdvUcreti));
            OnPropertyChanged(nameof(GenelToplam));
        }

        private void SepettenCikar(TeklifUrunModel? urun)
        {
            if (urun != null)
            {
                SecilenUrunler.Remove(urun);
                OnPropertyChanged(nameof(SecilenUrunler));
                OnPropertyChanged(nameof(ToplamFiyat));
                OnPropertyChanged(nameof(KdvUcreti));
                OnPropertyChanged(nameof(GenelToplam));
            }
        }

        private void HesaplaIndirimliFiyat(TeklifUrunModel model)
        {
            decimal indirim = GenelIndirimOrani / 100;
            model.indirimli_fiyat = model.birim_fiyat * (1 - indirim);
            // Toplam otomatik hesaplanacak
            OnPropertyChanged(nameof(SecilenUrunler)); // Koleksiyonu güncelle
        }

        private decimal _genelIndirimOrani = 0;
        public decimal GenelIndirimOrani
        {
            get => _genelIndirimOrani;
            set
            {
                _genelIndirimOrani = value < 0 ? 0 : value;
                OnPropertyChanged();
                RecalculateAll();
            }
        }

        private decimal _kdvOrani = 20;
        public decimal KdvOrani
        {
            get => _kdvOrani;
            set
            {
                _kdvOrani = value < 0 ? 0 : value;
                OnPropertyChanged();
                RecalculateAll();
            }
        }

        private void RecalculateAll()
        {
            foreach (var urun in SecilenUrunler)
            {
                HesaplaIndirimliFiyat(urun);
            }
            OnPropertyChanged(nameof(ToplamFiyat));
            OnPropertyChanged(nameof(KdvUcreti));
            OnPropertyChanged(nameof(GenelToplam));
        }

        public decimal ToplamFiyat => SecilenUrunler.Sum(u => u.toplam);
        public decimal KdvUcreti => ToplamFiyat * (KdvOrani / 100);
        public decimal GenelToplam => ToplamFiyat + KdvUcreti;

        public bool CanSave => FirmaBilgisi != null && SecilenUrunler.Any();
        private bool CanKaydetVePdfIndir() => CanSave;

        private void KaydetVePdfIndir()
        {
            if (FirmaBilgisi == null || !SecilenUrunler.Any())
            {
                MessageBox.Show("Lütfen bir firma seçin ve en az bir ürün ekleyin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var transaction = _context.Database.BeginTransaction();
                var teklif = new Teklif
                {
                    musteri_id = FirmaBilgisi.musteri_id,
                    olusturma_tarihi = DateTime.Now,
                    genel_indirim_orani = GenelIndirimOrani,
                    kdv_orani = KdvOrani
                };
                _context.Teklifler.Add(teklif);
                _context.SaveChanges();

                foreach (var urun in SecilenUrunler)
                {
                    _context.TeklifUrunleri.Add(new TeklifUrun
                    {
                        teklif_id = teklif.teklif_id,
                        urun_id = urun.urun_id,
                        adet = urun.adet,
                        birim_fiyat = urun.birim_fiyat,
                        indirimli_birim_fiyat = urun.indirimli_fiyat,
                        toplam_tutar = urun.toplam
                    });
                }
                _context.SaveChanges();

                var toplam = new TeklifToplam
                {
                    teklif_id = teklif.teklif_id,
                    indirimli_toplam = ToplamFiyat,
                    kdv_tutari = KdvUcreti,
                    genel_toplam = GenelToplam
                };
                _context.TeklifToplamlari.Add(toplam);
                _context.SaveChanges();
                transaction.Commit();

                SaveFileDialog saveFileDialog = new()
                {
                    Filter = "PDF Dosyaları (*.pdf)|*.pdf",
                    FileName = $"Teklif_{teklif.teklif_id}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    using FileStream fs = new(saveFileDialog.FileName, FileMode.Create);
                    Document doc = new(PageSize.A4, 25, 25, 30, 30);
                    PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    Font titleFont = new(baseFont, 18, Font.BOLD);
                    Font normalFont = new(baseFont, 12);
                    Font boldFont = new(baseFont, 12, Font.BOLD);

                    doc.Add(new Paragraph("Teklif Belgesi", titleFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 20 });
                    doc.Add(new Paragraph($"Firma: {FirmaBilgisi!.firma_adi}", normalFont));
                    doc.Add(new Paragraph($"Adres: {FirmaBilgisi.firma_adresi}", normalFont));
                    doc.Add(new Paragraph($"Telefon: {FirmaBilgisi.firma_telefonu}", normalFont));
                    doc.Add(new Paragraph($"E-posta: {FirmaBilgisi.firma_eposta ?? "Belirtilmemiş"}", normalFont));
                    doc.Add(new Paragraph($"Tarih: {DateTime.Now:dd.MM.yyyy HH:mm}", normalFont) { SpacingAfter = 20 });

                    PdfPTable table = new(5);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 1, 3, 1, 1, 1 });

                    table.AddCell(new PdfPCell(new Phrase("Kod", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                    table.AddCell(new PdfPCell(new Phrase("Açıklama", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                    table.AddCell(new PdfPCell(new Phrase("Adet", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                    table.AddCell(new PdfPCell(new Phrase("İnd. Fiyat", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                    table.AddCell(new PdfPCell(new Phrase("Toplam", boldFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                    foreach (var urun in SecilenUrunler)
                    {
                        table.AddCell(new PdfPCell(new Phrase(urun.urun_kodu, normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(urun.urun_aciklamasi, normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(urun.adet.ToString(), normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(urun.indirimli_fiyat.ToString("C2"), normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(urun.toplam.ToString("C2"), normalFont)));
                    }

                    doc.Add(table);
                    doc.Add(new Paragraph($"Toplam Fiyat: {ToplamFiyat:C2}", boldFont) { Alignment = Element.ALIGN_RIGHT, SpacingBefore = 10 });
                    doc.Add(new Paragraph($"KDV (%{KdvOrani}): {KdvUcreti:C2}", boldFont) { Alignment = Element.ALIGN_RIGHT });
                    doc.Add(new Paragraph($"Genel Toplam: {GenelToplam:C2}", boldFont) { Alignment = Element.ALIGN_RIGHT });
                    doc.Close();
                }
                MessageBox.Show("Teklif başarıyla kaydedildi ve PDF oluşturuldu!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}
#nullable restore
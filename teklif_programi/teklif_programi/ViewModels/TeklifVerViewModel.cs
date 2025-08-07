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
using teklif_programi.Services;

namespace teklif_programi.ViewModels
{
    public class TeklifVerViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context = new();

        private string _firmaArama = string.Empty;
        private Musteri? _firmaBilgisi;
        private string _urunArama = string.Empty;
        private string _selectedCurrency = "TL"; // Varsayılan para birimi
        private ObservableCollection<string> _paraBirimiListe;
        private string _ilgiliKisi = string.Empty; // Yeni özellik: İlgili Kişi
        private string _ilgiliKisiNumarasi = string.Empty; // Yeni özellik: İlgili Kişi Numarası

        public ObservableCollection<DovizKuru> DovizKurlari { get; set; }

        public TeklifVerViewModel()
        {
            ParaBirimiListe = new ObservableCollection<string> { "TL", "USD", "EUR" };
            UrunleriYukle();
            SepeteEkleCommand = new RelayCommand<Urun>(SepeteEkle, CanSepeteEkle);
            SepettenCikarCommand = new RelayCommand<TeklifUrunModel>(SepettenCikar);
            KaydetVePdfIndirCommand = new RelayCommand(KaydetVePdfIndir);

            DovizKurlari = new ObservableCollection<DovizKuru>();
            DovizKurlariGuncelle();
        }

        private void DovizKurlariGuncelle()
        {
            var kurListesi = DovizServisi.KurListesiniGetir();
            DovizKurlari.Clear();
            foreach (var kur in kurListesi)
                DovizKurlari.Add(kur);
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

                    if (string.IsNullOrWhiteSpace(_firmaArama))
                    {
                        FirmaBilgisi = null;
                        return;
                    }

                    var musteriler = _context.Musteriler.ToList();
                    Musteri? bulunanFirma = null;

                    if (int.TryParse(_firmaArama, out int idArama))
                    {
                        bulunanFirma = musteriler.FirstOrDefault(f => f.MusteriId == idArama);
                    }

                    if (bulunanFirma == null && _firmaArama.Length >= 2)
                    {
                        bulunanFirma = musteriler.FirstOrDefault(f =>
                            !string.IsNullOrEmpty(f.FirmaAdi) &&
                            f.FirmaAdi.Contains(_firmaArama, StringComparison.OrdinalIgnoreCase));
                    }

                    FirmaBilgisi = bulunanFirma;
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

        // Yeni özellik: İlgili Kişi
        public string IlgiliKisi
        {
            get => _ilgiliKisi;
            set
            {
                if (_ilgiliKisi != value)
                {
                    _ilgiliKisi = value;
                    OnPropertyChanged();
                }
            }
        }

        // Yeni özellik: İlgili Kişi Numarası
        public string IlgiliKisiNumarasi
        {
            get => _ilgiliKisiNumarasi;
            set
            {
                if (_ilgiliKisiNumarasi != value)
                {
                    _ilgiliKisiNumarasi = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Urun> TumUrunler { get; set; } = [];
        public ObservableCollection<Urun> FiltrelenmisUrunler { get; set; } = [];
        public ObservableCollection<TeklifUrunModel> SecilenUrunler { get; set; } = [];

        public ObservableCollection<string> ParaBirimiListe
        {
            get => _paraBirimiListe;
            set
            {
                _paraBirimiListe = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                if (_selectedCurrency != value)
                {
                    _selectedCurrency = value;
                    OnPropertyChanged();
                    RecalculateAll(); // Para birimi değiştiğinde tüm fiyatları güncelle
                }
            }
        }

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
                    u.UrunKodu.Contains(UrunArama, StringComparison.OrdinalIgnoreCase) ||
                    u.UrunAciklamasi.Contains(UrunArama, StringComparison.OrdinalIgnoreCase)).ToList();
                FiltrelenmisUrunler = new ObservableCollection<Urun>(filtreli);
            }
            OnPropertyChanged(nameof(FiltrelenmisUrunler));
        }

        private void Model_OnBirimFiyatDegisti(object? sender, EventArgs e)
        {
            if (sender is TeklifUrunModel model)
            {
                HesaplaIndirimliFiyat(model);
                OnPropertyChanged(nameof(ToplamFiyat));
                OnPropertyChanged(nameof(KdvUcreti));
                OnPropertyChanged(nameof(GenelToplam));
            }
        }

        public RelayCommand<Urun> SepeteEkleCommand { get; }
        public RelayCommand<TeklifUrunModel> SepettenCikarCommand { get; }
        public RelayCommand KaydetVePdfIndirCommand { get; }

        private bool CanSepeteEkle(Urun? urun) => urun != null;
        private void SepeteEkle(Urun? urun)
        {
            if (urun == null) return;
            var mevcutUrun = SecilenUrunler.FirstOrDefault(u => u.UrunId == urun.UrunId);
            if (mevcutUrun != null)
            {
                mevcutUrun.Adet++;
                HesaplaIndirimliFiyat(mevcutUrun);
            }
            else
            {
                var model = new TeklifUrunModel
                {
                    UrunId = urun.UrunId,
                    UrunKodu = urun.UrunKodu,
                    UrunAciklamasi = urun.UrunAciklamasi,
                    FiyatTL = urun.FiyatTL,
                    FiyatUSD = urun.FiyatUSD,
                    FiyatEUR = urun.FiyatEUR,
                    BirimFiyat = GetFiyatByCurrency(urun, SelectedCurrency),
                    Adet = 1
                };

                model.OnBirimFiyatDegisti += Model_OnBirimFiyatDegisti;
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
            model.IndirimliFiyat = model.BirimFiyat * (1 - indirim);
            OnPropertyChanged(nameof(SecilenUrunler)); // Koleksiyonu güncelle
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
                urun.BirimFiyat = GetFiyatByCurrency(
                    TumUrunler.FirstOrDefault(u => u.UrunId == urun.UrunId),
                    SelectedCurrency
                );
                HesaplaIndirimliFiyat(urun);
            }
            OnPropertyChanged(nameof(ToplamFiyat));
            OnPropertyChanged(nameof(KdvUcreti));
            OnPropertyChanged(nameof(GenelToplam));
        }

        public decimal ToplamFiyat => SecilenUrunler.Sum(u => u.Toplam);
        public decimal KdvUcreti => ToplamFiyat * (KdvOrani / 100);
        public decimal GenelToplam => ToplamFiyat + KdvUcreti;

        private void KaydetVePdfIndir()
        {
            if (FirmaBilgisi == null)
            {
                MessageBox.Show("Lütfen bir firma seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!SecilenUrunler.Any())
            {
                MessageBox.Show("Lütfen en az bir ürün ekleyin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var transaction = _context.Database.BeginTransaction();
                var teklif = new Teklif
                {
                    MusteriId = FirmaBilgisi.MusteriId,
                    PersonelId = 2,
                    OlusturmaTarihi = DateTime.Now,
                    GenelIndirimOrani = GenelIndirimOrani,
                    KdvOrani = KdvOrani
                };
                _context.Teklifler.Add(teklif);
                _context.SaveChanges();

                foreach (var urun in SecilenUrunler)
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
                _context.TeklifToplamlari.Add(new TeklifToplam
                {
                    TeklifId = teklif.TeklifId,
                    IndirimliToplam = ToplamFiyat,
                    KdvTutari = KdvUcreti,
                    GenelToplam = GenelToplam
                });
                _context.SaveChanges();
                transaction.Commit();

                string templatePath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\LiyaTeklifBelgesi.pdf";
                SaveFileDialog saveFileDialog = new()
                {
                    Filter = "PDF Dosyaları (*.pdf)|*.pdf",
                    FileName = $"Teklif_{FirmaBilgisi.FirmaAdi}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    PdfReader reader = new PdfReader(templatePath);
                    using FileStream fs = new(saveFileDialog.FileName, FileMode.Create);
                    PdfStamper stamper = new PdfStamper(reader, fs);

                    string fontPath = @"C:\Windows\Fonts\arial.ttf";
                    if (!File.Exists(fontPath))
                    {
                        MessageBox.Show("Arial font dosyası bulunamadı: " + fontPath, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        stamper.Close();
                        reader.Close();
                        return;
                    }

                    BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font firmaFont = new(baseFont, 10, Font.NORMAL, new BaseColor(128, 128, 128));
                    Font headerFont = new(baseFont, 10, Font.BOLD, BaseColor.BLACK);
                    Font bodyFont = new(baseFont, 10, Font.NORMAL, BaseColor.BLACK);

                    int currentPage = 2;
                    PdfContentByte canvas = stamper.GetOverContent(currentPage);

                    // Sol kısım: Firma Bilgileri
                    Phrase firmaBilgileri = new Phrase();
                    firmaBilgileri.Add(new Chunk("Firma Adı: ", headerFont));
                    firmaBilgileri.Add(new Chunk(FirmaBilgisi.FirmaAdi + "\n", bodyFont));
                    firmaBilgileri.Add(new Chunk("Firma Adresi: ", headerFont));
                    firmaBilgileri.Add(new Chunk(FirmaBilgisi.FirmaAdresi + "\n", bodyFont));
                    firmaBilgileri.Add(new Chunk("Firma Telefonu: ", headerFont));
                    firmaBilgileri.Add(new Chunk(FirmaBilgisi.FirmaTelefonu + "\n" ?? "Belirtilmemiş" + "\n", bodyFont));
                    firmaBilgileri.Add(new Chunk("Firma E-Posta: ", headerFont));
                    firmaBilgileri.Add(new Chunk(FirmaBilgisi.FirmaEposta + "\n" ?? "Belirtilmemiş" + "\n", bodyFont));
                    firmaBilgileri.Add(new Chunk("İlgili Kişi: ", headerFont));
                    firmaBilgileri.Add(new Chunk(string.IsNullOrWhiteSpace(IlgiliKisi) ? "Belirtilmemiş" : IlgiliKisi + "\n", bodyFont));
                    firmaBilgileri.Add(new Chunk("İlgili Kişi E-postası: ", headerFont));
                    firmaBilgileri.Add(new Chunk("Belirtilmemiş\n", bodyFont)); 

                    ColumnText ctLeft = new ColumnText(canvas);
                    ctLeft.SetSimpleColumn(firmaBilgileri, 50, 700, 300, 500, 15, Element.ALIGN_LEFT);
                    ctLeft.Go();

                    // Sağ kısım: Teklif Bilgileri
                    Phrase teklifBilgileri = new Phrase();
                    teklifBilgileri.Add(new Chunk("Teklif Tarihi: ", headerFont));
                    teklifBilgileri.Add(new Chunk(teklif.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm") + "\n", bodyFont));
                    teklifBilgileri.Add(new Chunk("Teklif Kodu: ", headerFont));
                    teklifBilgileri.Add(new Chunk(teklif.TeklifId.ToString() + "\n", bodyFont));
                    teklifBilgileri.Add(new Chunk("Teklifi Yapan Personel: ", headerFont));
                    teklifBilgileri.Add(new Chunk("Erhan Öğüt" + "\n", bodyFont));
                    teklifBilgileri.Add(new Chunk("Personel Cep No: ", headerFont));
                    teklifBilgileri.Add(new Chunk("05179841645\n", bodyFont)); 

                    ColumnText ctRight = new ColumnText(canvas);
                    ctRight.SetSimpleColumn(teklifBilgileri, 350, 700, 550, 500, 15, Element.ALIGN_LEFT);
                    ctRight.Go();

                    // Ürünler ve toplamlar kısmı (mevcut tasarım korunuyor)
                    Phrase urunBaslik = new Phrase("Teklif Edilen Ürünler", new Font(baseFont, 12, Font.BOLD, BaseColor.BLACK));
                    ctLeft.SetSimpleColumn(urunBaslik, 50, 480, 550, 460, 15, Element.ALIGN_LEFT);
                    ctLeft.Go();

                    PdfPTable table = new PdfPTable(6);
                    table.TotalWidth = 500f;
                    table.LockedWidth = true;
                    float[] widths = new float[] { 2f, 5f, 1f, 2f, 2f, 2f };
                    table.SetWidths(widths);

                    AddCellToHeader(table, "Ürün Kodu", headerFont, new BaseColor(240, 240, 240));
                    AddCellToHeader(table, "Açıklama", headerFont, new BaseColor(240, 240, 240));
                    AddCellToHeader(table, "Adet", headerFont, new BaseColor(240, 240, 240));
                    AddCellToHeader(table, "Birim Satış Fiyatı", headerFont, new BaseColor(240, 240, 240));
                    AddCellToHeader(table, $"İndirimli Birim Satış Fiyatı(%{GenelIndirimOrani})", headerFont, new BaseColor(240, 240, 240));
                    AddCellToHeader(table, "Toplam Fiyat", headerFont, new BaseColor(240, 240, 240));

                    int rowCount = 0;
                    foreach (var urun in SecilenUrunler)
                    {
                        BaseColor rowColor = rowCount % 2 == 0 ? BaseColor.WHITE : new BaseColor(245, 245, 245);
                        AddCellToBody(table, urun.UrunKodu, bodyFont, rowColor);
                        AddCellToBody(table, urun.UrunAciklamasi, bodyFont, rowColor);
                        AddCellToBody(table, urun.Adet.ToString(), bodyFont, rowColor);
                        AddCellToBody(table, urun.BirimFiyat.ToString("C2"), bodyFont, rowColor);
                        AddCellToBody(table, urun.IndirimliFiyat.ToString("C2"), bodyFont, rowColor);
                        AddCellToBody(table, urun.Toplam.ToString("C2"), bodyFont, rowColor);
                        rowCount++;
                    }

                    table.WriteSelectedRows(0, -1, 50, 460, canvas);

                    Phrase toplamBilgileri = new Phrase();
                    toplamBilgileri.Add(new Chunk($"İndirimli Toplam(%{GenelIndirimOrani}): {ToplamFiyat:C2}\n", headerFont));
                    toplamBilgileri.Add(new Chunk($"KDV (%{KdvOrani}): {KdvUcreti:C2}\n", headerFont));
                    toplamBilgileri.Add(new Chunk($"Genel Toplam: {GenelToplam:C2}", headerFont));

                    ctLeft.SetSimpleColumn(toplamBilgileri, 50, 150, 550, 50, 15, Element.ALIGN_RIGHT);
                    ctLeft.Go();

                    stamper.Close();
                    reader.Close();
                    MessageBox.Show("Teklif başarıyla kaydedildi ve PDF oluşturuldu!", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu: {ex.Message}\nİç Hata: {ex.InnerException?.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCellToHeader(PdfPTable table, string text, Font font, BaseColor backgroundColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = backgroundColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 5
            };  
            table.AddCell(cell);
        }

        private void AddCellToBody(PdfPTable table, string text, Font font, BaseColor backgroundColor)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = backgroundColor,
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 5
            };
            table.AddCell(cell);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}
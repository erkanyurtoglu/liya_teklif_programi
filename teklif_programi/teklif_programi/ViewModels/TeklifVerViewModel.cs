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
using System.Globalization;

namespace teklif_programi.ViewModels
{
    public class TeklifVerViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context = new();
        private string _firmaArama = string.Empty;
        private Musteri? _firmaBilgisi;
        private string _urunArama = string.Empty;
        private string _selectedCurrency = "TL";
        private ObservableCollection<string> _paraBirimiListe = new();
        private string _ilgiliKisi = string.Empty;
        private string _ilgiliKisiNumarasi = string.Empty;
        private string _ilgiliKisiEposta = string.Empty;
        private string _satisSozlesmesiMetni = string.Empty;

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
            SatisSozlesmesiMetni = @"
            1.Fiyatımız DOLAR cinsinden belirtilmiş olup, KDV dahildir. Fatura kesim tarihinde geçerli olan TCMB efektif satış kuru esas alınacaktır.
            2. Cihaz ücreti: %30’u sipariş sırasında peşin, kalan tutar teslimatta ödenecektir.
            3. Cihazlar; 1 yıl mekanik, 2 yıl elektronik parça olarak ücretsiz servis garantilidir. 10 yıl süreyle ücreti karşılığı teknik servis ve eğitim hizmeti verilecektir.
            4. Cihaz Teslimatı: Siparişe istinaden 1 hafta içinde teslim
            5. Teklif Opsiyonu: Teklif tarihinden itibaren 3 gündür.
            6. Nakliye: Satıcı firmaya aittir.
            7. Alternatif olarak sunulan cihaz bedelleri, toplam teklif tutarına dahil edilmemiştir.
            8. Banka Bilgilerimiz: Liya Laboratuvar Test Cihazları İmalat ve Dış Ticaret A.Ş.
               İŞ BANKASI TR16 0006 4000 0014 1520 1653 38
               HALK BANKASI TR51 0001 2009 4140 0010 2645 69";
        }

        public string SatisSozlesmesiMetni
        {
            get => _satisSozlesmesiMetni;
            set { _satisSozlesmesiMetni = value; OnPropertyChanged(); }
        }

        private void DovizKurlariGuncelle()
        {
            var kurListesi = DovizServisi.KurListesiniGetir();
            DovizKurlari.Clear();
            foreach (var kur in kurListesi) DovizKurlari.Add(kur);
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
                    if (string.IsNullOrWhiteSpace(_firmaArama)) { FirmaBilgisi = null; return; }
                    var musteriler = _context.Musteriler.ToList();
                    Musteri? bulunanFirma = null;
                    if (int.TryParse(_firmaArama, out int idArama))
                        bulunanFirma = musteriler.FirstOrDefault(f => f.MusteriId == idArama);
                    if (bulunanFirma == null && _firmaArama.Length >= 2)
                        bulunanFirma = musteriler.FirstOrDefault(f => f.FirmaAdi?.Contains(_firmaArama, StringComparison.OrdinalIgnoreCase) == true);
                    FirmaBilgisi = bulunanFirma;
                }
            }
        }

        public Musteri? FirmaBilgisi
        {
            get => _firmaBilgisi;
            set { _firmaBilgisi = value; OnPropertyChanged(); }
        }

        public string UrunArama
        {
            get => _urunArama;
            set { _urunArama = value; OnPropertyChanged(); UrunleriFiltrele(); }
        }

        public string IlgiliKisi
        {
            get => _ilgiliKisi;
            set { _ilgiliKisi = value; OnPropertyChanged(); }
        }

        public string IlgiliKisiNumarasi
        {
            get => _ilgiliKisiNumarasi;
            set { _ilgiliKisiNumarasi = value; OnPropertyChanged(); }
        }

        public string IlgiliKisiEposta
        {
            get => _ilgiliKisiEposta;
            set { _ilgiliKisiEposta = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Urun> TumUrunler { get; set; } = [];
        public ObservableCollection<Urun> FiltrelenmisUrunler { get; set; } = [];
        public ObservableCollection<TeklifUrunModel> SecilenUrunler { get; set; } = [];

        public ObservableCollection<string> ParaBirimiListe
        {
            get => _paraBirimiListe;
            set { _paraBirimiListe = value; OnPropertyChanged(); }
        }

        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set { _selectedCurrency = value; OnPropertyChanged(); RecalculateAll(); }
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
                FiltrelenmisUrunler = new ObservableCollection<Urun>(TumUrunler);
            else
                FiltrelenmisUrunler = new ObservableCollection<Urun>(TumUrunler.Where(u =>
                    u.UrunKodu.Contains(UrunArama, StringComparison.OrdinalIgnoreCase) ||
                    u.UrunAciklamasi.Contains(UrunArama, StringComparison.OrdinalIgnoreCase)));
            OnPropertyChanged(nameof(FiltrelenmisUrunler));
        }

        private void Model_OnBirimFiyatDegisti(object? sender, EventArgs e)
        {
            if (sender is TeklifUrunModel model)
            {
                model.BirimFiyatText = FormatPrice(model.BirimFiyat);
                model.IndirimliFiyatText = FormatPrice(model.IndirimliFiyat);
                model.ToplamText = FormatPrice(model.Toplam);

                OnPropertyChanged(nameof(ToplamFiyat));
                OnPropertyChanged(nameof(KdvUcreti));
                OnPropertyChanged(nameof(GenelToplam));
                UpdateTotalsText();
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
                model.BirimFiyatText = FormatPrice(model.BirimFiyat);
                model.IndirimliFiyatText = FormatPrice(model.IndirimliFiyat);
                model.ToplamText = FormatPrice(model.Toplam);
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
                UpdateTotalsText();
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
            OnPropertyChanged(nameof(SecilenUrunler));
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

        private CultureInfo GetCultureByCurrency(string currency)
        {
            return currency switch
            {
                "USD" => new CultureInfo("en-US"),
                "EUR" => new CultureInfo("en-IE"),
                _ => new CultureInfo("tr-TR"),
            };
        }

        private string FormatPrice(decimal price)
        {
            return price.ToString("C2", GetCultureByCurrency(SelectedCurrency));
        }

        private string _toplamFiyatText = string.Empty;
        public string ToplamFiyatText { get => _toplamFiyatText; set { _toplamFiyatText = value; OnPropertyChanged(); } }

        private string _kdvUcretiText = string.Empty;
        public string KdvUcretiText { get => _kdvUcretiText; set { _kdvUcretiText = value; OnPropertyChanged(); } }

        private string _genelToplamText = string.Empty;
        public string GenelToplamText { get => _genelToplamText; set { _genelToplamText = value; OnPropertyChanged(); } }

        private void UpdateTotalsText()
        {
            ToplamFiyatText = FormatPrice(ToplamFiyat);
            KdvUcretiText = FormatPrice(KdvUcreti);
            GenelToplamText = FormatPrice(GenelToplam);
        }

        private decimal _genelIndirimOrani = 0;
        public decimal GenelIndirimOrani
        {
            get => _genelIndirimOrani;
            set { _genelIndirimOrani = value < 0 ? 0 : value; OnPropertyChanged(); RecalculateAll(); }
        }

        private decimal _kdvOrani = 20;
        public decimal KdvOrani
        {
            get => _kdvOrani;
            set { _kdvOrani = value < 0 ? 0 : value; OnPropertyChanged(); RecalculateAll(); }
        }

        private void RecalculateAll()
        {
            foreach (var urun in SecilenUrunler)
            {
                var matchedUrun = TumUrunler.FirstOrDefault(u => u.UrunId == urun.UrunId);
                if (matchedUrun != null)
                {
                    urun.BirimFiyat = GetFiyatByCurrency(matchedUrun, SelectedCurrency);
                    HesaplaIndirimliFiyat(urun);
                    urun.BirimFiyatText = FormatPrice(urun.BirimFiyat);
                    urun.IndirimliFiyatText = FormatPrice(urun.IndirimliFiyat);
                    urun.ToplamText = FormatPrice(urun.Toplam);
                }
                else
                {
                    urun.BirimFiyat = 0;
                    HesaplaIndirimliFiyat(urun);
                    urun.BirimFiyatText = FormatPrice(0);
                    urun.IndirimliFiyatText = FormatPrice(0);
                    urun.ToplamText = FormatPrice(0);
                }
            }

            OnPropertyChanged(nameof(ToplamFiyat));
            OnPropertyChanged(nameof(KdvUcreti));
            OnPropertyChanged(nameof(GenelToplam));
            UpdateTotalsText();
        }

        public decimal ToplamFiyat => SecilenUrunler.Sum(u => u.Toplam);
        public decimal KdvUcreti => ToplamFiyat * (KdvOrani / 100);
        public decimal GenelToplam => ToplamFiyat + KdvUcreti;

        private void KaydetVePdfIndir()
        {
            if (FirmaBilgisi == null || !SecilenUrunler.Any())
            {
                MessageBox.Show(FirmaBilgisi == null ? "Lütfen bir firma seçin." : "Lütfen en az bir ürün ekleyin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    KdvOrani = KdvOrani,
                    ParaBirimi = SelectedCurrency // Para birimini ekle
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
                    firmaTable.AddCell(CreateLeftAlignedBodyCell(FirmaBilgisi.FirmaAdi, bodyFont));
                    firmaTable.AddCell(CreateRightAlignedHeaderCell("Firma Adresi:", headerFont));
                    firmaTable.AddCell(CreateLeftAlignedBodyCell(FirmaBilgisi.FirmaAdresi, bodyFont));
                    firmaTable.AddCell(CreateRightAlignedHeaderCell("Firma Telefonu:", headerFont));
                    firmaTable.AddCell(CreateLeftAlignedBodyCell(FirmaBilgisi.FirmaTelefonu ?? "Belirtilmemiş", bodyFont));
                    firmaTable.AddCell(CreateRightAlignedHeaderCell("Firma E-Posta:", headerFont));
                    firmaTable.AddCell(CreateLeftAlignedBodyCell(FirmaBilgisi.FirmaEposta ?? "Belirtilmemiş", bodyFont));
                    firmaTable.AddCell(CreateRightAlignedHeaderCell("Yetkili:", headerFont));
                    firmaTable.AddCell(CreateLeftAlignedBodyCell(string.IsNullOrWhiteSpace(IlgiliKisi) ? "Belirtilmemiş" : IlgiliKisi, bodyFont));
                    firmaTable.AddCell(CreateRightAlignedHeaderCell("Yetkili Numarası:", headerFont));
                    firmaTable.AddCell(CreateLeftAlignedBodyCell(string.IsNullOrWhiteSpace(IlgiliKisiNumarasi) ? "Belirtilmemiş" : IlgiliKisiNumarasi, bodyFont));
                    firmaTable.AddCell(CreateRightAlignedHeaderCell("Yetkili Email:", headerFont));
                    firmaTable.AddCell(CreateLeftAlignedBodyCell(string.IsNullOrWhiteSpace(IlgiliKisiEposta) ? "Belirtilmemiş" : IlgiliKisiEposta, bodyFont));
                    firmaTable.WriteSelectedRows(0, -1, 50, 740, canvas);

                    PdfPTable teklifTable = new PdfPTable(2) { TotalWidth = 240f, DefaultCell = { Border = 0 } };
                    teklifTable.SetWidths(new float[] { 2f, 2f });
                    teklifTable.AddCell(CreateRightAlignedHeaderCell("Teklif Tarihi:", headerFont));
                    teklifTable.AddCell(CreateLeftAlignedBodyCell(teklif.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm"), bodyFont));
                    teklifTable.AddCell(CreateRightAlignedHeaderCell("Teklif Kodu:", headerFont));
                    teklifTable.AddCell(CreateLeftAlignedBodyCell(teklif.TeklifId.ToString(), bodyFont));
                    teklifTable.AddCell(CreateRightAlignedHeaderCell("Teklif Veren:", headerFont));
                    teklifTable.AddCell(CreateLeftAlignedBodyCell("Erhan Öğüt", bodyFont));
                    teklifTable.AddCell(CreateRightAlignedHeaderCell("Personel Cep No:", headerFont));
                    teklifTable.AddCell(CreateLeftAlignedBodyCell("05179841645", bodyFont));
                    teklifTable.AddCell(CreateRightAlignedHeaderCell("Para Birimi:", headerFont));
                    teklifTable.AddCell(CreateLeftAlignedBodyCell(teklif.ParaBirimi, bodyFont)); // Para birimi ekle
                    teklifTable.WriteSelectedRows(0, -1, 330, 730, canvas);

                    canvas.SetColorFill(new BaseColor(200, 200, 200));
                    canvas.Rectangle(22.5f, 590, 550, 1);
                    canvas.Fill();
                    canvas.SetColorFill(new BaseColor(200, 200, 200));
                    canvas.Rectangle(22.5f, 555, 550, 1);
                    canvas.Fill();

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
                    AddCellToHeader(table, $"İndirimli Birim Satış Fiyatı(%{GenelIndirimOrani})", headerFont, new BaseColor(240, 240, 240));
                    AddCellToHeader(table, "Toplam Fiyat", headerFont, new BaseColor(240, 240, 240));
                    int rowCount = 0;
                    int urunNo = 1;
                    foreach (var urun in SecilenUrunler)
                    {
                        BaseColor rowColor = rowCount % 2 == 0 ? BaseColor.WHITE : new BaseColor(245, 245, 245);
                        AddCellToBody(table, urunNo.ToString(), bodyFont, rowColor);
                        AddCellToBody(table, urun.UrunKodu, bodyFont, rowColor);
                        AddCellToBody(table, urun.UrunAciklamasi, bodyFont, rowColor);
                        AddCellToBody(table, urun.Adet.ToString(), bodyFont, rowColor);
                        AddCellToBody(table, FormatPrice(urun.BirimFiyat), bodyFont, rowColor);
                        AddCellToBody(table, FormatPrice(urun.IndirimliFiyat), bodyFont, rowColor);
                        AddCellToBody(table, FormatPrice(urun.Toplam), bodyFont, rowColor);
                        rowCount++;
                        urunNo++;
                    }
                    table.WriteSelectedRows(0, -1, 22.5f, 520, canvas);

                    PdfPTable toplamTable = new PdfPTable(2) { TotalWidth = 240f, DefaultCell = { Border = 0 }, HorizontalAlignment = Element.ALIGN_RIGHT };
                    toplamTable.SetWidths(new float[] { 3f, 2f });
                    toplamTable.AddCell(CreateRightAlignedHeaderCell($"İndirimli Toplam(%{GenelIndirimOrani}):", headerFont));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(ToplamFiyat), headerFont));
                    toplamTable.AddCell(CreateRightAlignedHeaderCell($"KDV (%{KdvOrani}):", headerFont));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(KdvUcreti), headerFont));
                    toplamTable.AddCell(CreateRightAlignedHeaderCell("Genel Toplam:", headerFont));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(FormatPrice(GenelToplam), headerFont));
                    toplamTable.WriteSelectedRows(0, -1, 352.5f, 150, canvas);

                    string sozlesmePdfPath = @"C:\Users\yurto\Documents\GitHub\liya_teklif_programi\satisSozlesmesi.pdf";

                    using var sozlesmeReader = new PdfReader(sozlesmePdfPath);

                    int lastPage = reader.NumberOfPages;

                    // Yeni sayfa ekle
                    stamper.InsertPage(lastPage + 1, sozlesmeReader.GetPageSizeWithRotation(1));
                    currentPage = lastPage + 1;

                    // Arka plan olarak şablon PDF sayfasını ekle (under content)
                    PdfContentByte underContent = stamper.GetUnderContent(currentPage);
                    PdfImportedPage page = stamper.GetImportedPage(sozlesmeReader, 1);
                    underContent.AddTemplate(page, 0, 0);

                    // Üstüne metinleri basmaya devam et (over content)
                    PdfContentByte overContent = stamper.GetOverContent(currentPage);

                    ColumnText ctSozlesme = new ColumnText(overContent);
                    ctSozlesme.SetSimpleColumn(new Phrase(SatisSozlesmesiMetni, sozlesmeFont), 40f, 50f, 550f, 700f, 18, Element.ALIGN_LEFT);
                    ctSozlesme.Go();


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

        private PdfPCell CreateRightAlignedHeaderCell(string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = 0, PaddingRight = 5 };
            return cell;
        }

        private PdfPCell CreateLeftAlignedBodyCell(string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = 0 };
            return cell;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}
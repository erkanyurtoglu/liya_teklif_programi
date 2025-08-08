// Gerekli isim alanları: MVVM, PDF oluşturma, veritabanı ve UI için
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


// ViewModel sınıflarının isim alanı
namespace teklif_programi.ViewModels
{
    // TeklifVerViewModel: Yeni teklif oluşturmayı ve PDF üretimini yöneten ViewModel
    public class TeklifVerViewModel : INotifyPropertyChanged
    {
        // _context: Veritabanı bağlantısı için DbContext
        private readonly TeklifDbContext _context = new();
        // _firmaArama: Firma arama metni
        private string _firmaArama = string.Empty;
        // _firmaBilgisi: Seçilen müşteri bilgileri
        private Musteri? _firmaBilgisi;
        // _urunArama: Ürün arama metni
        private string _urunArama = string.Empty;
        // _selectedCurrency: Seçilen para birimi (varsayılan TL)
        private string _selectedCurrency = "TL";
        // _paraBirimiListe: Para birimi seçenekleri
        private ObservableCollection<string> _paraBirimiListe = new();
        // _ilgiliKisi: İlgili kişi adı
        private string _ilgiliKisi = string.Empty;
        // _ilgiliKisiNumarasi: İlgili kişi telefon numarası
        private string _ilgiliKisiNumarasi = string.Empty;
        // _ilgiliKisiEposta: İlgili kişi e-posta adresi
        private string _ilgiliKisiEposta = string.Empty;
        // _satisSozlesmesiMetni: Satış sözleşmesi metni
        private string _satisSozlesmesiMetni = string.Empty;

        // DovizKurlari: TCMB'den alınan döviz kurları
        public ObservableCollection<DovizKuru> DovizKurlari { get; set; }

        // Kurucu: Para birimleri, ürünler ve kurlar yüklenir, komutlar bağlanır
        public TeklifVerViewModel()
        {
            ParaBirimiListe = new ObservableCollection<string> { "TL", "USD", "EUR" };
            UrunleriYukle(); // Ürünleri yükler
            SepeteEkleCommand = new RelayCommand<Urun>(SepeteEkle, CanSepeteEkle); // Ürün ekleme komutu
            SepettenCikarCommand = new RelayCommand<TeklifUrunModel>(SepettenCikar); // Ürün çıkarma komutu
            KaydetVePdfIndirCommand = new RelayCommand(KaydetVePdfIndir); // Kaydet ve PDF oluştur komutu
            DovizKurlari = new ObservableCollection<DovizKuru>();
            DovizKurlariGuncelle(); // Döviz kurlarını günceller
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
               HALK BANKASI TR51 0001 2009 4140 0010 2645 69"";"; // Varsayılan satış sözleşmesi metni (kısaltıldı)
        }

        // SatisSozlesmesiMetni: Satış sözleşmesi metni, UI ile bağlı
        public string SatisSozlesmesiMetni
        {
            get => _satisSozlesmesiMetni;
            set { _satisSozlesmesiMetni = value; OnPropertyChanged(); }
        }

        // DovizKurlariGuncelle: TCMB'den kurları çeker ve günceller
        private void DovizKurlariGuncelle()
        {
            var kurListesi = DovizServisi.KurListesiniGetir();
            DovizKurlari.Clear();
            foreach (var kur in kurListesi) DovizKurlari.Add(kur);
        }

        // FirmaArama: Firma arama metni, değiştiğinde firmayı bulur
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

        // FirmaBilgisi: Seçilen müşteri bilgileri, UI ile bağlı
        public Musteri? FirmaBilgisi
        {
            get => _firmaBilgisi;
            set { _firmaBilgisi = value; OnPropertyChanged(); }
        }

        // UrunArama: Ürün arama metni, değiştiğinde ürünleri filtreler
        public string UrunArama
        {
            get => _urunArama;
            set { _urunArama = value; OnPropertyChanged(); UrunleriFiltrele(); }
        }

        // IlgiliKisi: İlgili kişi adı, UI ile bağlı
        public string IlgiliKisi
        {
            get => _ilgiliKisi;
            set { _ilgiliKisi = value; OnPropertyChanged(); }
        }

        // IlgiliKisiNumarasi: İlgili kişi telefon numarası, UI ile bağlı
        public string IlgiliKisiNumarasi
        {
            get => _ilgiliKisiNumarasi;
            set { _ilgiliKisiNumarasi = value; OnPropertyChanged(); }
        }

        // IlgiliKisiEposta: İlgili kişi e-posta adresi, UI ile bağlı
        public string IlgiliKisiEposta
        {
            get => _ilgiliKisiEposta;
            set { _ilgiliKisiEposta = value; OnPropertyChanged(); }
        }

        // TumUrunler: Tüm ürünlerin listesi
        public ObservableCollection<Urun> TumUrunler { get; set; } = [];
        // FiltrelenmisUrunler: Filtrelenmiş ürünlerin listesi
        public ObservableCollection<Urun> FiltrelenmisUrunler { get; set; } = [];
        // SecilenUrunler: Sepete eklenen ürünlerin listesi
        public ObservableCollection<TeklifUrunModel> SecilenUrunler { get; set; } = [];

        // ParaBirimiListe: Para birimi seçenekleri, UI ile bağlı
        public ObservableCollection<string> ParaBirimiListe
        {
            get => _paraBirimiListe;
            set { _paraBirimiListe = value; OnPropertyChanged(); }
        }

        // SelectedCurrency: Seçilen para birimi, değiştiğinde fiyatları günceller
        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set { _selectedCurrency = value; OnPropertyChanged(); RecalculateAll(); }
        }

        // UrunleriYukle: Veritabanından ürünleri yükler
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

        // UrunleriFiltrele: Arama metnine göre ürünleri filtreler
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

        // Model_OnBirimFiyatDegisti: Ürün fiyat/adet değiştiğinde toplamları günceller
        private void Model_OnBirimFiyatDegisti(object? sender, EventArgs e)
        {
            if (sender is TeklifUrunModel model)
            {
                // ilgili modelin gösterim metinlerini güncelle
                model.BirimFiyatText = FormatPrice(model.BirimFiyat);
                model.IndirimliFiyatText = FormatPrice(model.IndirimliFiyat);
                model.ToplamText = FormatPrice(model.Toplam);

                // ve toplamları yenile
                OnPropertyChanged(nameof(ToplamFiyat));
                OnPropertyChanged(nameof(KdvUcreti));
                OnPropertyChanged(nameof(GenelToplam));
                UpdateTotalsText();

            }
        }

        // SepeteEkleCommand: Ürünü sepete ekler
        public RelayCommand<Urun> SepeteEkleCommand { get; }
        // SepettenCikarCommand: Ürünü sepetten çıkarır
        public RelayCommand<TeklifUrunModel> SepettenCikarCommand { get; }
        // KaydetVePdfIndirCommand: Teklifi kaydeder ve PDF oluşturur
        public RelayCommand KaydetVePdfIndirCommand { get; }

        // CanSepeteEkle: Ürün ekleme komutunun çalışabilirliğini kontrol eder
        private bool CanSepeteEkle(Urun? urun) => urun != null;

        // SepeteEkle: Ürünü sepete ekler veya mevcutsa adedini artırır
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

        // SepettenCikar: Ürünü sepetten çıkarır
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

        // HesaplaIndirimliFiyat: Ürün için indirimli fiyatı hesaplar
        private void HesaplaIndirimliFiyat(TeklifUrunModel model)
        {
            decimal indirim = GenelIndirimOrani / 100;
            model.IndirimliFiyat = model.BirimFiyat * (1 - indirim);
            OnPropertyChanged(nameof(SecilenUrunler));
        }

        // GetFiyatByCurrency: Ürün fiyatını seçilen para birimine göre döndürür
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
                "USD" => new CultureInfo("en-US"),   // $ 
                "EUR" => new CultureInfo("de-DE"),   // € (almanya formatı, istersen "fr-FR" veya "en-IE" ile değiştir)
                _ => new CultureInfo("tr-TR"),       // ₺
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



        // GenelIndirimOrani: Teklifin genel indirim oranı
        private decimal _genelIndirimOrani = 0;
        public decimal GenelIndirimOrani
        {
            get => _genelIndirimOrani;
            set { _genelIndirimOrani = value < 0 ? 0 : value; OnPropertyChanged(); RecalculateAll(); }
        }

        // KdvOrani: Teklifin KDV oranı
        private decimal _kdvOrani = 20;
        public decimal KdvOrani
        {
            get => _kdvOrani;
            set { _kdvOrani = value < 0 ? 0 : value; OnPropertyChanged(); RecalculateAll(); }
        }

        // RecalculateAll: Para birimi veya indirim değiştiğinde tüm fiyatları günceller
        private void RecalculateAll()
        {
            foreach (var urun in SecilenUrunler)
            {
                var matchedUrun = TumUrunler.FirstOrDefault(u => u.UrunId == urun.UrunId);
                if (matchedUrun != null)
                {
                    urun.BirimFiyat = GetFiyatByCurrency(matchedUrun, SelectedCurrency);
                    HesaplaIndirimliFiyat(urun);

                    // UI'ya gösterilecek formatlı metinleri de setle
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

            // string toplamları da güncelle
            UpdateTotalsText();
        }


        // ToplamFiyat: Sepetteki ürünlerin toplam fiyatı
        public decimal ToplamFiyat => SecilenUrunler.Sum(u => u.Toplam);
        // KdvUcreti: Toplam fiyat üzerinden KDV ücreti
        public decimal KdvUcreti => ToplamFiyat * (KdvOrani / 100);
        // GenelToplam: Toplam fiyat + KDV
        public decimal GenelToplam => ToplamFiyat + KdvUcreti;

        // KaydetVePdfIndir: Teklifi kaydeder ve PDF oluşturur
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
                    Font firmaFont = new(baseFont, 12, Font.NORMAL, new BaseColor(128, 128, 128));
                    Font headerFont = new(baseFont, 12, Font.BOLD, BaseColor.BLACK);
                    Font bodyFont = new(baseFont, 11, Font.NORMAL, BaseColor.BLACK);
                    Font urunBaslikFont = new(baseFont, 14, Font.BOLD, BaseColor.BLACK);
                    Font sozlesmeBaslikFont = new(baseFont, 16, Font.BOLD, BaseColor.BLACK);
                    Font sozlesmeFont = new(baseFont, 12, Font.NORMAL, BaseColor.BLACK);

                    int currentPage = 2;
                    PdfContentByte canvas = stamper.GetOverContent(currentPage);

                    // Firma bilgileri tablosu
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

                    // Teklif bilgileri tablosu
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
                    teklifTable.WriteSelectedRows(0, -1, 330, 730, canvas);

                    // Çizgiler
                    canvas.SetColorFill(new BaseColor(200, 200, 200));
                    canvas.Rectangle(22.5f, 590, 550, 1);
                    canvas.Fill();
                    canvas.SetColorFill(new BaseColor(200, 200, 200));
                    canvas.Rectangle(22.5f, 555, 550, 1);
                    canvas.Fill();

                    // Ürünler başlığı
                    Phrase urunBaslik = new Phrase("Teklif Edilen Ürünler", urunBaslikFont);
                    ColumnText ctUrunBaslik = new ColumnText(canvas);
                    ctUrunBaslik.SetSimpleColumn(urunBaslik, 22.5f, 560f, 572.5f, 580f, 15, Element.ALIGN_CENTER);
                    ctUrunBaslik.Go();

                    // Ürün tablosu
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
                        AddCellToBody(table, urun.IndirimliFiyat.ToString("C2"), bodyFont, rowColor);
                        AddCellToBody(table, urun.Toplam.ToString("C2"), bodyFont, rowColor);
                        rowCount++;
                        urunNo++;
                    }
                    table.WriteSelectedRows(0, -1, 22.5f, 520, canvas);

                    // Toplamlar tablosu
                    PdfPTable toplamTable = new PdfPTable(2) { TotalWidth = 240f, DefaultCell = { Border = 0 }, HorizontalAlignment = Element.ALIGN_RIGHT };
                    toplamTable.SetWidths(new float[] { 3f, 2f });
                    toplamTable.AddCell(CreateRightAlignedHeaderCell($"İndirimli Toplam(%{GenelIndirimOrani}):", headerFont));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(ToplamFiyat.ToString("C2"), headerFont));
                    toplamTable.AddCell(CreateRightAlignedHeaderCell($"KDV (%{KdvOrani}):", headerFont));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(KdvUcreti.ToString("C2"), headerFont));
                    toplamTable.AddCell(CreateRightAlignedHeaderCell("Genel Toplam:", headerFont));
                    toplamTable.AddCell(CreateLeftAlignedBodyCell(GenelToplam.ToString("C2"), headerFont));
                    toplamTable.WriteSelectedRows(0, -1, 352.5f, 150, canvas);

                    // Yeni sayfa ekleyip satış sözleşmesini yazar
                    stamper.InsertPage(reader.NumberOfPages + 1, reader.GetPageSizeWithRotation(1));
                    currentPage = reader.NumberOfPages;
                    canvas = stamper.GetOverContent(currentPage);
                    Phrase sozlesmeBaslik = new Phrase("SATIŞ SÖZLEŞMESİ", sozlesmeBaslikFont);
                    ColumnText ctSozlesmeBaslik = new ColumnText(canvas);
                    ctSozlesmeBaslik.SetSimpleColumn(sozlesmeBaslik, 22.5f, 780f, 572.5f, 800f, 15, Element.ALIGN_CENTER);
                    ctSozlesmeBaslik.Go();
                    ColumnText ctSozlesme = new ColumnText(canvas);
                    ctSozlesme.SetSimpleColumn(new Phrase(SatisSozlesmesiMetni, sozlesmeFont), 40f, 50f, 550f, 760f, 18, Element.ALIGN_LEFT);
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

        // AddCellToHeader: PDF tablosuna başlık hücresi ekler
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

        // AddCellToBody: PDF tablosuna veri hücresi ekler
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

        // CreateRightAlignedHeaderCell: Sağ hizalı başlık hücresi oluşturur
        private PdfPCell CreateRightAlignedHeaderCell(string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = 0, PaddingRight = 5 };
            return cell;
        }

        // CreateLeftAlignedBodyCell: Sol hizalı veri hücresi oluşturur
        private PdfPCell CreateLeftAlignedBodyCell(string text, Font font)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font)) { HorizontalAlignment = Element.ALIGN_LEFT, Border = 0 };
            return cell;
        }

        // PropertyChanged: UI veri bağlama için özellik değişim olayı
        public event PropertyChangedEventHandler? PropertyChanged;
        // OnPropertyChanged: Özellik değiştiğinde UI’yi günceller
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}
using teklif_programi.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private readonly TeklifService _teklifService = new();
        private string _urunArama = string.Empty;
        private string _selectedCurrency = "TL";
        private ObservableCollection<string> _paraBirimiListe = new();
        private ObservableCollection<string> _dilListe = new();
        private string _selectedLanguage = "TR";
        private string _ilgiliKisi = string.Empty;
        private string _ilgiliKisiNumarasi = string.Empty;
        private string _ilgiliKisiEposta = string.Empty;
        private string _satisSozlesmesiMetni = string.Empty;

        public ObservableCollection<DovizKuru> DovizKurlari { get; set; }

        public TeklifVerViewModel()
        {
            ParaBirimiListe = new ObservableCollection<string> { "TL", "USD", "EUR" };
            DilListe = new ObservableCollection<string> { "TR", "EN" };
            SelectedLanguage = "TR";
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

        public ObservableCollection<string> DilListe
        {
            get => _dilListe;
            set { _dilListe = value; OnPropertyChanged(); }
        }

        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set { _selectedCurrency = value; OnPropertyChanged(); RecalculateAll(); }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set { _selectedLanguage = value; OnPropertyChanged(); UpdateDescriptions(); }
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
                    u.UrunAciklamasi.Contains(UrunArama, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(u.UrunAciklamasiEn) && u.UrunAciklamasiEn.Contains(UrunArama, StringComparison.OrdinalIgnoreCase))));
            OnPropertyChanged(nameof(FiltrelenmisUrunler));
        }

        private void UpdateDescriptions()
        {
            foreach (var model in SecilenUrunler)
            {
                model.UrunAciklamasi = SelectedLanguage == "EN" ? model.UrunAciklamasiEn : model.UrunAciklamasiTr;
            }
            OnPropertyChanged(nameof(SecilenUrunler));
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
                    UrunAciklamasiTr = urun.UrunAciklamasi,
                    UrunAciklamasiEn = urun.UrunAciklamasiEn,
                    UrunAciklamasi = SelectedLanguage == "EN" ? urun.UrunAciklamasiEn : urun.UrunAciklamasi,
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
            UpdateDescriptions();
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
                _teklifService.KaydetVePdfIndir(FirmaBilgisi, SecilenUrunler, GenelIndirimOrani, KdvOrani, SelectedCurrency, SatisSozlesmesiMetni);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu: {ex.Message}\nİç Hata: {ex.InnerException?.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}
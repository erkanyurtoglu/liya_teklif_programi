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
        private string _teslimatSekli = string.Empty;
        private string _teslimatYeri = string.Empty;
        private readonly Dictionary<int, decimal> _bekleyenMaliyetGuncellemeleri = new();

        private const string SatisSozlesmesiTr =
        @"
        1. Fiyatımız DOLAR cinsinden belirtilmiş olup, KDV dahildir. İş bu fatura ödemesinin, ödeme tarihindeki TCMB DÖVİZ EFEKTİF SATIŞ KURU ile Türk Lirası’na çevrilerek yapılması gerekmektedir. Aksi durumda kesilecek kur farkı faturasının tahsili yapılacaktır.
        2. Cihaz ücreti: %30’u sipariş sırasında peşin, kalan tutar teslimatta ödenecektir.
        3. Cihazlar; 1 yıl mekanik, 2 yıl elektronik parça olarak ücretsiz servis garantilidir. 10 yıl süreyle ücreti karşılığı teknik servis ve eğitim hizmeti verilecektir.
        4. Cihaz Teslimatı: Siparişe istinaden 1 hafta içinde teslim.
        5. Teklif Opsiyonu: Teklif tarihinden itibaren 3 gündür.
        6. Nakliye: Alıcı firmaya aittir.
        7. Alternatif olarak sunulan cihaz bedelleri, toplam teklif tutarına dahil edilmemiştir.
        8. Banka Bilgilerimiz: Liya Laboratuvar Test Cihazları İmalat ve Dış Ticaret A.Ş.
            - İŞ BANKASI TR16 0006 4000 0014 1520 1653 38
            - HALK BANKASI TR51 0001 2009 4140 0010 2645 69";

        private const string SatisSozlesmesiEn =
        @"
        1. Prices are given in USD.  
        2. Mode of payment: 100% bank transfer in advance as order confirmation.  
        3. Warranty: 1 year.  
        4. Delivery: Ex-Works Ankara, Turkey in 2–3 weeks after payment date.  
        5. Quotation valid until 26/03/2025.  
        6. Freight costs are given as Ex-Works.  
        7. Calibration will be charged separately.  
        8. Installation of equipment and training will be charged separately. The air freight and accommodation belong to buyer.  
           150 USD subsistence should be paid for a technical personnel per day.  
        9. Bank details:  
           - Bank name: Türkiye Halkbankası A.Ş.  
           - Bank address: İvedik Mah. 1368. Cad. Daire:61/C Yenimahalle/Ankara/Turkey  
           - Branch name: İvedik Organize Sanayi  
           - Branch code: 0414  
           - Account name: Liya Test Laboratuvar Cih. İmlt Dış Tic. Ltd. Şti.  
           - Swift code: TRHBTR2A  
           - IBAN number: TR68 0001 2009 4140 0053 0008 14 (USD)  
           - IBAN number: TR74 0001 2009 4140 0058 0006 68 (EUR)";



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
            UpdateContractText();
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

        public string TeslimatSekli
        {
            get => _teslimatSekli;
            set { _teslimatSekli = value; OnPropertyChanged(); }
        }

        public string TeslimatYeri
        {
            get => _teslimatYeri;
            set { _teslimatYeri = value; OnPropertyChanged(); }
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
            set { _selectedCurrency = value; OnPropertyChanged(); UpdatePricesByCurrency(); }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged();
                    UpdateDescriptions();
                    UpdateContractText();
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
                var preferred = SelectedLanguage == "EN" ? model.UrunAciklamasiEn : model.UrunAciklamasiTr;
                var fallback = SelectedLanguage == "EN" ? model.UrunAciklamasiTr : model.UrunAciklamasiEn;
                model.UrunAciklamasi = !string.IsNullOrWhiteSpace(preferred)
                    ? preferred!
                    : (!string.IsNullOrWhiteSpace(fallback) ? fallback! : model.UrunAciklamasi);
            }
            OnPropertyChanged(nameof(SecilenUrunler));
        }

        private void UpdateContractText()   
        {
            SatisSozlesmesiMetni = SelectedLanguage == "EN" ? SatisSozlesmesiEn : SatisSozlesmesiTr;
        }

        private void Model_OnBirimFiyatDegisti(object? sender, string propertyName)
        {
            if (sender is TeklifUrunModel model)
            {
                if (propertyName == nameof(TeklifUrunModel.BirimFiyat))
                    HesaplaIndirimliFiyat(model);

                if (propertyName == nameof(TeklifUrunModel.MaliyetFiyati))
                {
                    var maliyetTl = ConvertSelectedCurrencyToTl(model.MaliyetFiyati);
                    model.MaliyetFiyatiTl = maliyetTl;

                    if (model.UrunId > 0)
                    {
                        try
                        {
                            _bekleyenMaliyetGuncellemeleri[model.UrunId] = maliyetTl;

                            var tumUrun = TumUrunler.FirstOrDefault(u => u.UrunId == model.UrunId);
                            if (tumUrun != null)
                                tumUrun.MaliyetFiyati = maliyetTl;

                            var filtreUrun = FiltrelenmisUrunler.FirstOrDefault(u => u.UrunId == model.UrunId);
                            if (filtreUrun != null)
                                filtreUrun.MaliyetFiyati = maliyetTl;
                        }
                        catch (Exception ex)
                        {
                            _bekleyenMaliyetGuncellemeleri.Remove(model.UrunId);
                            MessageBox.Show($"Maliyet fiyatı kaydedilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }

                model.BirimFiyatText = FormatPrice(model.BirimFiyat);
                model.IndirimliFiyatText = FormatPrice(model.IndirimliFiyat);
                model.ToplamText = FormatPrice(model.Toplam);
                model.MaliyetFiyatText = FormatPrice(model.MaliyetFiyati);

                OnPropertyChanged(nameof(ToplamFiyat));
                OnPropertyChanged(nameof(KdvUcreti));
                OnPropertyChanged(nameof(GenelToplam));
                OnPropertyChanged(nameof(ToplamMaliyet));
                OnPropertyChanged(nameof(KarTutari));
                OnPropertyChanged(nameof(KarOrani));
                UpdateTotalsText();
            }
        }


        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not TeklifUrunModel model)
                return;

            if (e.PropertyName == nameof(TeklifUrunModel.UrunAciklamasi))
            {
                if (SelectedLanguage.Equals("EN", StringComparison.OrdinalIgnoreCase))
                {
                    model.UrunAciklamasiEn = model.UrunAciklamasi;
                    if (string.IsNullOrWhiteSpace(model.UrunAciklamasiTr))
                        model.UrunAciklamasiTr = model.UrunAciklamasi;
                }
                else
                {
                    model.UrunAciklamasiTr = model.UrunAciklamasi;
                    if (string.IsNullOrWhiteSpace(model.UrunAciklamasiEn))
                        model.UrunAciklamasiEn = model.UrunAciklamasi;
                }
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
                    Kategori = urun.Kategori,
                    UrunAciklamasiTr = urun.UrunAciklamasi,
                    UrunAciklamasiEn = urun.UrunAciklamasiEn,
                    UrunAciklamasi = SelectedLanguage == "EN" && !string.IsNullOrWhiteSpace(urun.UrunAciklamasiEn)
                        ? urun.UrunAciklamasiEn
                        : urun.UrunAciklamasi,
                    FiyatTL = urun.FiyatTL,
                    FiyatUSD = urun.FiyatUSD,
                    FiyatEUR = urun.FiyatEUR,
                    BirimFiyat = GetFiyatByCurrency(urun, SelectedCurrency),
                    MaliyetFiyati = ConvertTlToSelectedCurrency(urun.MaliyetFiyati),
                    MaliyetFiyatiTl = urun.MaliyetFiyati,
                    Adet = 1,
                    ManuelEklenen = false
                };
                model.OnBirimFiyatDegisti += Model_OnBirimFiyatDegisti;
                model.PropertyChanged += Model_PropertyChanged;
                HesaplaIndirimliFiyat(model);
                SecilenUrunler.Add(model);
                model.BirimFiyatText = FormatPrice(model.BirimFiyat);
                model.IndirimliFiyatText = FormatPrice(model.IndirimliFiyat);
                model.ToplamText = FormatPrice(model.Toplam);
                model.MaliyetFiyatText = FormatPrice(model.MaliyetFiyati);
            }
            OnPropertyChanged(nameof(SecilenUrunler));
            OnPropertyChanged(nameof(ToplamFiyat));
            OnPropertyChanged(nameof(KdvUcreti));
            OnPropertyChanged(nameof(GenelToplam));
            OnPropertyChanged(nameof(ToplamMaliyet));
            OnPropertyChanged(nameof(KarTutari));
            OnPropertyChanged(nameof(KarOrani));
            UpdateTotalsText();
        }

        public void ManuelUrunEkle(Urun manualUrun)
        {
            if (manualUrun == null) return;

            var fiyatTl = manualUrun.FiyatTL > 0 ? manualUrun.FiyatTL : manualUrun.BirimFiyat;
            var fiyatUsd = manualUrun.FiyatUSD > 0 ? manualUrun.FiyatUSD : ConvertTlToCurrency(fiyatTl, "USD");
            var fiyatEur = manualUrun.FiyatEUR > 0 ? manualUrun.FiyatEUR : ConvertTlToCurrency(fiyatTl, "EUR");

            var model = new TeklifUrunModel
            {
                UrunId = 0,
                UrunKodu = manualUrun.UrunKodu,
                Kategori = manualUrun.Kategori,
                UrunAciklamasiTr = manualUrun.UrunAciklamasi,
                UrunAciklamasiEn = manualUrun.UrunAciklamasiEn,
                UrunAciklamasi = SelectedLanguage == "EN" && !string.IsNullOrWhiteSpace(manualUrun.UrunAciklamasiEn)
                    ? manualUrun.UrunAciklamasiEn
                    : manualUrun.UrunAciklamasi,
                FiyatTL = fiyatTl,
                FiyatUSD = fiyatUsd,
                FiyatEUR = fiyatEur,
                BirimFiyat = SelectedCurrency switch
                {
                    "USD" => fiyatUsd,
                    "EUR" => fiyatEur,
                    _ => fiyatTl
                },
                MaliyetFiyati = ConvertTlToSelectedCurrency(manualUrun.MaliyetFiyati),
                MaliyetFiyatiTl = manualUrun.MaliyetFiyati,
                Adet = 1,
                ManuelEklenen = true
            };

            model.OnBirimFiyatDegisti += Model_OnBirimFiyatDegisti;
            model.PropertyChanged += Model_PropertyChanged;
            HesaplaIndirimliFiyat(model);
            SecilenUrunler.Add(model);

            model.BirimFiyatText = FormatPrice(model.BirimFiyat);
            model.IndirimliFiyatText = FormatPrice(model.IndirimliFiyat);
            model.ToplamText = FormatPrice(model.Toplam);
            model.MaliyetFiyatText = FormatPrice(model.MaliyetFiyati);

            OnPropertyChanged(nameof(SecilenUrunler));
            OnPropertyChanged(nameof(ToplamFiyat));
            OnPropertyChanged(nameof(KdvUcreti));
            OnPropertyChanged(nameof(GenelToplam));
            OnPropertyChanged(nameof(ToplamMaliyet));
            OnPropertyChanged(nameof(KarTutari));
            OnPropertyChanged(nameof(KarOrani));
            UpdateTotalsText();
        }


        private void SepettenCikar(TeklifUrunModel? urun)
        {
            if (urun != null)
            {
                urun.OnBirimFiyatDegisti -= Model_OnBirimFiyatDegisti;
                urun.PropertyChanged -= Model_PropertyChanged;
                SecilenUrunler.Remove(urun);
                UpdateTotalsText();
                OnPropertyChanged(nameof(SecilenUrunler));
                OnPropertyChanged(nameof(ToplamFiyat));
                OnPropertyChanged(nameof(KdvUcreti));
                OnPropertyChanged(nameof(GenelToplam));
                OnPropertyChanged(nameof(ToplamMaliyet));
                OnPropertyChanged(nameof(KarTutari));
                OnPropertyChanged(nameof(KarOrani));
            }
        }

        private void HesaplaIndirimliFiyat(TeklifUrunModel model)
        {
            // İndirimli fiyat hesaplamasını merkezi hesaba aktar
            model.IndirimliFiyat = TeklifHesaplayici.HesaplaIndirimliFiyat(model.BirimFiyat, GenelIndirimOrani);
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

        private decimal GetCurrencyRate(string currency)
        {
            var rate = DovizKurlari.FirstOrDefault(k => k.DovizCinsi == currency)?.Satis ?? 0;
            return rate <= 0 ? 1 : rate;
        }


        private decimal ConvertTlToSelectedCurrency(decimal tlValue)
        {
            return SelectedCurrency switch
            {
                "USD" => tlValue / GetCurrencyRate("USD"),
                "EUR" => tlValue / GetCurrencyRate("EUR"),
                _ => tlValue
            };
        }

        private decimal ConvertTlToCurrency(decimal tlValue, string currency)
        {
            return currency switch
            {
                "USD" => tlValue / GetCurrencyRate("USD"),
                "EUR" => tlValue / GetCurrencyRate("EUR"),
                _ => tlValue
            };
        }


        private decimal ConvertSelectedCurrencyToTl(decimal value)
        {
            return SelectedCurrency switch
            {
                "USD" => value * GetCurrencyRate("USD"),
                "EUR" => value * GetCurrencyRate("EUR"),
                _ => value
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

        private string _toplamMaliyetText = string.Empty;
        public string ToplamMaliyetText { get => _toplamMaliyetText; set { _toplamMaliyetText = value; OnPropertyChanged(); } }

        private string _karTutariText = string.Empty;
        public string KarTutariText { get => _karTutariText; set { _karTutariText = value; OnPropertyChanged(); } }

        private string _karOraniText = string.Empty;
        public string KarOraniText { get => _karOraniText; set { _karOraniText = value; OnPropertyChanged(); } }


        private string _toplamFiyatText = string.Empty;
        public string ToplamFiyatText { get => _toplamFiyatText; set { _toplamFiyatText = value; OnPropertyChanged(); } }

        private string _kdvUcretiText = string.Empty;
        public string KdvUcretiText { get => _kdvUcretiText; set { _kdvUcretiText = value; OnPropertyChanged(); } }

        private string _genelToplamText = string.Empty;
        public string GenelToplamText { get => _genelToplamText; set { _genelToplamText = value; OnPropertyChanged(); } }
        private string _paketlemeUcretText = string.Empty;
        public string PaketlemeUcretText { get => _paketlemeUcretText; set { _paketlemeUcretText = value; OnPropertyChanged(); } }

        private string _tasimaUcretText = string.Empty;
        public string TasimaUcretText { get => _tasimaUcretText; set { _tasimaUcretText = value; OnPropertyChanged(); } }

        private void UpdateTotalsText()
        {
            ToplamMaliyetText = FormatPrice(ToplamMaliyet);
            KarTutariText = FormatPrice(KarTutari);
            KarOraniText = KarOrani.ToString("F2") + "%";
            ToplamFiyatText = FormatPrice(ToplamFiyat);
            KdvUcretiText = FormatPrice(KdvUcreti);
            GenelToplamText = FormatPrice(GenelToplam);
            PaketlemeUcretText = FormatPrice(PaketlemeUcret);
            TasimaUcretText = FormatPrice(TasimaUcret);
        }

        private decimal _genelIndirimOrani = 0;
        public decimal GenelIndirimOrani
        {
            get => _genelIndirimOrani;
            set { _genelIndirimOrani = Math.Clamp(value, 0, 100); OnPropertyChanged(); RecalculateAll(); }
        }

        private decimal _kdvOrani = 20;
        public decimal KdvOrani
        {
            get => _kdvOrani;
            set { _kdvOrani = Math.Clamp(value, 0, 100); OnPropertyChanged(); RecalculateAll(); }
        }

        private decimal _paketlemeUcret = 0;
        public decimal PaketlemeUcret
        {
            get => _paketlemeUcret;
            set
            {
                _paketlemeUcret = Math.Max(0, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(GenelToplam));
                UpdateTotalsText();
            }
        }

        private decimal _tasimaUcret = 0;
        public decimal TasimaUcret
        {
            get => _tasimaUcret;
            set
            {
                _tasimaUcret = Math.Max(0, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(GenelToplam));
                UpdateTotalsText();
            }
        }





        private void RecalculateAll()
        {
            foreach (var urun in SecilenUrunler)
            {
                HesaplaIndirimliFiyat(urun);
                urun.BirimFiyatText = FormatPrice(urun.BirimFiyat);
                urun.IndirimliFiyatText = FormatPrice(urun.IndirimliFiyat);
                urun.ToplamText = FormatPrice(urun.Toplam);
                urun.MaliyetFiyatText = FormatPrice(urun.MaliyetFiyati);
            }

            OnPropertyChanged(nameof(ToplamFiyat));
            OnPropertyChanged(nameof(KdvUcreti));
            OnPropertyChanged(nameof(GenelToplam));
            OnPropertyChanged(nameof(ToplamMaliyet));
            OnPropertyChanged(nameof(KarTutari));
            OnPropertyChanged(nameof(KarOrani));
            UpdateTotalsText();
            UpdateDescriptions();
        }

        private void UpdatePricesByCurrency()
        {
            foreach (var urun in SecilenUrunler)
            {
                if (urun.ManuelEklenen)
                {
                    var fiyatTl = urun.FiyatTL;
                    var fiyatUsd = urun.FiyatUSD > 0 ? urun.FiyatUSD : ConvertTlToCurrency(fiyatTl, "USD");
                    var fiyatEur = urun.FiyatEUR > 0 ? urun.FiyatEUR : ConvertTlToCurrency(fiyatTl, "EUR");

                    urun.FiyatUSD = fiyatUsd;
                    urun.FiyatEUR = fiyatEur;

                    urun.BirimFiyat = SelectedCurrency switch
                    {
                        "USD" => fiyatUsd,
                        "EUR" => fiyatEur,
                        _ => fiyatTl
                    };

                    urun.MaliyetFiyati = ConvertTlToSelectedCurrency(urun.MaliyetFiyatiTl);
                }
                else
                {
                    var matchedUrun = TumUrunler.FirstOrDefault(u => u.UrunId == urun.UrunId);
                    if (matchedUrun != null)
                    {
                        urun.FiyatTL = matchedUrun.FiyatTL;
                        urun.FiyatUSD = matchedUrun.FiyatUSD;
                        urun.FiyatEUR = matchedUrun.FiyatEUR;
                        urun.BirimFiyat = GetFiyatByCurrency(matchedUrun, SelectedCurrency);
                        urun.MaliyetFiyati = ConvertTlToSelectedCurrency(matchedUrun.MaliyetFiyati);
                        urun.MaliyetFiyatiTl = matchedUrun.MaliyetFiyati;
                    }
                }
            }
            RecalculateAll();
        }

        // Toplam ve KDV hesaplamaları merkezi hesaba devredildi
        public decimal ToplamFiyat => TeklifHesaplayici.HesaplaToplamFiyat(SecilenUrunler);
        public decimal KdvUcreti => TeklifHesaplayici.HesaplaKdv(ToplamFiyat, KdvOrani);
        public decimal GenelToplam => TeklifHesaplayici.HesaplaGenelToplam(ToplamFiyat, KdvOrani, PaketlemeUcret, TasimaUcret);
        public decimal ToplamMaliyet => TeklifHesaplayici.HesaplaToplamMaliyet(SecilenUrunler);
        public decimal KarTutari => TeklifHesaplayici.HesaplaKarTutari(ToplamFiyat, ToplamMaliyet);
        public decimal KarOrani => TeklifHesaplayici.HesaplaKarOrani(KarTutari, ToplamMaliyet);

        private void KaydetVePdfIndir()
        {
            if (FirmaBilgisi == null || !SecilenUrunler.Any())
            {
                MessageBox.Show(FirmaBilgisi == null ? "Lütfen bir firma seçin." : "Lütfen en az bir ürün ekleyin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!BekleyenMaliyetGuncellemeleriniKaydet())
            {
                return;
            }

            try
            {
                foreach (var urun in SecilenUrunler)
                {
                    if (SelectedLanguage.Equals("EN", StringComparison.OrdinalIgnoreCase))
                    {
                        urun.UrunAciklamasiEn = urun.UrunAciklamasi;
                        if (string.IsNullOrWhiteSpace(urun.UrunAciklamasiTr))
                            urun.UrunAciklamasiTr = urun.UrunAciklamasi;
                    }
                    else
                    {
                        urun.UrunAciklamasiTr = urun.UrunAciklamasi;
                        if (string.IsNullOrWhiteSpace(urun.UrunAciklamasiEn))
                            urun.UrunAciklamasiEn = urun.UrunAciklamasi;
                    }

                    if (string.IsNullOrWhiteSpace(urun.UrunAciklamasi))
                    {
                        urun.UrunAciklamasi = SelectedLanguage.Equals("EN", StringComparison.OrdinalIgnoreCase)
                            ? urun.UrunAciklamasiEn ?? string.Empty
                            : urun.UrunAciklamasiTr ?? string.Empty;
                    }
                }

                _teklifService.KaydetVePdfIndir(FirmaBilgisi,
                                              SecilenUrunler,
                                              GenelIndirimOrani,
                                              KdvOrani,
                                              PaketlemeUcret,
                                              TasimaUcret,
                                              SelectedCurrency,
                                              TeslimatSekli,
                                              TeslimatYeri,
                                              IlgiliKisi,
                                              IlgiliKisiNumarasi,
                                              IlgiliKisiEposta,
                                              SatisSozlesmesiMetni,
                                              SelectedLanguage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata oluştu: {ex.Message}\nİç Hata: {ex.InnerException?.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool BekleyenMaliyetGuncellemeleriniKaydet()
        {
            if (!_bekleyenMaliyetGuncellemeleri.Any())
                return true;

            try
            {
                var urunIdler = _bekleyenMaliyetGuncellemeleri.Keys.ToList();
                var urunler = _context.Urunler.Where(u => urunIdler.Contains(u.UrunId)).ToList();

                foreach (var urun in urunler)
                {
                    if (_bekleyenMaliyetGuncellemeleri.TryGetValue(urun.UrunId, out var maliyetTl))
                    {
                        urun.MaliyetFiyati = maliyetTl;
                    }
                }

                _context.SaveChanges();
                _bekleyenMaliyetGuncellemeleri.Clear();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Maliyet fiyatı kaydedilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }



        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}
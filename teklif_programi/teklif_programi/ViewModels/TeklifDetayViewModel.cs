using teklif_programi.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using teklif_programi.Data;
using teklif_programi.Models;
using teklif_programi.Services;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace teklif_programi.ViewModels
{
    public class TeklifDetayViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context;
        private readonly TeklifService _teklifService = new();
        private Teklif _teklif;
        private TeklifToplam? _teklifToplam;
        private string _urunArama = string.Empty;
        private string _satisSozlesmesiMetni = string.Empty;
        private string _selectedLanguage = "TR";
        private string _teslimatSekli = string.Empty;
        private string _teslimatYeri = string.Empty;
        private DateTime? _teslimatTarihi;
        private DateTime? _teslimTarihi;
        public ObservableCollection<Urun> TumUrunler { get; set; } = new();
        public ObservableCollection<Urun> FiltrelenmisUrunler { get; set; } = new();
        private ObservableCollection<TeklifUrunModel> _teklifUrunler = new();
        private readonly HashSet<int> _silinecekUrunIdSet = new();
        public ObservableCollection<DovizKuru> DovizKurlari { get; set; } = new();
        public ObservableCollection<string> DilListe { get; } = new() { "TR", "EN" };

        private const string SatisSozlesmesiTr = @"1.Fiyatımız DOLAR cinsinden belirtilmiş olup, KDV dahildir. Fatura kesim tarihinde geçerli olan TCMB efektif satış kuru esas alınacaktır.
        2. Cihaz ücreti: %30’u sipariş sırasında peşin, kalan tutar teslimatta ödenecektir.
        3. Cihazlar; 1 yıl mekanik, 2 yıl elektronik parça olarak ücretsiz servis garantilidir. 10 yıl süreyle ücreti karşılığı teknik servis ve eğitim hizmeti verilecektir.
        4. Cihaz Teslimatı: Siparişe istinaden 1 hafta içinde teslim
        5. Teklif Opsiyonu: Teklif tarihinden itibaren 3 gündür.
        6. Nakliye: Satıcı firmaya aittir.
        7. Alternatif olarak sunulan cihaz bedelleri, toplam teklif tutarına dahil edilmemiştir.
        8. Banka Bilgilerimiz: Liya Laboratuvar Test Cihazları İmalat ve Dış Ticaret A.Ş.
        İŞ BANKASI TR16 0006 4000 0014 1520 1653 38
        HALK BANKASI TR51 0001 2009 4140 0010 2645 69";

        private const string SatisSozlesmesiEn = @"1.Our price is quoted in USD and includes VAT. The effective selling exchange rate of the Central Bank of the Republic of Turkey (CBRT) valid on the invoice date will be applied.
        2.Device payment terms: 30% is payable in advance at the time of order, and the remaining amount upon delivery.
        3.The devices are covered by a warranty of 1 year for mechanical parts and 2 years for electronic parts. Technical service and training services will be provided for a period of 10 years on a paid basis.
        4.Delivery of the devices: Within 1 week following the order.
        5.Offer validity: The offer is valid for 3 days from the quotation date.
        6.Transportation: To be borne by the seller.
        7.Alternative device prices are not included in the total quotation amount.
        8.Bank Account Information: Liya Laboratuvar Test Cihazları İmalat ve Dış Ticaret A.Ş.
        İŞ BANK: TR16 0006 4000 0014 1520 1653 38
        HALK BANK: TR51 0001 2009 4140 0010 2645 69";


        public TeklifDetayViewModel(Teklif teklif)
        {
            _context = new TeklifDbContext();
            _teklif = teklif ?? throw new ArgumentNullException(nameof(teklif));
            _selectedLanguage = teklif.Dil;

            Durumlar = new ObservableCollection<string> { "Beklemede", "Kabul Edildi", "Reddedildi" };
            ParaBirimiListe = new ObservableCollection<string> { "TL", "USD", "EUR" };
            if (string.IsNullOrWhiteSpace(_teklif.ParaBirimi)) _teklif.ParaBirimi = "TL";

            KaydetCommand = new RelayCommand(Kaydet, CanKaydet);
            PdfIndirCommand = new RelayCommand(PdfIndir, CanPdfIndir);
            UretimListesiIndirCommand = new RelayCommand(UretimListesiIndir, CanPdfIndir);
            FarkliKaydetCommand = new RelayCommand(FarkliKaydet, CanFarkliKaydet);
            SepeteEkleCommand = new RelayCommand<Urun>(SepeteEkle, u => u != null);
            SepettenCikarCommand = new RelayCommand<TeklifUrunModel>(SepettenCikar, u => u != null);

            DovizKurlariGuncelle();
            UrunleriYukle();
            YukleTeklifDetaylari();
            UpdateContractText();
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
                TeslimatSekli = value?.TeslimatSekli ?? string.Empty;
                TeslimatYeri = value?.TeslimatYeri ?? string.Empty;
                TeslimatTarihi = value?.TeslimatTarihi;
                TeslimTarihi = value?.TeslimTarihi;
                RecalculateAll();
            }
        }

        public ObservableCollection<TeklifUrunModel> TeklifUrunler
        {
            get => _teklifUrunler;
            set { _teklifUrunler = value; OnPropertyChanged(); }
        }

        public TeklifToplam? TeklifToplam
        {
            get => _teklifToplam;
            set
            {
                _teklifToplam = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PaketlemeUcret));
                OnPropertyChanged(nameof(TasimaUcret));
                UpdateToplamlarText();
            }
        }

        public ObservableCollection<string> Durumlar { get; }
        public ObservableCollection<string> ParaBirimiListe { get; }

        public string SelectedDurum
        {
            get => Teklif?.Durum ?? "Beklemede";
            set
            {
                if (Teklif == null) return;
                if (Teklif.Durum != value)
                {
                    Teklif.Durum = value;
                    OnPropertyChanged();
                    try
                    {
                        _context.SaveChanges();
                        EventHub.RaiseTeklifGuncellendi(Teklif.TeklifId);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Durum güncellenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        public string SelectedCurrency
        {
            get => Teklif?.ParaBirimi ?? "TL";
            set
            {
                if (Teklif == null) return;
                if (Teklif.ParaBirimi != value)
                {
                    Teklif.ParaBirimi = value;
                    OnPropertyChanged();
                    UpdatePricesByCurrency();
                }
            }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    if (Teklif != null) Teklif.Dil = value;
                    OnPropertyChanged();
                    UpdateDescriptions();
                    UpdateContractText();
                }
            }
        }

        public string SatisSozlesmesiMetni
        {
            get => _satisSozlesmesiMetni;
            set { _satisSozlesmesiMetni = value; OnPropertyChanged(); }
        }

        public string TeslimatSekli
        {
            get => _teslimatSekli;
            set
            {
                _teslimatSekli = value;
                if (Teklif != null) Teklif.TeslimatSekli = value;
                OnPropertyChanged();
            }
        }

        public string TeslimatYeri
        {
            get => _teslimatYeri;
            set
            {
                _teslimatYeri = value;
                if (Teklif != null) Teklif.TeslimatYeri = value;
                OnPropertyChanged();
            }
        }

        public DateTime? TeslimatTarihi
        {
            get => _teslimatTarihi;
            set
            {
                _teslimatTarihi = value;
                if (Teklif != null) Teklif.TeslimatTarihi = value;
                OnPropertyChanged();
            }
        }

        public DateTime? TeslimTarihi
        {
            get => _teslimTarihi;
            set
            {
                _teslimTarihi = value;
                if (Teklif != null) Teklif.TeslimTarihi = value;
                OnPropertyChanged();
            }
        }



        public decimal GenelIndirimOrani
        {
            get => Teklif?.GenelIndirimOrani ?? 0;
            set
            {
                if (Teklif == null) return;
                var val = Math.Clamp(value, 0, 100);
                if (Teklif.GenelIndirimOrani != val)
                {
                    Teklif.GenelIndirimOrani = val;
                    OnPropertyChanged();
                    RecalculateAll();
                }
            }
        }

        public decimal KdvOrani
        {
            get => Teklif?.KdvOrani ?? 0;
            set
            {
                if (Teklif == null) return;
                var val = Math.Clamp(value, 0, 100);
                if (Teklif.KdvOrani != val)
                {
                    Teklif.KdvOrani = val;
                    OnPropertyChanged();
                    RecalculateAll();
                }
            }
        }

        public decimal PaketlemeUcret
        {
            get => TeklifToplam?.PaketlemeUcreti ?? 0;
            set
            {
                if (TeklifToplam == null) return;
                var val = Math.Max(0, value);
                if (TeklifToplam.PaketlemeUcreti != val)
                {
                    TeklifToplam.PaketlemeUcreti = val;
                    OnPropertyChanged();
                    UpdateToplamlarText();
                }
            }
        }

        public decimal TasimaUcret
        {
            get => TeklifToplam?.TasimaUcreti ?? 0;
            set
            {
                if (TeklifToplam == null) return;
                var val = Math.Max(0, value);
                if (TeklifToplam.TasimaUcreti != val)
                {
                    TeklifToplam.TasimaUcreti = val;
                    OnPropertyChanged();
                    UpdateToplamlarText();
                }
            }
        }


        public string PersonelAdiSoyadi => Teklif?.Personel != null ? Teklif.Personel.AdSoyad : "Personel bilgisi yok";

        public string IndirimliToplamText { get; private set; } = "₺0,00";
        public string KdvTutariText { get; private set; } = "₺0,00";
        public string GenelToplamText { get; private set; } = "₺0,00";
        public string ToplamMaliyetText { get; private set; } = "₺0,00";
        public string KarTutariText { get; private set; } = "₺0,00";
        public string KarOraniText { get; private set; } = "0%";

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

        public RelayCommand KaydetCommand { get; }
        public RelayCommand PdfIndirCommand { get; }
        public RelayCommand UretimListesiIndirCommand { get; }
        public RelayCommand FarkliKaydetCommand { get; }
        public RelayCommand<Urun> SepeteEkleCommand { get; }
        public RelayCommand<TeklifUrunModel> SepettenCikarCommand { get; }

        private void UrunleriYukle()
        {
            try
            {
                var list = _context.Urunler.AsNoTracking().ToList();
                TumUrunler = new ObservableCollection<Urun>(list);
                FiltrelenmisUrunler = new ObservableCollection<Urun>(list);
                OnPropertyChanged(nameof(TumUrunler));
                OnPropertyChanged(nameof(FiltrelenmisUrunler));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ürünler yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var q = UrunArama.Trim();
                FiltrelenmisUrunler = new ObservableCollection<Urun>(
                    TumUrunler.Where(u =>
                        (u.UrunKodu?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.UrunAciklamasi?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (!string.IsNullOrEmpty(u.UrunAciklamasiEn) && u.UrunAciklamasiEn.Contains(q, StringComparison.OrdinalIgnoreCase)))
                );
            }
            OnPropertyChanged(nameof(FiltrelenmisUrunler));
        }

        private void DovizKurlariGuncelle()
        {
            var kurListesi = DovizServisi.KurListesiniGetir();
            DovizKurlari.Clear();
            foreach (var kur in kurListesi) DovizKurlari.Add(kur);
        }

        private decimal ConvertTlToSelectedCurrency(decimal tlValue)
        {
            return SelectedCurrency switch
            {
                "USD" => tlValue / (DovizKurlari.FirstOrDefault(k => k.DovizCinsi == "USD")?.Satis ?? 1),
                "EUR" => tlValue / (DovizKurlari.FirstOrDefault(k => k.DovizCinsi == "EUR")?.Satis ?? 1),
                _ => tlValue
            };
        }

        private void YukleTeklifDetaylari()
        {
            try
            {
                var teklifFull = _context.Teklifler
                    .Include(t => t.Personel)
                    .Include(t => t.Musteri)
                    .FirstOrDefault(t => t.TeklifId == Teklif.TeklifId);
                if (teklifFull != null) Teklif = teklifFull;

                var satirlar = _context.TeklifUrunleri
                    .Include(tu => tu.Urun)
                    .Where(tu => tu.TeklifId == Teklif.TeklifId)
                    .ToList();

                TeklifUrunler.Clear();
                foreach (var s in satirlar)
                {
                    var m = new TeklifUrunModel
                    {
                        UrunId = s.UrunId,
                        UrunKodu = s.Urun?.UrunKodu ?? "Bilinmiyor",
                        UrunAciklamasi = s.Urun?.UrunAciklamasi ?? "Bilinmiyor",
                        UrunAciklamasiTr = s.Urun?.UrunAciklamasi ?? "Bilinmiyor",
                        UrunAciklamasiEn = s.Urun?.UrunAciklamasiEn ?? s.Urun?.UrunAciklamasi ?? "Bilinmiyor",
                        Adet = s.Adet,
                        BirimFiyat = s.BirimFiyat,
                        IndirimliFiyat = s.IndirimliBirimFiyat,
                        FiyatTL = s.Urun?.FiyatTL ?? 0,
                        FiyatUSD = s.Urun?.FiyatUSD ?? 0,
                        FiyatEUR = s.Urun?.FiyatEUR ?? 0,
                        MaliyetFiyati = ConvertTlToSelectedCurrency(s.Urun?.MaliyetFiyati ?? 0),
                        Tamamlandi = s.Tamamlandi ?? false,
                        UretimNotu = s.UretimNotu ?? string.Empty,
                    };
                    m.BirimFiyatText = FormatPrice(m.BirimFiyat);
                    m.IndirimliFiyatText = FormatPrice(m.IndirimliFiyat);
                    m.ToplamText = FormatPrice(m.Toplam);
                    m.MaliyetFiyatText = FormatPrice(m.MaliyetFiyati);

                    m.OnBirimFiyatDegisti += Model_OnBirimFiyatDegisti;
                    m.PropertyChanged += Model_PropertyChanged;
                    TeklifUrunler.Add(m);
                }

                TeklifToplam = _context.TeklifToplamlari.FirstOrDefault(tt => tt.TeklifId == Teklif.TeklifId)
                               ?? new TeklifToplam { TeklifId = Teklif.TeklifId, PaketlemeUcreti = 0, TasimaUcreti = 0 };


                UpdateDescriptions();
                RecalculateAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Teklif detayları yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SepeteEkle(Urun? urun)
        {
            if (urun == null) return;

            var mevcut = TeklifUrunler.FirstOrDefault(x => x.UrunId == urun.UrunId);
            if (mevcut != null)
            {
                mevcut.Adet++;
                mevcut.IndirimliFiyat = TeklifHesaplayici.HesaplaIndirimliFiyat(mevcut.BirimFiyat, Teklif.GenelIndirimOrani);
                mevcut.BirimFiyatText = FormatPrice(mevcut.BirimFiyat);
                mevcut.IndirimliFiyatText = FormatPrice(mevcut.IndirimliFiyat);
                mevcut.ToplamText = FormatPrice(mevcut.Toplam);
                mevcut.MaliyetFiyatText = FormatPrice(mevcut.MaliyetFiyati);
            }
            else
            {
                var birim = GetFiyatByCurrency(urun, SelectedCurrency);
                var m = new TeklifUrunModel
                {
                    UrunId = urun.UrunId,
                    UrunKodu = urun.UrunKodu,
                    UrunAciklamasiTr = urun.UrunAciklamasi,
                    UrunAciklamasiEn = urun.UrunAciklamasiEn ?? urun.UrunAciklamasi,
                    UrunAciklamasi = SelectedLanguage == "EN" ? (urun.UrunAciklamasiEn ?? urun.UrunAciklamasi) : urun.UrunAciklamasi,
                    Adet = 1,
                    BirimFiyat = birim,
                    IndirimliFiyat = TeklifHesaplayici.HesaplaIndirimliFiyat(birim, Teklif.GenelIndirimOrani),
                    FiyatTL = urun.FiyatTL,
                    FiyatUSD = urun.FiyatUSD,
                    FiyatEUR = urun.FiyatEUR,
                    MaliyetFiyati = ConvertTlToSelectedCurrency(urun.MaliyetFiyati),
                    Tamamlandi = false,
                    UretimNotu = string.Empty
                };
                m.BirimFiyatText = FormatPrice(m.BirimFiyat);
                m.IndirimliFiyatText = FormatPrice(m.IndirimliFiyat);
                m.ToplamText = FormatPrice(m.Toplam);
                m.MaliyetFiyatText = FormatPrice(m.MaliyetFiyati);

                m.OnBirimFiyatDegisti += Model_OnBirimFiyatDegisti;
                m.PropertyChanged += Model_PropertyChanged;

                TeklifUrunler.Add(m);
            }

            _silinecekUrunIdSet.Remove(urun.UrunId);

            UpdateToplamlarText();
            OnPropertyChanged(nameof(TeklifUrunler));
        }

        private void SepettenCikar(TeklifUrunModel? item)
        {
            if (item == null) return;

            var varMi = _context.TeklifUrunleri.Any(tu => tu.TeklifId == Teklif.TeklifId && tu.UrunId == item.UrunId);
            if (varMi) _silinecekUrunIdSet.Add(item.UrunId);

            TeklifUrunler.Remove(item);
            UpdateToplamlarText();
            OnPropertyChanged(nameof(TeklifUrunler));
        }

        private void RecalculateAll()
        {
            foreach (var urun in TeklifUrunler)
            {
                urun.IndirimliFiyat = TeklifHesaplayici.HesaplaIndirimliFiyat(urun.BirimFiyat, Teklif.GenelIndirimOrani);
                urun.BirimFiyatText = FormatPrice(urun.BirimFiyat);
                urun.IndirimliFiyatText = FormatPrice(urun.IndirimliFiyat);
                urun.ToplamText = FormatPrice(urun.Toplam);
                urun.MaliyetFiyatText = FormatPrice(urun.MaliyetFiyati);
            }
            UpdateToplamlarText();
        }

        private void UpdatePricesByCurrency()
        {
            foreach (var urun in TeklifUrunler)
            {
                urun.BirimFiyat = GetFiyatByCurrency(urun, SelectedCurrency);
                var dbUrun = TumUrunler.FirstOrDefault(u => u.UrunId == urun.UrunId);
                urun.MaliyetFiyati = ConvertTlToSelectedCurrency(dbUrun?.MaliyetFiyati ?? 0);
            }
            RecalculateAll();
        }


        private void UpdateToplamlarText()
        {
            if (TeklifToplam == null) return;

            var indirimliToplam = TeklifHesaplayici.HesaplaToplamFiyat(TeklifUrunler);
            var kdvTutari = TeklifHesaplayici.HesaplaKdv(indirimliToplam, Teklif.KdvOrani);
            var genelToplam = TeklifHesaplayici.HesaplaGenelToplam(indirimliToplam, Teklif.KdvOrani, TeklifToplam.PaketlemeUcreti, TeklifToplam.TasimaUcreti);

            var toplamMaliyet = TeklifHesaplayici.HesaplaToplamMaliyet(TeklifUrunler);
            var karTutari = TeklifHesaplayici.HesaplaKarTutari(indirimliToplam, toplamMaliyet);
            var karOrani = TeklifHesaplayici.HesaplaKarOrani(karTutari, toplamMaliyet);

            TeklifToplam.IndirimliToplam = indirimliToplam;
            TeklifToplam.KdvTutari = kdvTutari;
            TeklifToplam.GenelToplam = genelToplam;

            IndirimliToplamText = FormatPrice(indirimliToplam);
            KdvTutariText = FormatPrice(kdvTutari);
            GenelToplamText = FormatPrice(genelToplam);
            ToplamMaliyetText = FormatPrice(toplamMaliyet);
            KarTutariText = FormatPrice(karTutari);
            KarOraniText = karOrani.ToString("F2") + "%";

            OnPropertyChanged(nameof(TeklifToplam));
            OnPropertyChanged(nameof(IndirimliToplamText));
            OnPropertyChanged(nameof(KdvTutariText));
            OnPropertyChanged(nameof(GenelToplamText));
            OnPropertyChanged(nameof(ToplamMaliyetText));
            OnPropertyChanged(nameof(KarTutariText));
            OnPropertyChanged(nameof(KarOraniText));
        }

        private void Model_OnBirimFiyatDegisti(object? sender, string propertyName)
        {
            if (sender is TeklifUrunModel m)
            {
                if (propertyName == nameof(TeklifUrunModel.BirimFiyat))
                    m.IndirimliFiyat = TeklifHesaplayici.HesaplaIndirimliFiyat(m.BirimFiyat, Teklif.GenelIndirimOrani);

                m.BirimFiyatText = FormatPrice(m.BirimFiyat);
                m.IndirimliFiyatText = FormatPrice(m.IndirimliFiyat);
                m.ToplamText = FormatPrice(m.Toplam);
                m.MaliyetFiyatText = FormatPrice(m.MaliyetFiyati);
                UpdateToplamlarText();
            }
        }

        private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TeklifUrunModel && (e.PropertyName == nameof(TeklifUrunModel.Adet) || e.PropertyName == nameof(TeklifUrunModel.IndirimliFiyat)))
                UpdateToplamlarText();
        }

        private static decimal GetFiyatByCurrency(Urun urun, string currency) => currency switch
        {
            "USD" => urun.FiyatUSD,
            "EUR" => urun.FiyatEUR,
            _ => urun.FiyatTL
        };

        private void UpdateDescriptions()
        {
            foreach (var model in TeklifUrunler)
            {
                model.UrunAciklamasi = SelectedLanguage == "EN" ? model.UrunAciklamasiEn : model.UrunAciklamasiTr;
            }
            OnPropertyChanged(nameof(TeklifUrunler));
        }

        private void UpdateContractText()
        {
            SatisSozlesmesiMetni = SelectedLanguage == "EN" ? SatisSozlesmesiEn : SatisSozlesmesiTr;
        }


        private static decimal GetFiyatByCurrency(TeklifUrunModel urun, string currency) => currency switch
        {
            "USD" => urun.FiyatUSD,
            "EUR" => urun.FiyatEUR,
            _ => urun.FiyatTL
        };

        private string FormatPrice(decimal price) => price.ToString("C2", GetCulture(SelectedCurrency));
        private static CultureInfo GetCulture(string currency) => currency switch
        {
            "USD" => new CultureInfo("en-US"),
            "EUR" => new CultureInfo("en-IE"),
            _ => new CultureInfo("tr-TR"),
        };

        private bool CanKaydet() => Teklif != null;
        private void Kaydet()
        {
            if (Teklif == null) return;

            try
            {
                using var tr = _context.Database.BeginTransaction();

                var dbT = _context.Teklifler.Find(Teklif.TeklifId);
                if (dbT != null)
                {
                    dbT.Durum = Teklif.Durum;
                    dbT.MusteriNotu = Teklif.MusteriNotu;
                    dbT.ParaBirimi = Teklif.ParaBirimi;
                    dbT.GenelIndirimOrani = Teklif.GenelIndirimOrani;
                    dbT.KdvOrani = Teklif.KdvOrani;
                    dbT.Dil = Teklif.Dil;
                    dbT.IlgiliKisi = Teklif.IlgiliKisi;
                    dbT.IlgiliKisiTelefonu = Teklif.IlgiliKisiTelefonu;
                    dbT.IlgiliKisiEposta = Teklif.IlgiliKisiEposta;
                    dbT.TeslimatSekli = Teklif.TeslimatSekli;
                    dbT.TeslimatYeri = Teklif.TeslimatYeri;
                    dbT.TeslimatTarihi = Teklif.TeslimatTarihi;
                    dbT.TeslimTarihi = Teklif.TeslimTarihi;
                }

                if (_silinecekUrunIdSet.Count > 0)
                {
                    var silinecekler = _context.TeklifUrunleri
                        .Where(tu => tu.TeklifId == Teklif.TeklifId && _silinecekUrunIdSet.Contains(tu.UrunId))
                        .ToList();
                    _context.TeklifUrunleri.RemoveRange(silinecekler);
                    _silinecekUrunIdSet.Clear();
                }

                foreach (var m in TeklifUrunler)
                {
                    var dbU = _context.TeklifUrunleri.FirstOrDefault(tu => tu.TeklifId == Teklif.TeklifId && tu.UrunId == m.UrunId);
                    if (dbU != null)
                    {
                        dbU.Adet = m.Adet;
                        dbU.BirimFiyat = m.BirimFiyat;
                        dbU.IndirimliBirimFiyat = m.IndirimliFiyat;
                        dbU.ToplamTutar = m.Toplam;
                        dbU.Tamamlandi = m.Tamamlandi;
                        dbU.UretimNotu = m.UretimNotu;
                    }
                    else
                    {
                        _context.TeklifUrunleri.Add(new TeklifUrun
                        {
                            TeklifId = Teklif.TeklifId,
                            UrunId = m.UrunId,
                            Adet = m.Adet,
                            BirimFiyat = m.BirimFiyat,
                            IndirimliBirimFiyat = m.IndirimliFiyat,
                            ToplamTutar = m.Toplam,
                            Tamamlandi = m.Tamamlandi,
                            UretimNotu = m.UretimNotu
                        });
                    }
                }

                var dbTop = _context.TeklifToplamlari.FirstOrDefault(tt => tt.TeklifId == Teklif.TeklifId);
                if (dbTop != null)
                {
                    dbTop.IndirimliToplam = TeklifToplam?.IndirimliToplam ?? 0;
                    dbTop.KdvTutari = TeklifToplam?.KdvTutari ?? 0;
                    dbTop.PaketlemeUcreti = TeklifToplam?.PaketlemeUcreti ?? 0;
                    dbTop.TasimaUcreti = TeklifToplam?.TasimaUcreti ?? 0;
                    dbTop.GenelToplam = TeklifToplam?.GenelToplam ?? 0;
                }
                else
                {
                    _context.TeklifToplamlari.Add(new TeklifToplam
                    {
                        TeklifId = Teklif.TeklifId,
                        IndirimliToplam = TeklifToplam?.IndirimliToplam ?? 0,
                        KdvTutari = TeklifToplam?.KdvTutari ?? 0,
                        PaketlemeUcreti = TeklifToplam?.PaketlemeUcreti ?? 0,
                        TasimaUcreti = TeklifToplam?.TasimaUcreti ?? 0,
                        GenelToplam = TeklifToplam?.GenelToplam ?? 0
                    });
                }

                _context.SaveChanges();
                tr.Commit();

                EventHub.RaiseTeklifGuncellendi(Teklif.TeklifId);
                MessageBox.Show("Değişiklikler kaydedildi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kaydederken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanFarkliKaydet() => Teklif != null;
        private void FarkliKaydet()
        {
            if (Teklif == null) return;

            try
            {
                using var tr = _context.Database.BeginTransaction();

                var yeni = new Teklif
                {
                    MusteriId = Teklif.MusteriId,
                    PersonelId = Teklif.PersonelId,
                    OlusturmaTarihi = DateTime.Now,
                    Durum = "Beklemede",
                    ParaBirimi = Teklif.ParaBirimi,
                    GenelIndirimOrani = Teklif.GenelIndirimOrani,
                    KdvOrani = Teklif.KdvOrani,
                    Dil = Teklif.Dil,
                    MusteriNotu = Teklif.MusteriNotu,
                    IlgiliKisi = Teklif.IlgiliKisi,
                    IlgiliKisiTelefonu = Teklif.IlgiliKisiTelefonu,
                    IlgiliKisiEposta = Teklif.IlgiliKisiEposta,
                    TeslimatSekli = TeslimatSekli,
                    TeslimatYeri = TeslimatYeri,
                    TeslimatTarihi = TeslimatTarihi,
                    TeslimTarihi = TeslimTarihi

                };
                _context.Teklifler.Add(yeni);
                _context.SaveChanges();

                foreach (var m in TeklifUrunler)
                {
                    _context.TeklifUrunleri.Add(new TeklifUrun
                    {
                        TeklifId = yeni.TeklifId,
                        UrunId = m.UrunId,
                        Adet = m.Adet,
                        BirimFiyat = m.BirimFiyat,
                        IndirimliBirimFiyat = m.IndirimliFiyat,
                        ToplamTutar = m.Toplam,
                        Tamamlandi = m.Tamamlandi,
                        UretimNotu = m.UretimNotu
                    });
                }

                var indTop = TeklifUrunler.Sum(u => u.Toplam);
                var kdv = indTop * (yeni.KdvOrani / 100m);
                var genTop = indTop + kdv + (TeklifToplam?.PaketlemeUcreti ?? 0) + (TeklifToplam?.TasimaUcreti ?? 0);

                _context.TeklifToplamlari.Add(new TeklifToplam
                {
                    TeklifId = yeni.TeklifId,
                    IndirimliToplam = indTop,
                    KdvTutari = kdv,
                    PaketlemeUcreti = TeklifToplam?.PaketlemeUcreti ?? 0,
                    TasimaUcreti = TeklifToplam?.TasimaUcreti ?? 0,
                    GenelToplam = genTop
                });

                _context.SaveChanges();
                tr.Commit();

                EventHub.RaiseTeklifGuncellendi(yeni.TeklifId);
                MessageBox.Show($"Yeni teklif oluşturuldu. Teklif No: {yeni.TeklifId}",
                    "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Farklı kaydederken hata: {ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanPdfIndir() => Teklif != null;

        private void UretimListesiIndir()
        {
            if (Teklif == null) return;

            try
            {
                _teklifService.UretimListesiPdfIndir(Teklif, TeklifUrunler);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Üretim listesi oluşturulurken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PdfIndir()
        {
            if (Teklif == null) return;

            try
            {
                _teklifService.PdfIndir(Teklif, TeklifUrunler, SatisSozlesmesiMetni, SelectedLanguage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF oluşturulurken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}
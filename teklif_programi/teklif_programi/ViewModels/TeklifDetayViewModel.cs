using teklif_programi.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using iTextSharp.text;
using iTextSharp.text.pdf;
using teklif_programi.Data;
using teklif_programi.Models;
using teklif_programi.Services;
using teklif_programi.Helpers;

namespace teklif_programi.ViewModels
{
    public class TeklifDetayViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context;
        private Teklif _teklif;
        private TeklifToplam? _teklifToplam;

        // Sepet mantığı için yeni alanlar
        private string _urunArama = string.Empty;
        public ObservableCollection<Urun> TumUrunler { get; set; } = new();
        public ObservableCollection<Urun> FiltrelenmisUrunler { get; set; } = new();

        // Sepet (mevcut satırlar)
        private ObservableCollection<TeklifUrunModel> _teklifUrunler = new();

        // Silinecek satırları takip (DB’den kaldırmak için)
        private readonly HashSet<int> _silinecekUrunIdSet = new();

        public ObservableCollection<DovizKuru> DovizKurlari { get; set; } = new();

        public TeklifDetayViewModel(Teklif teklif)
        {
            _context = new TeklifDbContext();
            _teklif = teklif ?? throw new ArgumentNullException(nameof(teklif));

            Durumlar = new ObservableCollection<string> { "Beklemede", "Kabul Edildi", "Reddedildi" };
            ParaBirimiListe = new ObservableCollection<string> { "TL", "USD", "EUR" };
            if (string.IsNullOrWhiteSpace(_teklif.ParaBirimi)) _teklif.ParaBirimi = "TL";

            // Komutlar
            KaydetCommand = new RelayCommand(Kaydet, CanKaydet);
            PdfIndirCommand = new RelayCommand(PdfIndir, CanPdfIndir);
            FarkliKaydetCommand = new RelayCommand(FarkliKaydet, CanFarkliKaydet);
            SepeteEkleCommand = new RelayCommand<Urun>(SepeteEkle, u => u != null);
            SepettenCikarCommand = new RelayCommand<TeklifUrunModel>(SepettenCikar, u => u != null);

            // Veri
            DovizKurlariGuncelle();
            UrunleriYukle();
            YukleTeklifDetaylari();
        }

        // === Public bindings ===
        public Teklif Teklif
        {
            get => _teklif;
            set { _teklif = value; OnPropertyChanged(); OnPropertyChanged(nameof(PersonelAdiSoyadi)); OnPropertyChanged(nameof(SelectedCurrency)); RecalculateAll(); }
        }

        public ObservableCollection<TeklifUrunModel> TeklifUrunler
        {
            get => _teklifUrunler;
            set { _teklifUrunler = value; OnPropertyChanged(); }
        }

        public TeklifToplam? TeklifToplam
        {
            get => _teklifToplam;
            set { _teklifToplam = value; OnPropertyChanged(); UpdateToplamlarText(); }
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
                    RecalculateAll();
                }
            }
        }

        public decimal GenelIndirimOrani
        {
            get => Teklif?.GenelIndirimOrani ?? 0;
            set
            {
                if (Teklif == null) return;
                var val = value < 0 ? 0 : value;
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
                var val = value < 0 ? 0 : value;
                if (Teklif.KdvOrani != val)
                {
                    Teklif.KdvOrani = val;
                    OnPropertyChanged();
                    RecalculateAll();
                }
            }
        }


        public string PersonelAdiSoyadi => Teklif?.Personel != null ? Teklif.Personel.AdSoyad : "Personel bilgisi yok";

        // Toplamlar (formatlı)
        public string IndirimliToplamText { get; private set; } = "₺0,00";
        public string KdvTutariText { get; private set; } = "₺0,00";
        public string GenelToplamText { get; private set; } = "₺0,00";
        public string ToplamMaliyetText { get; private set; } = "₺0,00";
        public string KarTutariText { get; private set; } = "₺0,00";
        public string KarOraniText { get; private set; } = "0%";

        // Ürün arama (sol panel)
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

        // Komutlar
        public RelayCommand KaydetCommand { get; }
        public RelayCommand PdfIndirCommand { get; }
        public RelayCommand FarkliKaydetCommand { get; }
        public RelayCommand<Urun> SepeteEkleCommand { get; }
        public RelayCommand<TeklifUrunModel> SepettenCikarCommand { get; }

        // === Load ===
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
                // Teklif + ilişkiler
                var teklifFull = _context.Teklifler
                    .Include(t => t.Personel)
                    .Include(t => t.Musteri)
                    .FirstOrDefault(t => t.TeklifId == Teklif.TeklifId);
                if (teklifFull != null) Teklif = teklifFull;

                // Mevcut satırlar (sepet)
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
                        Adet = s.Adet,
                        BirimFiyat = s.BirimFiyat,
                        IndirimliFiyat = s.IndirimliBirimFiyat,
                        FiyatTL = s.Urun?.FiyatTL ?? 0,
                        FiyatUSD = s.Urun?.FiyatUSD ?? 0,
                        FiyatEUR = s.Urun?.FiyatEUR ?? 0,
                        MaliyetFiyati = ConvertTlToSelectedCurrency(s.Urun?.MaliyetFiyati ?? 0),
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
                               ?? new TeklifToplam { TeklifId = Teklif.TeklifId };

                RecalculateAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Teklif detayları yüklenirken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // === Sepet işlemleri ===
        private void SepeteEkle(Urun? urun)
        {
            if (urun == null) return;

            var mevcut = TeklifUrunler.FirstOrDefault(x => x.UrunId == urun.UrunId);
            if (mevcut != null)
            {
                mevcut.Adet++;
                // toplam/format tetiklensin
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
                    UrunAciklamasi = urun.UrunAciklamasi,
                    Adet = 1,
                    BirimFiyat = birim,
                    IndirimliFiyat = TeklifHesaplayici.HesaplaIndirimliFiyat(birim, Teklif.GenelIndirimOrani),
                    FiyatTL = urun.FiyatTL,
                    FiyatUSD = urun.FiyatUSD,
                    FiyatEUR = urun.FiyatEUR,
                    MaliyetFiyati = ConvertTlToSelectedCurrency(urun.MaliyetFiyati)
                };
                m.BirimFiyatText = FormatPrice(m.BirimFiyat);
                m.IndirimliFiyatText = FormatPrice(m.IndirimliFiyat);
                m.ToplamText = FormatPrice(m.Toplam);
                m.MaliyetFiyatText = FormatPrice(m.MaliyetFiyati);

                m.OnBirimFiyatDegisti += Model_OnBirimFiyatDegisti;
                m.PropertyChanged += Model_PropertyChanged;

                TeklifUrunler.Add(m);
            }

            // Eğer daha önce silinecekler listesine eklenmişse, geri al
            _silinecekUrunIdSet.Remove(urun.UrunId);

            UpdateToplamlarText();
            OnPropertyChanged(nameof(TeklifUrunler));
        }

        private void SepettenCikar(TeklifUrunModel? item)
        {
            if (item == null) return;

            // Mevcut DB’de varsa, silinecek olarak işaretle
            var varMi = _context.TeklifUrunleri.Any(tu => tu.TeklifId == Teklif.TeklifId && tu.UrunId == item.UrunId);
            if (varMi) _silinecekUrunIdSet.Add(item.UrunId);

            TeklifUrunler.Remove(item);
            UpdateToplamlarText();
            OnPropertyChanged(nameof(TeklifUrunler));
        }

        // === Hesap/Format ===
        private void RecalculateAll()
        {
            foreach (var urun in TeklifUrunler)
            {
                urun.BirimFiyat = GetFiyatByCurrency(urun);
                urun.IndirimliFiyat = TeklifHesaplayici.HesaplaIndirimliFiyat(urun.BirimFiyat, Teklif.GenelIndirimOrani);
                var dbUrun = TumUrunler.FirstOrDefault(u => u.UrunId == urun.UrunId);
                urun.MaliyetFiyati = ConvertTlToSelectedCurrency(dbUrun?.MaliyetFiyati ?? 0);

                urun.BirimFiyatText = FormatPrice(urun.BirimFiyat);
                urun.IndirimliFiyatText = FormatPrice(urun.IndirimliFiyat);
                urun.ToplamText = FormatPrice(urun.Toplam);
                urun.MaliyetFiyatText = FormatPrice(urun.MaliyetFiyati);
            }
            UpdateToplamlarText();
        }

        private void UpdateToplamlarText()
        {
            if (TeklifToplam == null) return;

            var indirimliToplam = TeklifUrunler.Sum(u => u.Toplam);
            var kdvTutari = indirimliToplam * (Teklif.KdvOrani / 100m);
            var genelToplam = indirimliToplam + kdvTutari;

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

        private void Model_OnBirimFiyatDegisti(object? sender, EventArgs e)
        {
            if (sender is TeklifUrunModel m)
            {
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

        private decimal GetFiyatByCurrency(Urun urun, string currency) => currency switch
        {
            "USD" => urun.FiyatUSD,
            "EUR" => urun.FiyatEUR,
            _ => urun.FiyatTL
        };

        private decimal GetFiyatByCurrency(TeklifUrunModel urun) => SelectedCurrency switch
        {
            "USD" => urun.FiyatUSD,
            "EUR" => urun.FiyatEUR,
            _ => urun.FiyatTL
        };


        private string FormatPrice(decimal price) => price.ToString("C2", GetCulture(SelectedCurrency));
        private CultureInfo GetCulture(string currency) => currency switch
        {
            "USD" => new CultureInfo("en-US"),
            "EUR" => new CultureInfo("en-IE"),
            _ => new CultureInfo("tr-TR"),
        };

        // === Kaydet / Farklı kaydet / PDF ===
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
                    dbT.IlgiliKisi = Teklif.IlgiliKisi;
                    dbT.IlgiliKisiTelefonu = Teklif.IlgiliKisiTelefonu;
                    dbT.IlgiliKisiEposta = Teklif.IlgiliKisiEposta;
                }

                // Silinecekler
                if (_silinecekUrunIdSet.Count > 0)
                {
                    var silinecekler = _context.TeklifUrunleri
                        .Where(tu => tu.TeklifId == Teklif.TeklifId && _silinecekUrunIdSet.Contains(tu.UrunId))
                        .ToList();
                    _context.TeklifUrunleri.RemoveRange(silinecekler);
                    _silinecekUrunIdSet.Clear();
                }

                // Satırları upsert
                foreach (var m in TeklifUrunler)
                {
                    var dbU = _context.TeklifUrunleri.FirstOrDefault(tu => tu.TeklifId == Teklif.TeklifId && tu.UrunId == m.UrunId);
                    if (dbU != null)
                    {
                        dbU.Adet = m.Adet;
                        dbU.BirimFiyat = m.BirimFiyat;
                        dbU.IndirimliBirimFiyat = m.IndirimliFiyat;
                        dbU.ToplamTutar = m.Toplam;
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
                            ToplamTutar = m.Toplam
                        });
                    }
                }

                // Toplamlar
                var dbTop = _context.TeklifToplamlari.FirstOrDefault(tt => tt.TeklifId == Teklif.TeklifId);
                if (dbTop != null)
                {
                    dbTop.IndirimliToplam = TeklifToplam?.IndirimliToplam ?? 0;
                    dbTop.KdvTutari = TeklifToplam?.KdvTutari ?? 0;
                    dbTop.GenelToplam = TeklifToplam?.GenelToplam ?? 0;
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
                    Durum = "Beklemede", // <<< her zaman beklemede başlasın
                    ParaBirimi = Teklif.ParaBirimi,
                    GenelIndirimOrani = Teklif.GenelIndirimOrani,
                    KdvOrani = Teklif.KdvOrani,
                    MusteriNotu = Teklif.MusteriNotu,
                    IlgiliKisi = Teklif.IlgiliKisi,
                    IlgiliKisiTelefonu = Teklif.IlgiliKisiTelefonu,
                    IlgiliKisiEposta = Teklif.IlgiliKisiEposta
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
                        ToplamTutar = m.Toplam
                    });
                }

                var indTop = TeklifUrunler.Sum(u => u.Toplam);
                var kdv = indTop * (yeni.KdvOrani / 100m);
                var genTop = indTop + kdv;

                _context.TeklifToplamlari.Add(new TeklifToplam
                {
                    TeklifId = yeni.TeklifId,
                    IndirimliToplam = indTop,
                    KdvTutari = kdv,
                    GenelToplam = genTop
                });

                _context.SaveChanges();
                tr.Commit();

                // ✅ Burada yeni teklif tabloya düşsün diye Id ile çağır
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
        private void PdfIndir()
        {
            if (Teklif == null) return;

            try
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "PDF Dosyaları (*.pdf)|*.pdf",
                    FileName = $"Teklif_{Teklif?.Musteri?.FirmaAdi}_{DateTime.Now:yyyyMMdd}.pdf"
                };
                if (sfd.ShowDialog() != true) return;

                using var fs = new FileStream(sfd.FileName, FileMode.Create);
                var doc = new Document(PageSize.A4, 36, 36, 36, 36);
                PdfWriter.GetInstance(doc, fs);
                doc.Open();

                var titleFont = FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD);
                doc.Add(new Paragraph($"Teklif Detayları - {Teklif?.Musteri?.FirmaAdi}", titleFont));
                doc.Add(new Paragraph($"Tarih: {Teklif?.OlusturmaTarihi:dd.MM.yyyy}"));
                doc.Add(new Paragraph($"Durum: {Teklif?.Durum}"));
                doc.Add(new Paragraph($"Para Birimi: {Teklif?.ParaBirimi}"));
                doc.Add(new Paragraph($"Müşteri Notu: {Teklif?.MusteriNotu ?? "-"}"));
                doc.Add(new Paragraph("\n"));

                var table = new PdfPTable(6) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 2f, 5f, 1f, 2f, 2f, 2f });
                AddHeader(table, "Ürün Kodu");
                AddHeader(table, "Açıklama");
                AddHeader(table, "Adet");
                AddHeader(table, "Birim Fiyat");
                AddHeader(table, $"İndirimli Fiyat (%{Teklif.GenelIndirimOrani})");
                AddHeader(table, "Toplam");

                foreach (var u in TeklifUrunler)
                {
                    AddCell(table, u.UrunKodu);
                    AddCell(table, u.UrunAciklamasi);
                    AddCell(table, u.Adet.ToString());
                    AddCell(table, u.BirimFiyatText);
                    AddCell(table, u.IndirimliFiyatText);
                    AddCell(table, u.ToplamText);
                }

                doc.Add(table);
                doc.Add(new Paragraph("\n"));
                doc.Add(new Paragraph($"İndirimli Toplam: {IndirimliToplamText}"));
                doc.Add(new Paragraph($"KDV Tutarı: {KdvTutariText}"));
                doc.Add(new Paragraph($"Genel Toplam: {GenelToplamText}"));

                doc.Close();
                MessageBox.Show("PDF indirildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"PDF oluşturulurken hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void AddHeader(PdfPTable t, string text)
        {
            var c = new PdfPCell(new Phrase(text))
            {
                BackgroundColor = new BaseColor(240, 240, 240),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5
            };
            t.AddCell(c);
        }

        private static void AddCell(PdfPTable t, string text)
        {
            var c = new PdfPCell(new Phrase(text))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5
            };
            t.AddCell(c);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name ?? string.Empty));
    }
}

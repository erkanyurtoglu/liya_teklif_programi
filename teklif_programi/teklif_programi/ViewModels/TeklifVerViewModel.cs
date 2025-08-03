using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using teklif_programi.Data;
using teklif_programi.Models;

namespace teklif_programi.ViewModels
{
    public class TeklifVerViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context = new TeklifDbContext();

        public TeklifVerViewModel()
        {
            UrunleriYukle();
            SepeteEkleCommand = new RelayCommand(SepeteEkle);
            SepettenCikarCommand = new RelayCommand<TeklifUrunModel>(SepettenCikar);
            AdetArttirCommand = new RelayCommand<TeklifUrunModel>(urun => urun.adet++);
            AdetAzaltCommand = new RelayCommand<TeklifUrunModel>(urun =>
            {
                if (urun.adet > 1) urun.adet--;
            });
            KaydetVePdfIndirCommand = new RelayCommand(KaydetVePdfIndir);
        }

        #region Firma Arama
        private string _firmaArama;
        public string FirmaArama
        {
            get => _firmaArama;
            set
            {
                _firmaArama = value;
                OnPropertyChanged(nameof(FirmaArama));

                int.TryParse(_firmaArama, out int idArama);
                FirmaBilgisi = _context.Musteriler
                    .FirstOrDefault(f =>
                        f.firma_adi.Contains(_firmaArama) ||
                        f.musteri_id == idArama);
            }
        }

        private Musteri _firmaBilgisi;
        public Musteri FirmaBilgisi
        {
            get => _firmaBilgisi;
            set
            {
                _firmaBilgisi = value;
                OnPropertyChanged(nameof(FirmaBilgisi));
            }
        }
        #endregion

        #region Ürün Listesi
        private string _urunArama;
        public string UrunArama
        {
            get => _urunArama;
            set
            {
                _urunArama = value;
                OnPropertyChanged(nameof(UrunArama));
                UrunleriFiltrele();
            }
        }

        public ObservableCollection<Urun> TumUrunler { get; set; } = new();
        public ObservableCollection<Urun> FiltrelenmisUrunler { get; set; } = new();

        private void UrunleriYukle()
        {
            TumUrunler = new ObservableCollection<Urun>(_context.Urunler.ToList());
            FiltrelenmisUrunler = new ObservableCollection<Urun>(TumUrunler);
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
        #endregion

        #region Sepet
        public ObservableCollection<TeklifUrunModel> SecilenUrunler { get; set; } = new();

        public ICommand SepeteEkleCommand { get; }
        public ICommand SepettenCikarCommand { get; }
        public ICommand AdetArttirCommand { get; }
        public ICommand AdetAzaltCommand { get; }

        private void SepeteEkle()
        {
            if (FiltrelenmisUrunler.FirstOrDefault() is Urun secili)
            {
                var model = new TeklifUrunModel
                {
                    urun_id = secili.urun_id,
                    urun_kodu = secili.urun_kodu,
                    urun_aciklamasi = secili.urun_aciklamasi,
                    birim_fiyat = secili.birim_fiyat,
                    adet = 1
                };
                HesaplaIndirimliFiyat(model);
                SecilenUrunler.Add(model);
                OnPropertyChanged(nameof(SecilenUrunler));
            }
        }

        private void SepettenCikar(TeklifUrunModel urun)
        {
            SecilenUrunler.Remove(urun);
            OnPropertyChanged(nameof(SecilenUrunler));
            OnPropertyChanged(nameof(ToplamFiyat));
            OnPropertyChanged(nameof(KdvUcreti));
            OnPropertyChanged(nameof(GenelToplam));
        }

        private void HesaplaIndirimliFiyat(TeklifUrunModel model)
        {
            decimal indirim = GenelIndirimOrani / 100;
            model.indirimli_fiyat = model.birim_fiyat * (1 - indirim);
        }
        #endregion

        #region İndirim - KDV - Toplamlar

        private decimal _genelIndirimOrani = 0;
        public decimal GenelIndirimOrani
        {
            get => _genelIndirimOrani;
            set
            {
                _genelIndirimOrani = value < 0 ? 0 : value;  // eksiye karşı da önlem
                OnPropertyChanged(nameof(GenelIndirimOrani));
                foreach (var urun in SecilenUrunler)
                    HesaplaIndirimliFiyat(urun);
                OnPropertyChanged(nameof(ToplamFiyat));
                OnPropertyChanged(nameof(KdvUcreti));
                OnPropertyChanged(nameof(GenelToplam));
            }
        }

        private decimal _kdvOrani = 0;
        public decimal KdvOrani
        {
            get => _kdvOrani;
            set
            {
                _kdvOrani = value < 0 ? 0 : value;
                OnPropertyChanged(nameof(KdvOrani));
                OnPropertyChanged(nameof(KdvUcreti));
                OnPropertyChanged(nameof(GenelToplam));
            }
        }

        public decimal ToplamFiyat => SecilenUrunler.Sum(u => u.toplam);
        public decimal KdvUcreti => ToplamFiyat * (KdvOrani / 100);
        public decimal GenelToplam => ToplamFiyat + KdvUcreti;

        #endregion

        #region Kaydet ve PDF

        public ICommand KaydetVePdfIndirCommand { get; }

        private void KaydetVePdfIndir()
        {
            if (FirmaBilgisi == null || !SecilenUrunler.Any()) return;

            // 1. Veritabanına teklif kaydı ekle
            var teklif = new Teklif
            {
                musteri_id = FirmaBilgisi.musteri_id,
                olusturma_tarihi = DateTime.Now,
                genel_indirim_orani = GenelIndirimOrani,
                kdv_orani = KdvOrani,
                TeklifToplam = GenelToplam
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
                    indirimli_birim_fiyat = urun.indirimli_fiyat
                });
            }

            _context.SaveChanges();

            // 2. PDF oluştur (iTextSharp kullanıyordun, o kodu ayrıca yazabiliriz.)
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }


}

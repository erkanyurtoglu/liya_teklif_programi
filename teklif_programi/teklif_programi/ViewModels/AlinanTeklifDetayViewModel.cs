using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using teklif_programi.Data;
using teklif_programi.Helpers;
using teklif_programi.Models;
using teklif_programi.Services;

namespace teklif_programi.ViewModels
{
    public class AlinanTeklifDetayViewModel : INotifyPropertyChanged
    {
        private readonly TeklifDbContext _context;
        public Teklif Teklif { get; }
        public ObservableCollection<TeklifUrun> Urunler { get; } = new();
        public ObservableCollection<DovizKuru> DovizKurlari { get; } = new();

        public RelayCommand KaydetCommand { get; }

        public AlinanTeklifDetayViewModel(Teklif teklif)
        {
            _context = new TeklifDbContext();
            Teklif = _context.Teklifler
                .Include(t => t.TeklifUrunleri)
                    .ThenInclude(tu => tu.Urun)
                .First(t => t.TeklifId == teklif.TeklifId);

            DovizKurlariGuncelle();

            foreach (var u in Teklif.TeklifUrunleri)
            {
                u.BirimFiyatText = FormatPrice(u.BirimFiyat);
                u.IndirimliBirimFiyatText = FormatPrice(u.IndirimliBirimFiyat);
                var maliyet = ConvertTlToTeklifCurrency(u.Urun.MaliyetFiyati);
                u.MaliyetFiyatText = FormatPrice(maliyet);
                Urunler.Add(u);
            }

            KaydetCommand = new RelayCommand(Kaydet);

            UpdateTotalsText();
        }

        private void Kaydet()
        {
            _context.SaveChanges();
        }

        // Hesaplanan değerler
        public decimal ToplamFiyat => Urunler.Sum(u => u.ToplamTutar);
        public decimal KdvTutari => TeklifHesaplayici.HesaplaKdv(ToplamFiyat, Teklif.KdvOrani);
        public decimal GenelToplam => TeklifHesaplayici.HesaplaGenelToplam(ToplamFiyat, Teklif.KdvOrani, Teklif.TeklifToplam?.PaketlemeUcreti ?? 0);
        public decimal ToplamMaliyet => Urunler.Sum(u => u.Adet * ConvertTlToTeklifCurrency(u.Urun.MaliyetFiyati));
        public decimal KarTutari => TeklifHesaplayici.HesaplaKarTutari(ToplamFiyat, ToplamMaliyet);
        public decimal KarOrani => TeklifHesaplayici.HesaplaKarOrani(KarTutari, ToplamMaliyet);

        private string _toplamMaliyetText = string.Empty;
        public string ToplamMaliyetText { get => _toplamMaliyetText; set { _toplamMaliyetText = value; OnPropertyChanged(); } }

        private string _karTutariText = string.Empty;
        public string KarTutariText { get => _karTutariText; set { _karTutariText = value; OnPropertyChanged(); } }

        private string _karOraniText = string.Empty;
        public string KarOraniText { get => _karOraniText; set { _karOraniText = value; OnPropertyChanged(); } }

        private string _toplamFiyatText = string.Empty;
        public string ToplamFiyatText { get => _toplamFiyatText; set { _toplamFiyatText = value; OnPropertyChanged(); } }

        private string _kdvTutariText = string.Empty;
        public string KdvTutariText { get => _kdvTutariText; set { _kdvTutariText = value; OnPropertyChanged(); } }

        private string _genelToplamText = string.Empty;
        public string GenelToplamText { get => _genelToplamText; set { _genelToplamText = value; OnPropertyChanged(); } }

        private void UpdateTotalsText()
        {
            ToplamMaliyetText = FormatPrice(ToplamMaliyet);
            KarTutariText = FormatPrice(KarTutari);
            KarOraniText = KarOrani.ToString("F2") + "%";
            ToplamFiyatText = FormatPrice(ToplamFiyat);
            KdvTutariText = FormatPrice(KdvTutari);
            GenelToplamText = FormatPrice(GenelToplam);
        }

        private string FormatPrice(decimal price)
        {
            return price.ToString("C2", GetCultureByCurrency(Teklif.ParaBirimi));
        }

        private decimal ConvertTlToTeklifCurrency(decimal tlValue)
        {
            return Teklif.ParaBirimi switch
            {
                "USD" => tlValue / (DovizKurlari.FirstOrDefault(k => k.DovizCinsi == "USD")?.Satis ?? 1),
                "EUR" => tlValue / (DovizKurlari.FirstOrDefault(k => k.DovizCinsi == "EUR")?.Satis ?? 1),
                _ => tlValue
            };
        }

        private void DovizKurlariGuncelle()
        {
            var kurListesi = DovizServisi.KurListesiniGetir();
            DovizKurlari.Clear();
            foreach (var kur in kurListesi) DovizKurlari.Add(kur);
        }

        private static CultureInfo GetCultureByCurrency(string currency)
        {
            return currency switch
            {
                "USD" => new CultureInfo("en-US"),
                "EUR" => new CultureInfo("en-IE"),
                _ => new CultureInfo("tr-TR"),
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
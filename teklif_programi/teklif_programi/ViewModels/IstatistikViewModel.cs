using System;
using System.Collections.ObjectModel;
using System.Linq;
using teklif_programi.Services;
using teklif_programi.Data;

namespace teklif_programi.ViewModels
{
    public class IstatistikViewModel
    {
        public int GunlukTeklif { get; }
        public int HaftalikTeklif { get; }
        public int AylikTeklif { get; }
        public int YillikTeklif { get; }
        public int ToplamGonderilen { get; }
        public int ToplamKabulEdilen { get; }
        public int ToplamReddedilen { get; }
        public int ToplamBeklemede { get; }
        public ObservableCollection<PersonelPerformans> PersonelPerformanslari { get; }

        public IstatistikViewModel()
        {
            var service = new IstatistikService();
            (GunlukTeklif, HaftalikTeklif, AylikTeklif, YillikTeklif) = service.GetTeklifSayilari();
            var list = service.GetPersonelPerformanslari();
            ToplamGonderilen = list.Sum(p => p.Gonderilen);
            ToplamKabulEdilen = list.Sum(p => p.KabulEdilen);
            ToplamReddedilen = list.Sum(p => p.Reddedilen);
            ToplamBeklemede = list.Sum(p => p.Beklemede);
            PersonelPerformanslari = new ObservableCollection<PersonelPerformans>(list);
        }
    }
}
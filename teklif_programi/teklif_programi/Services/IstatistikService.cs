using System;
using System.Collections.Generic;
using System.Linq;
using teklif_programi.Data;

namespace teklif_programi.Services
{
    public class PersonelPerformans
    {
        public string Personel { get; set; } = string.Empty;
        public int Gonderilen { get; set; }
        public int KabulEdilen { get; set; }
        public int Reddedilen { get; set; }
        public int Beklemede { get; set; }
    }

    public class IstatistikService
    {
        private readonly TeklifDbContext _context = new();

        public (int gun, int hafta, int ay, int yil) GetTeklifSayilari()
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);

            int gunluk = _context.Teklifler.Count(t => t.OlusturmaTarihi >= today);
            int haftalik = _context.Teklifler.Count(t => t.OlusturmaTarihi >= startOfWeek);
            int aylik = _context.Teklifler.Count(t => t.OlusturmaTarihi >= startOfMonth);
            int yillik = _context.Teklifler.Count(t => t.OlusturmaTarihi >= startOfYear);

            return (gunluk, haftalik, aylik, yillik);
        }

        public List<PersonelPerformans> GetPersonelPerformanslari()
        {
            return _context.Personeller
                .Select(p => new PersonelPerformans
                {
                    Personel = p.AdSoyad,
                    Gonderilen = p.Teklifler.Count(),
                    KabulEdilen = p.Teklifler.Count(t => t.Durum == "Kabul Edildi"),
                    Reddedilen = p.Teklifler.Count(t => t.Durum == "Reddedildi"),
                    Beklemede = p.Teklifler.Count(t => t.Durum == "Beklemede")
                })
                .ToList();
        }
    }
}
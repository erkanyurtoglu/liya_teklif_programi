// Gerekli isim alanları: Temel sistem, koleksiyonlar, kültür ayarları ve XML işleme için
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using teklif_programi.Models;

// Teklif programı servis sınıflarının isim alanı
namespace teklif_programi.Services
{
    // DovizServisi: TCMB'den döviz kurlarını çeken statik servis sınıfı
    public static class DovizServisi
    {
        // KurListesiniGetir: TCMB'den USD ve EUR kurlarını alır ve DovizKuru listesi döndürür
        public static List<DovizKuru> KurListesiniGetir()
        {
            try
            {
                // TCMB'nin güncel kurları içeren XML dosyasının URL'si
                var url = "https://www.tcmb.gov.tr/kurlar/today.xml";
                // XML dosyasını yükler
                XDocument doc = XDocument.Load(url);

                // XML'den sadece USD ve EUR kurlarını seçer, DovizKuru nesnelerine dönüştürür
                return doc
                    .Descendants("Currency") // XML'deki Currency etiketlerini alır
                    .Where(d => d.Attribute("CurrencyCode")?.Value is "USD" or "EUR") // Sadece USD ve EUR filtreler
                    .Select(d => new DovizKuru( // Her bir Currency için DovizKuru nesnesi oluşturur
                        d.Attribute("CurrencyCode")!.Value, // Para birimi kodu (USD veya EUR)
                        decimal.Parse(d.Element("ForexBuying")?.Value ?? "0", CultureInfo.InvariantCulture), // Alış kuru
                        decimal.Parse(d.Element("ForexSelling")?.Value ?? "0", CultureInfo.InvariantCulture) // Satış kuru
                    ))
                    .ToList(); // Sonucu liste olarak döndürür
            }
            catch
            {
                // Hata durumunda boş liste döner (loglama yapılabilir)
                return new List<DovizKuru>();
            }
        }
    }
}
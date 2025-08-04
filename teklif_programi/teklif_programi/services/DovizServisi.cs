using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using teklif_programi.Models;

namespace teklif_programi.Services
{
    public static class DovizServisi
    {
        public static List<DovizKuru> KurListesiniGetir()
        {
            try
            {
                var url = "https://www.tcmb.gov.tr/kurlar/today.xml";
                XDocument doc = XDocument.Load(url);

                return doc
                    .Descendants("Currency")
                    .Where(d => d.Attribute("CurrencyCode")?.Value is "USD" or "EUR")
                    .Select(d => new DovizKuru(
                        d.Attribute("CurrencyCode")!.Value,
                        decimal.Parse(d.Element("ForexBuying")?.Value ?? "0", CultureInfo.InvariantCulture),
                        decimal.Parse(d.Element("ForexSelling")?.Value ?? "0", CultureInfo.InvariantCulture)
                    ))
                    .ToList();
            }
            catch
            {
                // Loglama yapılabilir
                return new List<DovizKuru>();
            }
        }
    }
}

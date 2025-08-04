using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using teklif_programi.Models;


public static class DovizServisi
{
    public static List<DovizKuru> KurListesiniGetir()
    {
        try
        {
            var url = "https://www.tcmb.gov.tr/kurlar/today.xml";
            XDocument doc = XDocument.Load(url);

            var kurlar = new List<DovizKuru>();

            foreach (var doviz in doc.Descendants("Currency"))
            {
                var code = doviz.Attribute("CurrencyCode")?.Value;

                if (code == "USD" || code == "EUR")
                {
                    kurlar.Add(new DovizKuru
                    {
                        DovizCinsi = code,
                        Alis = decimal.Parse(doviz.Element("ForexBuying")?.Value ?? "0", CultureInfo.InvariantCulture),
                        Satis = decimal.Parse(doviz.Element("ForexSelling")?.Value ?? "0", CultureInfo.InvariantCulture)
                    });
                }
            }

            return kurlar;
        }
        catch (Exception ex)
        {
            // Hata loglanabilir
            return new List<DovizKuru>();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using teklif_programi.Models;

namespace teklif_programi.Services
{
    /// <summary>
    /// Teklif hesaplamalarında kullanılan tüm matematiksel işlemleri merkezi olarak sağlar.
    /// Bu sınıf sayesinde formüller tek bir noktada toplanır ve bakımı kolaylaşır.
    /// </summary>
    public static class TeklifHesaplayici
    {
        /// <summary>
        /// Birim fiyat ve genel indirim oranından indirimli birim fiyatı hesaplar.
        /// </summary>
        /// <param name="birimFiyat">Ürünün indirimsiz birim fiyatı.</param>
        /// <param name="genelIndirimOrani">Genel indirim yüzdesi.</param>
        /// <returns>Indirim uygulanmış birim fiyat.</returns>
        public static decimal HesaplaIndirimliFiyat(decimal birimFiyat, decimal genelIndirimOrani)
            => birimFiyat * (1 - genelIndirimOrani / 100);

        /// <summary>
        /// Ürün adedi ve indirimli fiyatı üzerinden satır toplamını hesaplar.
        /// </summary>
        public static decimal HesaplaToplam(int adet, decimal indirimliFiyat)
            => adet * indirimliFiyat;

        /// <summary>
        /// Seçilen ürünlerin indirimli tutarlarının toplamını verir.
        /// </summary>
        public static decimal HesaplaToplamFiyat(IEnumerable<TeklifUrunModel> urunler)
            => urunler.Sum(u => u.Toplam);

        /// <summary>
        /// Toplam fiyat ve KDV oranına göre KDV tutarını hesaplar.
        /// </summary>
        public static decimal HesaplaKdv(decimal toplamFiyat, decimal kdvOrani)
            => toplamFiyat * (kdvOrani / 100);

        /// <summary>
        /// Toplam fiyat ve KDV oranına göre genel toplamı hesaplar.
        /// </summary>
        public static decimal HesaplaGenelToplam(decimal toplamFiyat, decimal kdvOrani)
        {
            var kdv = HesaplaKdv(toplamFiyat, kdvOrani);
            return toplamFiyat + kdv;
        }

        /// <summary>
        /// Seçilen ürünlerin maliyetlerinin toplamını hesaplar.
        /// </summary>
        public static decimal HesaplaToplamMaliyet(IEnumerable<TeklifUrunModel> urunler)
            => urunler.Sum(u => u.Adet * u.MaliyetFiyati);

        /// <summary>
        /// Toplam satış fiyatı ve toplam maliyet üzerinden kâr tutarını hesaplar.
        /// </summary>
        public static decimal HesaplaKarTutari(decimal toplamFiyat, decimal toplamMaliyet)
            => toplamFiyat - toplamMaliyet;

        /// <summary>
        /// Kâr tutarı ve toplam maliyet üzerinden kâr oranını hesaplar.
        /// </summary>
        public static decimal HesaplaKarOrani(decimal karTutari, decimal toplamMaliyet)
            => toplamMaliyet == 0 ? 0 : (karTutari / toplamMaliyet) * 100;
    }
}
using System;
using System.Globalization;
using System.Windows.Data;

namespace teklif_programi.Converters
{
    /// <summary>
    /// Personelin aktiflik durumunu kullanıcı arayüzünde anlamlı metinlere dönüştürür.
    /// </summary>
    public class PersonelDurumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool aktifMi)
            {
                return Binding.DoNothing;
            }

            var mode = parameter?.ToString();

            return mode switch
            {
                "Button" => aktifMi ? "Pasif Yap" : "Aktif Yap",
                "Tooltip" => aktifMi
                    ? "Personeli pasif duruma al"
                    : "Personeli yeniden aktifleştir",
                _ => aktifMi ? "Aktif" : "Pasif"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
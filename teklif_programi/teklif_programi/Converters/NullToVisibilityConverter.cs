using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace teklif_programi
{
    /// <summary>
    /// NullToVisibilityConverter:
    /// WPF veri bağlama (DataBinding) içinde kullanılır.
    /// Bir değerin null olup olmadığına göre Visibility döndürür.
    /// XAML tarafında görünürlük kontrolü için kullanılır.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// value: Bağlanan veri
        /// targetType: Hedef veri tipi (Visibility)
        /// parameter: "Inverse" verilirse mantık tersine çevrilir
        /// culture: Kültürel format bilgisi
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Parametre "Inverse" ise görünürlük mantığı ters çevrilir
            bool inverse = parameter?.ToString() == "Inverse";

            // Değer null mu kontrol edilir
            bool isNull = value == null;

            // Normal kullanım:
            // null → Collapsed
            // dolu → Visible
            // "Inverse" kullanımında tam tersi olur.
            return inverse
                ? (isNull ? Visibility.Visible : Visibility.Collapsed)
                : (isNull ? Visibility.Collapsed : Visibility.Visible);
        }

        /// <summary>
        /// Geri dönüş dönüşümü bu converter'da desteklenmez.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

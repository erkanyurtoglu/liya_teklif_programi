using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace teklif_programi
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = parameter?.ToString() == "Inverse";
            bool isNull = value == null;
            return inverse ? (isNull ? Visibility.Visible : Visibility.Collapsed) : (isNull ? Visibility.Collapsed : Visibility.Visible);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
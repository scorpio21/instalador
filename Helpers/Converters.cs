using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;

namespace Instalador.Helpers
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverse = parameter?.ToString() == "Inverse";
            bool isNull = value == null;
            
            if (isInverse) return isNull ? Visibility.Visible : Visibility.Collapsed;
            return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EstadoToBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string estado = value?.ToString() ?? "";

            if (estado == "OK") return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x3C, 0xD6, 0x5B));
            if (estado == "WARN") return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0xB9, 0x00));
            if (estado == "ERROR") return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xFF, 0x4D, 0x4D));

            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringNotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool inverse = parameter?.ToString() == "Inverse";
            bool visible = !string.IsNullOrWhiteSpace(value?.ToString());
            if (inverse) visible = !visible;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Lexplosion.Common.Converters
{
    public sealed class SolidColorBrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var solidColorBrush = (SolidColorBrush)value;
            return solidColorBrush.Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

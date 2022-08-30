using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Lexplosion.Gui.Converters
{
    internal sealed class StringToGeometryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
                return Geometry.Parse(value as string);
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

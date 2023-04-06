using Lexplosion.Tools;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Lexplosion.Common.Converters
{
    public sealed class BytesToBitmapImageConventer : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = (byte[])value;
            return ImageTools.ToImage(bytes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

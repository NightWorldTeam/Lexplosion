using System;
using System.Globalization;
using System.Windows;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    public sealed class NullableToVisibilityConverter : ConverterBase<NullableToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}

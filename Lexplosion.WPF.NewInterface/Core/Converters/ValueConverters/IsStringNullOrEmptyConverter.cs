using System;
using System.Globalization;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    public class IsStringNullOrEmptyConverter : ConverterBase<IsStringNullOrEmptyConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string);
        }
    }
}

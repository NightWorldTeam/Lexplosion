using System;
using System.Globalization;

namespace Lexplosion.UI.WPF.Core.Converters
{
    public sealed class ToStringConverter : ConverterBase<ToStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
}

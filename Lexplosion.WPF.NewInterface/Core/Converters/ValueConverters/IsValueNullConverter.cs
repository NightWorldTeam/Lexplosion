using System;
using System.Globalization;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    public sealed class IsValueNullConverter : ConverterBase<IsValueNullConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null;
        }
    }
}

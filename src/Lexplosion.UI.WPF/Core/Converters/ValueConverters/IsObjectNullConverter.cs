using System;
using System.Globalization;

namespace Lexplosion.UI.WPF.Core.Converters.ValueConverters
{
    public sealed class IsObjectNullConverter : ConverterBase<IsObjectNullConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return true;
            return false;
        }
    }
}

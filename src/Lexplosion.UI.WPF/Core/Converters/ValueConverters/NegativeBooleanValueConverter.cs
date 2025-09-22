using System;
using System.Globalization;

namespace Lexplosion.UI.WPF.Core.Converters
{
    internal class NegativeBooleanValueConverter : ConverterBase<NegativeBooleanValueConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !((bool)value); 
        }
    }
}

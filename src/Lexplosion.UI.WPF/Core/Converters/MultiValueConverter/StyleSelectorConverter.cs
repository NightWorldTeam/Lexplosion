using System;
using System.Globalization;

namespace Lexplosion.UI.WPF.Core.Converters.MultiValueConverter
{
    public class StyleSelectorConverter : ConverterBase<StyleSelectorConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double test)
            {
                var s = test / (double)parameter;

                return 0;
            }
            return 0;
        }
    }
}

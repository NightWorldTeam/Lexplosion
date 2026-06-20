using System;
using System.Globalization;

namespace Lexplosion.UI.WPF.Core.Converters
{
    public class EmptyValueToDashConverter : ConverterBase<EmptyValueToDashConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            return string.IsNullOrEmpty(str) ? "\u2014" : str;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            return str == "\u2014" ? "" : (str ?? "");
        }
    }
}

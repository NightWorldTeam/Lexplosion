using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Lexplosion.WPF.NewInterface.Core.Converters.MultiValueConverter
{
    public sealed class StringFormatConverter : MultiValueConverterBase<StringFormatConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == DependencyProperty.UnsetValue || values[0] == null)
                return String.Empty;

            var format = (string)values[0];

            return String.Format(format, values.Skip(0).ToArray<object>());
        }
    }
}

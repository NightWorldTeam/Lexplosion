using System;
using System.Globalization;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    public sealed class RowElementCountConverter : ConverterBase<RowElementCountConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                var width = (double)value;
                return (int)(width / double.Parse(parameter.ToString(), CultureInfo.InvariantCulture));
            }
            return 0;
        }
    }
}

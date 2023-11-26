using System;
using System.Globalization;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    public class WindowHeightToXProcentHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double) 
            {
                var height = (double)value;
                return (height / 100) * int.Parse(parameter.ToString());
            }
            return 620;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

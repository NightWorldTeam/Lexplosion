using System;
using System.Globalization;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    public sealed class WindowWidthToXProcentWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double) 
            {
                return ((double)value / 100) * int.Parse(parameter.ToString());
            }
            return 620;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

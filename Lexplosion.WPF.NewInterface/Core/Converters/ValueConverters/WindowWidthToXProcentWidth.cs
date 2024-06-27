using System;
using System.Globalization;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    public sealed class WindowWidthToXProcentWidth : ConverterBase<WindowWidthToXProcentWidth>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double) 
            {
                //Runtime.DebugWrite(((double)value / 100) * double.Parse(parameter.ToString(), CultureInfo.InvariantCulture));
                return ((double)value / 100) * double.Parse(parameter.ToString(), CultureInfo.InvariantCulture);
            }
            return 620;
        }
    }
}

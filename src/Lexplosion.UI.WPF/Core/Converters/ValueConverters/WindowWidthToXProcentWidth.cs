using System;
using System.Globalization;

namespace Lexplosion.UI.WPF.Core.Converters
{
    public sealed class WindowWidthToXProcentWidth : ConverterBase<WindowWidthToXProcentWidth>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                return ((double)value / 100) * double.Parse(parameter.ToString(), CultureInfo.InvariantCulture) / GetScalingFactor();
            }
            return 620;
        }

        private double GetScalingFactor()
        {
            var value = App.Current.Resources["ScalingFactorValue"];
            value ??= 1.0d;
            return (double)value;
        }
    }
}

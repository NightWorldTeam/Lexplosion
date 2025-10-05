using Lexplosion.UI.WPF.Mvvm.Views.Windows;
using System;
using System.Globalization;

namespace Lexplosion.UI.WPF.Core.Converters
{
    // Height="{
    //  Binding ActualHeight,
    //  RelativeSource={RelativeSource AncestorType={x:Type Window}},
    //  Converter={converters:WindowHeightToXProcentHeight}, ConverterParameter=80
    // }"

    public sealed class WindowHeightToXProcentHeight : ConverterBase<WindowHeightToXProcentHeight>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double) 
            {
                var height = (double)value;
                return (height / 100) * double.Parse(parameter.ToString(), CultureInfo.InvariantCulture) / GetScalingFactor();
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
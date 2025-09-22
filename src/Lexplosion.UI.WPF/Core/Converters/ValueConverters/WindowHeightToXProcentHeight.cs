using Lexplosion.UI.WPF.Core.Converters.MultiValueConverter;
using Lexplosion.UI.WPF.Mvvm.Views.Windows;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace Lexplosion.UI.WPF.Core.Converters
{
    // Height="{
    //  Binding ActualHeight,
    //  RelativeSource={RelativeSource AncestorType={x:Type Window}},
    //  Converter={converters:WindowHeightToXProcentHeight}, ConverterParameter=80
    // }"

    public sealed class WindowHeightToXProcentHeight : ConverterBase<WindowHeightToXProcentHeight>
    {
        // TODO: Need to know scalling coef;
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double) 
            {
                var height = (double)value;
                return (height / 100) * double.Parse(parameter.ToString(), CultureInfo.InvariantCulture);
            }

            if (value is IScalable scalable)
            {
                var height = scalable.ActualHeight;
                return ((height / 100) * double.Parse(parameter.ToString(), CultureInfo.InvariantCulture)) / scalable.ScalingFactor;
            }

            return 620;
        }
    }
}

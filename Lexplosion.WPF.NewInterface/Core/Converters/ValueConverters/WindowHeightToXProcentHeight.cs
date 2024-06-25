using System;
using System.Globalization;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Core.Converters
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
                return (height / 100) * double.Parse(parameter.ToString(), CultureInfo.InvariantCulture);
            }
            return 620;
        }
    }
}

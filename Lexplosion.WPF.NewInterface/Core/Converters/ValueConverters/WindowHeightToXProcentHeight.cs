using System;
using System.Globalization;
using System.Windows.Data;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    // Height="{
    //  Binding ActualHeight,
    //  RelativeSource={RelativeSource AncestorType={x:Type Window}},
    //  Converter={StaticResource WindowHeightToXProcentHeight}, ConverterParameter=80
    // }"
            
    public sealed class WindowHeightToXProcentHeight : ConverterBase<WindowHeightToXProcentHeight>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Runtime.DebugWrite(targetType);
            if (value is double) 
            {
                var height = (double)value;
                return (height / 100) * int.Parse(parameter.ToString());
            }
            return 620;
        }
    }
}

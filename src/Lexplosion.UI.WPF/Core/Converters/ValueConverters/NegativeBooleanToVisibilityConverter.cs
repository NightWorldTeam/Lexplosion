using System;
using System.Globalization;
using System.Windows;

namespace Lexplosion.UI.WPF.Core.Converters
{
    public class NegativeBooleanToVisibilityConverter : ConverterBase<NegativeBooleanToVisibilityConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bValue = false;
            if (value is bool)
            {
                bValue = (bool)value;
            }
            else if (value is Nullable<bool>)
            {
                Nullable<bool> tmp = (Nullable<bool>)value;
                bValue = tmp.HasValue ? tmp.Value : false;
            }
            return (bValue) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}

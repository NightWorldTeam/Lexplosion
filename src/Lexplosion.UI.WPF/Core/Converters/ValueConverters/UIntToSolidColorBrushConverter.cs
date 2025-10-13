using System;
using System.Globalization;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.Core.Converters
{
    public class UIntToSolidColorBrushConverter : ConverterBase<UIntToSolidColorBrushConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Must be uint not null");
            }

            if (value is uint uintValue)
            {
                return ConvertNumericToColor(uintValue);
            }

            throw new ArgumentException($"Must be uint not {value.GetType()}");
        }

        private SolidColorBrush ConvertNumericToColor(uint value)
        {
            return new SolidColorBrush(GetColor(value));
        }

        public static Color GetColor(uint hex)
        {
            return Color.FromArgb(
                (byte)((hex >> 24) & 0xFF), // Alpha
                (byte)((hex >> 16) & 0xFF), // Red
                (byte)((hex >> 8) & 0xFF),  // Green
                (byte)(hex & 0xFF)          // Blue
            );
        }
    }
}

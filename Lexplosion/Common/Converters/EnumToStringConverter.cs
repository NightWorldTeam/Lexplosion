using Lexplosion.Properties;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Lexplosion.Common.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            return Resources.ResourceManager.GetString(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = (string)value;

            foreach (object enumValue in Enum.GetValues(targetType))
            {
                if (str == Resources.ResourceManager.GetString(enumValue.ToString()))
                {
                    return enumValue;
                }
            }

            throw new ArgumentException(null, "value");
        }
    }
}

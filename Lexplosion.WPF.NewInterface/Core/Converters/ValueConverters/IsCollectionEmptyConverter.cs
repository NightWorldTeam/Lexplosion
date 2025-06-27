using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    public class IsCollectionEmptyConverter : ConverterBase<IsCollectionEmptyConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) 
            {
                return true;
            }

            if (value is ICollection collection) 
            {
                return collection.Count == 0;
            }

            if (value is IEnumerable enumerable)
            {
                return enumerable.Cast<object>().Count() == 0;
            }

            throw new ArgumentException($"Неверный формат данных ожидалась колекция, но значение {value}");
        }
    }
}

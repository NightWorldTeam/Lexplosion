using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    internal class CollectionToStringWithBreaklineConverter : ConverterBase<CollectionToStringWithBreaklineConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            if (value is IEnumerable collection) 
            {
                if (int.TryParse(parameter as string, out int skipCount))
                {
                    return string.Join("\n", collection.Cast<object>().Skip(skipCount).Select(i => i.ToString()));
                }

                Runtime.DebugWrite("[Error] Parameter is not int. Parameter is count of items for skip.", color: ConsoleColor.Red);
                return value;
            }

            Runtime.DebugWrite("[Error] Value is not a collection.", color: ConsoleColor.Red);
            return null;
        }
    }
}

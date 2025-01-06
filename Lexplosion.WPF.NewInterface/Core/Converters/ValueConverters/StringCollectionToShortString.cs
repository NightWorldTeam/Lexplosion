using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    internal class StringCollectionToShortString : ConverterBase<StringCollectionToShortString>
    {
        // TODO: Need to know scalling coef;
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<string> items) 
            {
                return $"{items.FirstOrDefault()} + {items.Count() - 1}";
            }

            Runtime.DebugWrite("[Error] Wrong string collection format.", color: ConsoleColor.Red);

            return "wrong string collection format";
        }
    }
}

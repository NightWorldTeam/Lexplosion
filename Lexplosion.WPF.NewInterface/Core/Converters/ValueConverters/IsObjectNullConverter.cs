using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;

namespace Lexplosion.WPF.NewInterface.Core.Converters.ValueConverters
{
    public sealed class IsObjectNullConverter : ConverterBase<IsObjectNullConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return true;
            return false;
        }
    }
}

using System;
using System.Globalization;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    public sealed class PositiveIntToShortStringConverter : ConverterBase<PositiveIntToShortStringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number)
            {
                if (number == 0 || number < 0)
                {
                    return 0;
                }

                if (number < 10000)
                    return number;

                var size = (int)Math.Log10(number);

                switch (size) 
                {
                    //k
                    case 4:
                        {
                            return (number / 1000).ToString("##.###k");
                        }
                    case 5:
                        {
                            return (number / 100).ToString("###.###k");
                        }
                    // M
                    case 7:
                        {
                            return (number / 1000000).ToString("##.##M");
                        }
                    case 8:
                        {
                            return (number / 100000).ToString("###.##M");
                        }
                    default:
                        return (number / Math.Pow(10, size)).ToString("#.##M");
                }


            }

            return Int32.MinValue;
        }
    }
}

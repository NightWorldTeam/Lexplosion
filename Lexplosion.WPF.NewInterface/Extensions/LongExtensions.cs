using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.Extensions
{
    public static class LongExtensions
    {
        /// <summary>
        /// Принимает long и возвращает строку формата 7.77M, где M - миллион
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string LongToString(this long number)
        {
            if (number == 0)
                return "0";

            if (number < 10000)
                return number.ToString();

            var size = (long)Math.Log10(number);

            switch (size)
            {
                //k
                case 4:
                    {
                        return (number / 1000).ToString("##.###k");
                    }
                case 5:
                    {
                        return (number / 1000).ToString("###.###k");
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
    }
}

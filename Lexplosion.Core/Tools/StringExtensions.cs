using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.Tools
{
    static class StringExtensions
    {
        public static int ToInt32(this string str)
        {
            Int32.TryParse(str, out int result);
            return result;
        }
    }
}

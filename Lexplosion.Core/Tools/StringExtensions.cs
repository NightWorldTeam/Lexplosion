using System;

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

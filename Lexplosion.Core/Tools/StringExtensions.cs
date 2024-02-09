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

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}

using System;

namespace Lexplosion.Tools
{
    public static class ColorTools
    {
        public static string FromRgbToHex(byte r, byte g, byte b)
        {
            return "#" + BitConverter.ToString(new byte[] { r, g, b }).Replace("-", "");
        }
    }
}

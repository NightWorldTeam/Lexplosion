using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

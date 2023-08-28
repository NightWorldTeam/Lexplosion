using System;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Tools
{
    public static class ColorTools
    {
        public static Color GetColorByHex(string hexValue)
        {
            return (Color)ColorConverter.ConvertFromString(hexValue);
        }

        public static string GetHexByColor(Color color)
        {
            var stringHexR = color.R.ToString("X");
            var stringHexG = color.G.ToString("X");
            var stringHexB = color.B.ToString("X");

            stringHexR = stringHexR == "0" ? "00" : stringHexR;
            stringHexG = stringHexG == "0" ? "00" : stringHexG;
            stringHexB = stringHexB == "0" ? "00" : stringHexB;

            return "#" + stringHexR + stringHexG + stringHexB;
        }

        public static Color GetDarkerColor(Color color, float persentages)
        {
            Runtime.DebugWrite(color.R + " " + color.G + " " + color.B);

            var newR = color.R - Math.Round(color.R * persentages / 100);
            var newG = color.G - Math.Round(color.G * persentages / 100);
            var newB = color.B - Math.Round(color.B * persentages / 100);

            color.R = (byte)newR;
            color.G = (byte)newG;
            color.B = (byte)newB;

            return color;

            //var newR = color.R - Math.Round(color.R * persentages / 100);
            //var newG = color.G - Math.Round(color.G * persentages / 100);
            //var newB = color.B - Math.Round(color.B * persentages / 100);

            //Runtime.DebugWrite(newR + " " + newG + " " + newB);

            //return new Color()
            //{
            //    R = (byte)newR,
            //    G = (byte)newG,
            //    B = (byte)newB,
            //};
        }

        public static Color GetLighterColor(Color color, float persentages)
        {
            var newR = color.R + (color.R * Math.Round(persentages / 100));
            var newG = color.G + (color.G * Math.Round(persentages / 100));
            var newB = color.B + (color.B * Math.Round(persentages / 100));

            newR = newR > 255 ? 255 : newR;
            newR = newR < 0 ? 0 : newR;

            newG = newG > 255 ? 255 : newR;
            newG = newG < 0 ? 0 : newR;

            newB = newB > 255 ? 255 : newR;
            newB = newB < 0 ? 0 : newR;

            return new Color()
            {
                R = (byte)newR,
                G = (byte)newG,
                B = (byte)newB,
            };
        }
    }
}

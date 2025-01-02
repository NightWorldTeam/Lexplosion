using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.Tools
{
    public static class ColorTools
    {
        public static IEnumerable<string> _colors;

        static ColorTools() 
        {
            Type colorType = typeof(Colors);
            PropertyInfo[] propInfos = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
            var colors = new string[propInfos.Length];
            for (var i = 0; i < propInfos.Length; i++)
            {
                colors[i] = propInfos[i].Name;
            }
            _colors = colors;
        }



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

        public static Color GetDarkerColor(Color color, float percentages)
        {
            return GetLighterColor(color, percentages * -1);
        }

        public static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            Runtime.DebugWrite(red + " " + green + " " + blue);
            return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }

        public static Color GetLighterColor(Color color, float percentages)
        {
            var newR = Math.Min(255, Math.Floor(color.R + (color.R * percentages / 100)));
            var newG = Math.Min(255, Math.Floor(color.G + (color.G * percentages / 100)));
            var newB = Math.Min(255, Math.Floor(color.B + (color.B * percentages / 100)));

            color.R = (byte)newR;
            color.G = (byte)newG;
            color.B = (byte)newB;

            return color;
        }

        public static float CalculateLuminance(Color color) 
        {
            return (float)(0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B);
        }

        public static Color ForegroundByColor(Color color) 
        {
            var luminance = CalculateLuminance(color);
            return luminance < 140 ? Colors.White : Colors.Black;
        }

        public static Color? ConvertColor(string strColor)
        {
            try
            {
                if (IsHex(strColor))
                {
                    return (Color)ColorConverter.ConvertFromString(strColor);
                }

                foreach (var i in _colors)
                {
                    if (i == strColor)
                        return (Color)ColorConverter.ConvertFromString(strColor);
                }
            }
            catch { }

            return null;
        }

        public static bool IsHex(string strHex)
        {
            bool isHex;
            if (strHex.Length < 3)
                return false;

            foreach (var c in strHex)
            {
                isHex = ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'));

                if (!isHex) return false;
            }

            return true;
        }

        public static IEnumerable<Color> GetIntervalColor(Color color1, Color color2, int intervalCount = 50) 
        {
            var rMin = color1.R;
            var rMax = color2.R;

            var gMin = color1.G;
            var gMax = color2.G;

            var bMin = color1.B;
            var bMax = color2.B;

            var colorList = new List<Color>();
            for (int i = 0; i < intervalCount; i++)
            {
                var rAverage = rMin + (int)((rMax - rMin) * i / intervalCount);
                var gAverage = gMin + (int)((gMax - gMin) * i / intervalCount);
                var bAverage = bMin + (int)((bMax - bMin) * i / intervalCount);
                colorList.Add(Color.FromRgb((byte)rAverage, (byte)gAverage, (byte)bAverage));
            }

            return colorList;

            //var intervalR = (byte)((color2.R - color1.R) / intervalCount);
            //var intervalG = (byte)((color2.G - color1.G) / intervalCount);
            //var intervalB = (byte)((color2.B - color1.B) / intervalCount);

            //var currentR = color1.R;
            //var currentG = color1.G;
            //var currentB = color1.G;

            //for (var i = 0; i <= intervalCount; i++) 
            //{
            //    yield return Color.FromRgb(currentR, currentG, currentB);

            //    currentR += intervalR;
            //    currentG += intervalG;
            //    currentB += intervalB;
            //}
        }
    }
}

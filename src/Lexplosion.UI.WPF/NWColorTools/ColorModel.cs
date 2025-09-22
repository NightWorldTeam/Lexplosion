using Lexplosion.UI.WPF.Tools;
using System;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.NWColorTools
{
    public class ColorModel
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public double Luminance { get; }
        public double Hue { get; }
        public double Saturation { get; }

        public int Max { get; }
        public int Min { get; }


        #region Constructors


        public ColorModel((byte, byte, byte) rbgTyple) : this(rbgTyple.Item1, rbgTyple.Item2, rbgTyple.Item3)
        {
            
        }

        public ColorModel(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;

            //Luminance = 0.2126 * R + 0.7152 * G + 0.0722 * B;
            Luminance = 0.299 * R + 0.587 * G + 0.114 * B;
            Max = Math.Max(Math.Max(r, g), b);
            Min = Math.Min(Math.Min(r, g), b);

            Hue = CalculateHue();
            Saturation = (double)(Max - Min) / Max;
        }


        #endregion Constructors


        #region Private Methods


        private double CalculateHue() 
        {
            double hue;

            double maxMinDiff = Max - Min;

            if (Max == R)
            {
                hue = (G - B) / maxMinDiff;
            }
            else if (Max == G)
            {
                hue = 2.0 + (B - R) / maxMinDiff;
            }
            else 
            {
                hue = 4 + (R - G) / maxMinDiff;
            }

            hue *= 60;
            if (hue < 0)
                hue += 360;

            return (int)Math.Round(hue);
        }


        #endregion Private Methods


        public string ToHex(bool hasNumberSign = true) 
        {
            var r = string.Format("{0:x}", R);
            var g = string.Format("{0:x}", G);
            var b = string.Format("{0:x}", B);

            var hexResult = $"{r}{g}{b}";

            return hasNumberSign ? $"#{hexResult}" : hexResult;
        }

        public static byte[] GetRedsFromArray(ColorModel[] rGBs)
        {
            byte[] reds = new byte[rGBs.Length];
            for (int i = 0; i < rGBs.Length; i++)
            {
                reds[i] = rGBs[i].R;
            }
            return reds;
        }

        public static byte[] GetGreensFromArray(ColorModel[] rGBs)
        {
            byte[] greens = new byte[rGBs.Length];
            for (int i = 0; i < rGBs.Length; i++)
            {
                greens[i] = rGBs[i].G;
            }
            return greens;
        }
        public static byte[] GetBluesFromArray(ColorModel[] rGBs)
        {
            byte[] blues = new byte[rGBs.Length];
            for (int i = 0; i < rGBs.Length; i++)
            {
                blues[i] = rGBs[i].B;
            }
            return blues;
        }
    }
}

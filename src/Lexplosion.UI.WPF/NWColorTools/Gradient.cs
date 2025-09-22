using System;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.NWColorTools
{
    public static class Gradient
    {
        public static Color[] GenerateGradient(Color startColor, Color endColor, int length)
        {
            var gradient = new Color[length];
            for (int i = 0; i < length; i++)
            {
                var r = (byte)(startColor.R + (endColor.R - startColor.R) * (double)i / (length - 1));
                var g = (byte)(startColor.G + (endColor.G - startColor.G) * (double)i / (length - 1));
                var b = (byte)(startColor.B + (endColor.B - startColor.B) * (double)i / (length - 1));
                gradient[i] = Color.FromRgb(r, g, b);
            }
            return gradient;
        }

        public static Color[] GenerateGradient(Color startColor, Color endColor, int length, bool correctToEnd = true)
        {
            var startHSV = new HSV(new ColorModel(startColor.R, startColor.G, startColor.B));
            var endHSV = new HSV(new ColorModel(endColor.R, endColor.G, endColor.B));

            if (startHSV.Hue - endHSV.Hue > 180)
            {
                endHSV.Hue += 360;
            }
            else if (startHSV.Hue - endHSV.Hue < -180)
            {
                startHSV.Hue += 360;
            }

            var hues = LinearInterpolate((int)startHSV.Hue, (int)endHSV.Hue, length);
            var saturations = LinearInterpolate(startHSV.Saturation, endHSV.Saturation, length);
            var values = LinearInterpolate(startHSV.Value, endHSV.Value, length);

            var colors = new Color[length];
            var HSVs = new ColorModel[length];
            for (var i = 0; i < length; i++)
            {
                HSVs[i] = new HSV(hues[i], saturations[i], values[i]).ToColorModel();
            }

            var reds = ColorModel.GetRedsFromArray(HSVs);
            var greens = ColorModel.GetGreensFromArray(HSVs);
            var blues = ColorModel.GetBluesFromArray(HSVs);

            for (var i = 0; i < length; i++)
            {
                colors[i] = Color.FromRgb(reds[i], greens[i], blues[i]);
            }

            return colors;
        }

        public static double[] LinearInterpolate(double start, double end, int length)
        {
            var stepCount = length - 1;
            double slope = (double)(end - start) / stepCount;
            double tmpStart = start;
            var gradientValues = new double[length];

            for (var i = 0; i < stepCount; i++)
            {
                gradientValues[i] = tmpStart;
                tmpStart += slope;
            }

            gradientValues[stepCount] = tmpStart;
            return gradientValues;
        }

        public static int[] LinearInterpolate(int start, int end, int length)
        {
            var stepCount = length - 1;
            double slope = (double)(end - start) / stepCount;
            double tmpStart = start;
            var gradientValues = new int[length];

            for (var i = 0; i < stepCount; i++)
            {
                gradientValues[i] = Convert.ToInt32(Math.Round(tmpStart));
                tmpStart += slope;
            }

            gradientValues[stepCount] = Convert.ToInt32(Math.Round(tmpStart));
            return gradientValues;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;

namespace Lexplosion.WPF.NewInterface.NWColorTools
{
    public static class Gradient
    {
        public static Color[] GenerateGradient(Color startColor, Color endColor, int lenght) 
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

            var hues = LinearInterpolate((int)startHSV.Hue, (int)endHSV.Hue, lenght);
            var saturations = LinearInterpolate(startHSV.Saturation, endHSV.Saturation, lenght);
            var values = LinearInterpolate(startHSV.Value, endHSV.Value, lenght);
            Console.WriteLine("Linear Interpolate executed");
            /**
            HSV[] HSVs = new HSV[lenght];

            for (var i = 0; i < lenght; i++) 
            {
                HSVs[i] = new HSV(hues[i], saturations[i], values[i]);
            }

            var colorModel = new ColorModel[lenght];

            for (var i = 0; i < lenght; i++) 
            {
                colorModel[i] = 
            }
            */

            var colors = new Color[lenght];
            var HSVs = new ColorModel[lenght];
            for (var i = 0; i < lenght; i++) {
                HSVs[i] = new HSV(hues[i], saturations[i], values[i]).ToColorModel();
            }

            var reds = ColorModel.GetRedsFromArray(HSVs);
            var greens = ColorModel.GetGreensFromArray(HSVs);
            var blues = ColorModel.GetBluesFromArray(HSVs);

            Console.WriteLine("Building Colors...");
            for (var i = 0; i < lenght; i++)
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

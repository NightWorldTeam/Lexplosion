using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexplosion.WPF.NewInterface.NWColorTools
{
    public class HSV
    {
        //private int _hue;
        //private double _saturation;
        // or brithness
        //private double _value;
        public double Hue { get; set; }
        public double Saturation { get; private set; }
        public double Value { get; private set; }


        #region Constructors


        public HSV(double hue, double saturation, double value)
        {
            Hue = hue;
            Saturation = saturation;
            Value = value;
        }

        public HSV(ColorModel colorModel)
        {
            Hue = colorModel.Hue;
            Saturation = colorModel.Saturation;
            Value = (double)colorModel.Max / 255;
        }


        #endregion Constructors


        #region Public & Protected Methods


        public static HSV[] ParseArraysToHSV(int[] hues, double[] saturations, double[] values)
        {
            if (hues.Length != saturations.Length && hues.Length != values.Length)
                return new HSV[0];

            HSV[] hsvValues = new HSV[hues.Length];
            for (var i = 0; i < hues.Length; i++)
            {
                hsvValues[i] = new(hues[i], saturations[i], values[i]);
            }
            return hsvValues;
        }


        public (byte, byte, byte) ToRGBTuple() 
        {
            double chroma = Value * Saturation;
            // intermediate value X for the second largest component of this color
            double x = chroma * (1 - Math.Abs(((double)Hue / 60) % 2 - 1));
            double matchValue = Value - chroma;

            double tempR = 0;
            double tempG = 0;
            double tempB = 0;

            if (Hue < 60 && Hue >= 0)
            {
                tempR = chroma;
                tempG = x;
                tempB = 0;
            }
            else if (Hue < 120 && Hue >= 60)
            {
                tempR = x;
                tempG = chroma;
                tempB = 0;
            }
            else if (Hue < 180 && Hue >= 120)
            {
                tempR = 0;
                tempG = chroma;
                tempB = x;
            }
            else if (Hue < 240 && Hue >= 180)
            {
                tempR = 0;
                tempG = x;
                tempB = chroma;
            }
            else if (Hue < 300 && Hue >= 240)
            {
                tempR = x;
                tempG = 0;
                tempB = chroma;
            }
            else if (Hue < 360 && Hue >= 300)
            {
                tempR = chroma;
                tempG = 0;
                tempB = x;
            }

            return (Convert.ToByte((tempR + matchValue) * 255), Convert.ToByte((tempG + matchValue) * 255), Convert.ToByte((tempB + matchValue) * 255));
        }

        public ColorModel ToColorModel() 
        {
            return new(ToRGBTuple());
        }


        #endregion Public & Protected Methods
    }
}

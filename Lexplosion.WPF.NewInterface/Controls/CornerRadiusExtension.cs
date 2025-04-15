using System;

namespace Lexplosion.WPF.NewInterface.Controls
{
    internal static class CornerRadiusExtension
    {
        public static bool IsValid(this System.Windows.CornerRadius cornerRadius, bool allowNegative, bool allowNaN, bool allowPositiveInfinity, bool allowNegativeInfinity)
        {
            var _topLeft = cornerRadius.TopLeft;
            var _topRight = cornerRadius.TopRight;
            var _bottomLeft = cornerRadius.BottomLeft;
            var _bottomRight = cornerRadius.BottomRight;

            if (!allowNegative)
            {
                if (_topLeft < 0d || _topRight < 0d || _bottomLeft < 0d || _bottomRight < 0d)
                {
                    return false;
                }
            }

            if (!allowNaN)
            {
                if (double.IsNaN(_topLeft) || double.IsNaN(_topRight) || double.IsNaN(_bottomLeft) || double.IsNaN(_bottomRight))
                {
                    return false;
                }
            }

            if (!allowPositiveInfinity)
            {
                if (double.IsPositiveInfinity(_topLeft) || double.IsPositiveInfinity(_topRight) || double.IsPositiveInfinity(_bottomLeft) || Double.IsPositiveInfinity(_bottomRight))
                {
                    return false;
                }
            }

            if (!allowNegativeInfinity)
            {
                if (double.IsNegativeInfinity(_topLeft) || double.IsNegativeInfinity(_topRight) || double.IsNegativeInfinity(_bottomLeft) || Double.IsNegativeInfinity(_bottomRight))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

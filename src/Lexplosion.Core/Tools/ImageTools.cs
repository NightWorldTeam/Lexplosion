using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Lexplosion.Tools
{
    public static class ImageTools
    {
        private static Random _random = new Random();

        public static byte[] ResizeImage(byte[] imageBytes, int width, int height)
        {
            if (imageBytes.Length == 0)
                return imageBytes;

            Image image;
            using (var ms = new MemoryStream(imageBytes))
            {
                image = Image.FromStream(ms);
            }

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            using (MemoryStream stream = new MemoryStream())
            {
                destImage.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                return stream.ToArray();
            }
        }

        public static byte[] GenerateRandomGradientImage(int width, int height)
        {
            var random = new Random();

            int value = random.Next(0, 16777215);
            int startR = (byte)value;
            int startG = (byte)(value >> 8);
            int startB = (byte)(value >> 16);
            MakeColorSaturated(ref startR, ref startG, ref startB);

            value = random.Next(0, 16777215);
            int endR = (byte)value;
            int endG = (byte)(value >> 8);
            int endB = (byte)(value >> 16);

            endR = ValueShift(startR, endR);
            endG = ValueShift(startG, endG);
            endB = ValueShift(startB, endB);

            var startColor = Color.FromArgb(startR, startG, startB);
            var endColor = Color.FromArgb(endR, endG, endB);

            return GenerateGradient(width, height, startColor, endColor, random.Next(0, 180));
        }

        public static (Color, Color) GenerateColorPair()
        {
            int value = _random.Next(0, 16777215);
            int startR = (byte)value;
            int startG = (byte)(value >> 8);
            int startB = (byte)(value >> 16);

            double lightness = (startR + startG + startB) / 3d;

            Color startColor = Color.FromArgb(startR, startG, startB);
            Color endColor;
            if (lightness > 112.5)
            {
                endColor = ChangeColorBrightness(startColor, -0.3);
            }
            else
            {
                endColor = ChangeColorBrightness(startColor, 0.3);
            }

            return (startColor, endColor);
        }

        public static Color ChangeColorBrightness(Color color, double lightness)
        {
            byte calcRes(double value)
            {
                if (value > 255) value = 255;
                else if (value < 0) value = 0;
                return (byte)value;
            }

            if (lightness > 0)
            {
                // стартовыми координатами делаем наш цвет (конечными коорднатами будет белый цвет)
                byte whiteColorR = (byte)(255 - color.R);
                byte whiteColorG = (byte)(255 - color.G);
                byte whiteColorB = (byte)(255 - color.B);

                var res = ColorShift(whiteColorR, whiteColorG, whiteColorB, lightness);
                //сейчас у нас нулевые координаты соотвествуют цвету color, поэтому прибавляем их к результату чтобы получить нужный цвет
                return Color.FromArgb(calcRes(res.Item1 + color.R), calcRes(res.Item2 + color.G), calcRes(res.Item3 + color.B));
            }
            else
            {
                var res = ColorShift(color.R, color.G, color.B, lightness + 1);
                return Color.FromArgb(calcRes(res.Item1), calcRes(res.Item2), calcRes(res.Item3));
            }
        }

        /// <summary>
        /// Высчитывает координаты между началом координатной оси и точкой с позицией (endPointR, endPointG, endPointB)
        /// в зависимости от shiftRatio. shiftRatio показывает для какой части пути от начала до точки нужно врнуть координаты.
        /// </summary>
        /// <param name="endPointR">Конечная точка R</param>
        /// <param name="endPointG">Конечная точка G</param>
        /// <param name="endPointB">Конечная точка B</param>
        /// <param name="shiftRatio">
        /// Коэфициент смещения по пути до точки (endPointR, endPointG, endPointB). 
        /// Принимает значение от 0 до 1. 
        /// При значении 1 путь будет полным, а значит возвращены будут переданные координаты. 
        /// При значении 0.5 будут возвращены координаты соответствующие половине пути до точки (endPointR, endPointG, endPointB)</param>
        /// <returns>Высчитанные координаты</returns>
        private static (double, double, double) ColorShift(byte endPointR, byte endPointG, byte endPointB, double shiftRatio)
        {
            double vectorLength = Math.Sqrt(endPointR * endPointR + endPointG * endPointG + endPointB * endPointB);
            double resultVectorLenght = vectorLength * shiftRatio;

            double vectorAngle = endPointR / vectorLength;

            double resultR = vectorAngle * resultVectorLenght;

            double vectorProjection = Math.Sqrt(vectorLength * vectorLength - endPointR * endPointR);
            double resultVectorProjection = Math.Sqrt(resultVectorLenght * resultVectorLenght - resultR * resultR);

            double resultG = (endPointG / vectorProjection) * resultVectorProjection;
            double resultB = (endPointB / vectorProjection) * resultVectorProjection;

            return (resultR, resultG, resultB);
        }

        public static Color ColorLighten(Color color, double lightness)
        {
            byte calcRes(double value)
            {
                if (value > 255) value = 255;
                else if (value < 0) value = 0;
                return (byte)value;
            }

            byte wayToWhiteR = (byte)(255 - color.R);
            byte wayToWhiteG = (byte)(255 - color.G);
            byte wayToWhiteB = (byte)(255 - color.B);

            double vectorLength = Math.Sqrt(wayToWhiteR * wayToWhiteR + wayToWhiteG * wayToWhiteG + wayToWhiteB * wayToWhiteB);
            double resultVectorLenght = vectorLength * lightness;

            double vectorAngle = wayToWhiteR / vectorLength;

            double resultR = vectorAngle * resultVectorLenght;

            double vectorProjection = Math.Sqrt(vectorLength * vectorLength - wayToWhiteR * wayToWhiteR);
            double resultVectorProjection = Math.Sqrt(resultVectorLenght * resultVectorLenght - resultR * resultR);

            double resultG = (wayToWhiteG / vectorProjection) * resultVectorProjection;
            double resultB = (wayToWhiteB / vectorProjection) * resultVectorProjection;

            return Color.FromArgb(calcRes(resultR + color.R), calcRes(resultG + color.G), calcRes(resultB + color.B));
        }

        private static void MakeColorSaturated(ref int r, ref int g, ref int b)
        {
            if (r > g && r > b)
            {
                if (g >= b) IncreaseDivergence(ref r, ref g);
                else IncreaseDivergence(ref r, ref b);
            }
            else if (g > r && g > b)
            {
                if (r >= b) IncreaseDivergence(ref g, ref r);
                else IncreaseDivergence(ref g, ref b);
            }
            else
            {
                if (r >= g) IncreaseDivergence(ref b, ref r);
                else IncreaseDivergence(ref b, ref g);
            }
        }

        private static void IncreaseDivergence(ref int maxValue, ref int minValue)
        {
            double diffRatio = minValue / maxValue;
            double ratio = diffRatio / 2;

            maxValue += (int)(maxValue * ratio);
            minValue -= (int)(minValue * ratio);

            if (minValue < 0)
            {
                int step = minValue * -1;
                minValue = 0;
                maxValue += step;
            }

            if (maxValue > 255) maxValue = 255;
        }

        private static int ValueShift(int compareValue, int targetValue)
        {
            double diffRatio;
            if (compareValue > targetValue) diffRatio = -1 * (targetValue / compareValue);
            else diffRatio = compareValue / targetValue;

            double ratio = diffRatio * 0.7;

            targetValue += (int)(targetValue * ratio);

            if (targetValue > 255) targetValue = 255;
            else if (targetValue < 0) targetValue = 0;

            return targetValue;
        }

        public static byte[] GenerateGradient(int width, int height, Color startColor, Color endColor, float angleDegrees = 0f)
        {
            using (Bitmap bmp = new Bitmap(width, height))
            {
                Rectangle rect = new Rectangle(0, 0, width, height);
                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

                IntPtr ptr = bmpData.Scan0;
                int bytes = Math.Abs(bmpData.Stride) * height;
                byte[] rgbValues = new byte[bytes];

                // Преобразуем угол в радианы
                float angle = angleDegrees * (float)Math.PI / 180f;
                float cosAngle = (float)Math.Cos(angle);
                float sinAngle = (float)Math.Sin(angle);

                // Центр изображения
                float centerX = width / 2f;
                float centerY = height / 2f;

                // Максимальное расстояние от центра до угла
                float maxDistance = (float)Math.Sqrt(centerX * centerX + centerY * centerY);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Вычисляем позицию относительно центра
                        float relX = x - centerX;
                        float relY = y - centerY;

                        // Проецируем точку на направление градиента
                        float distance = relX * cosAngle + relY * sinAngle;

                        // Нормализуем значение от 0 до 1
                        float t = (distance + maxDistance) / (2 * maxDistance);
                        t = Math.Max(0, Math.Min(1, t)); // Ограничиваем диапазон

                        // Интерполируем цвета
                        int r = Interpolate(startColor.R, endColor.R, t);
                        int g = Interpolate(startColor.G, endColor.G, t);
                        int b = Interpolate(startColor.B, endColor.B, t);
                        int a = Interpolate(startColor.A, endColor.A, t);

                        int position = y * bmpData.Stride + x * 4;
                        rgbValues[position] = (byte)b;     // B
                        rgbValues[position + 1] = (byte)g; // G
                        rgbValues[position + 2] = (byte)r;  // R
                        rgbValues[position + 3] = (byte)a;  // A
                    }
                }

                Marshal.Copy(rgbValues, 0, ptr, bytes);
                bmp.UnlockBits(bmpData);
                return ImageToByte(bmp);
            }
        }

        private static int Interpolate(int start, int end, float t)
        {
            return (int)(start + (end - start) * t);
        }

        public static byte[] ImageToByte(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}

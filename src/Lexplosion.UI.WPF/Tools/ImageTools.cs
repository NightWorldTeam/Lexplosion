using System;
using System.Drawing;
using System.IO;
using System.Windows;

using System.Windows.Media.Imaging;

namespace Lexplosion.UI.WPF.Core.Tools
{
    public static class ImageTools
    {
        const string pathToDefaultImage = "pack://Application:,,,/Assets/images/icons/non_image.png";
        public static readonly BitmapImage defaultBitmapImage = new BitmapImage(new Uri(pathToDefaultImage));
        public static readonly BitmapImage defaultBitmapImageTemplate = new BitmapImage(new Uri("pack://Application:,,,/Assets/images/icons/non_image_template.png"));

        /// <summary>
        /// Приводит массив байт в bitmapimage.
        /// </summary>
        /// <param name="bytes">Массив байт изображения</param>
        /// <returns>Итоговое изображение</returns>
        public static BitmapImage ToImage(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return defaultBitmapImage;

            return ToImageWithoutValidate(bytes);
        }

        /// <summary>
        /// Приводит массив байт в bitmapimage, не производит проверок на nullю
        /// </summary>
        /// <param name="bytes">Массив байт изображения</param>
        /// <returns>Итоговое изображение</returns>
        private static BitmapImage ToImageWithoutValidate(byte[] bytes)
        {
            try
            {
                using (var stream = new System.IO.MemoryStream(bytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
            }
            catch
            {
                return defaultBitmapImage;
            }
        }

        public static byte[] GenerateRandomIcon()
        {
            byte calcRes(double value)
            {
                if (value > 255) value = 255;
                else if (value < 0) value = 0;
                return (byte)value;
            }

            BitmapImage template = defaultBitmapImageTemplate;

            var colors = Lexplosion.Tools.ImageTools.GenerateColorPair();
            Color topColor = colors.Item1;
            Color bottomColor = colors.Item2;

            // Получаем размеры изображения
            int width = template.PixelWidth;
            int height = template.PixelHeight;

            // Вычисляем количество байтов на пиксель (обычно 4 для формата Pbgra32)
            int bytesPerPixel = (template.Format.BitsPerPixel + 7) / 8;
            int stride = width * bytesPerPixel;

            // Создаем массив для хранения пикселей
            byte[] templatePixels = new byte[height * stride];

            // Копируем пиксели из WriteableBitmap в массив
            template.CopyPixels(templatePixels, stride, 0);

            // Проходим по всем пикселям и заменяем цвет
            for (int i = 0; i < templatePixels.Length; i += bytesPerPixel)
            {
                byte templateB = templatePixels[i];
                byte templateG = templatePixels[i + 1];
                byte templateR = templatePixels[i + 2];
                byte templateA = templatePixels[i + 3];

                //это фон, его перерисовывать не надо
                if (templateR == 21 && templateG == 23 && templateB == 25) continue;

                Color colorToPlace;
                double blueRatio = (double)templateB / (templateG + templateR);

                //коэфициент синего больше 0.6, значит это верхняя часть картинки
                double lightnessRatio;
                if (blueRatio > 0.6)
                {
                    colorToPlace = topColor;
                    lightnessRatio = (templateR + templateG + templateB) / 510d;
                }
                else
                {
                    lightnessRatio = (templateR + templateG + templateB) / 255d;
                    colorToPlace = bottomColor;
                }

                templatePixels[i] = calcRes(colorToPlace.B * lightnessRatio);     // Синий
                templatePixels[i + 1] = calcRes(colorToPlace.G * lightnessRatio);   // Зеленый
                templatePixels[i + 2] = calcRes(colorToPlace.R * lightnessRatio);  // Красный
                templatePixels[i + 3] = templateA;  // Альфа
            }

            // Создаем новый WriteableBitmap и записываем измененные пиксели
            WriteableBitmap resultBitmap = new WriteableBitmap(width, height, template.DpiX, template.DpiY, template.Format, null);
            resultBitmap.WritePixels(new Int32Rect(0, 0, width, height), templatePixels, stride, 0);

            // Конвертируем WriteableBitmap обратно в BitmapImage
            return ConvertBitmapToByteArray(resultBitmap);
        }

        public static byte[] ConvertBitmapToByteArray(WriteableBitmap bitmap)
        {
            byte[] data;
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }

            return data;
        }
    }
}

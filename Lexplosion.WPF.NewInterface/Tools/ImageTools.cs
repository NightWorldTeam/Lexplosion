using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lexplosion.WPF.NewInterface.Core.Tools
{
    public static class ImageTools
    {
        const string pathToDefaultImage = "pack://Application:,,,/Assets/images/icons/non_image.png";
        public static readonly BitmapImage defaultBitmapImage = new BitmapImage(new Uri(pathToDefaultImage));

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
    }
}

using System;
using System.Windows.Media.Imaging;

namespace Lexplosion.WPF.NewInterface.Core.Tools
{
    public static class ImageTools
    {
        public static BitmapImage ToImage(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return new BitmapImage(new Uri("pack://application:,,,/assets/images/icons/non_image.png"));

            using (var stream = new System.IO.MemoryStream(imageBytes))
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
    }
}

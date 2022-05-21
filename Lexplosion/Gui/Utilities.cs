using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Lexplosion.Gui
{
    public class Utilities
    {
        public static BitmapImage ToImage(byte[] array)
        {
            if (array is null || array.Length == 0)
                return new BitmapImage(new Uri("pack://application:,,,/assets/images/icons/non_image.png"));
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = new System.IO.MemoryStream(array);
            image.EndInit();
            return image;
        }

        public static BitmapImage GetImage(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    using (var fstream = new FileStream(path, FileMode.Open))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad; // here
                        image.StreamSource = fstream;
                        image.EndInit();
                        return image;
                    }
                }
                catch { return new BitmapImage(new Uri("pack://application:,,,/assets/images/icons/non_image.png")); }
            }
            else return new BitmapImage(new Uri("pack://application:,,,/assets/images/icons/non_image.png"));
        }
    }
}

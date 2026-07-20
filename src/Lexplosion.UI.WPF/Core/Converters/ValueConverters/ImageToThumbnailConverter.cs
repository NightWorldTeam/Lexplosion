using Lexplosion.Tools;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Media.Imaging;

namespace Lexplosion.UI.WPF.Core.Converters
{
    public sealed class ImageToThumbnailConverter : ConverterBase<ImageToThumbnailConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (!(parameter is double[] sizes))
            {
                throw new ArgumentException("Parameter must be double[] contains size of Thumbnail.");
            }

            if (sizes == null)
            {
                throw new ArgumentNullException("Parameter must be double[] containes size of Thumbnail not NULL");
            }

            if (sizes.Length != 2)
            {
                throw new ArgumentException($"Thumbnail size parameter must be contains two elements but Length = {sizes.Length}");
            }

            if (!(sizes[0] > 0 && sizes[1] > 0))
            {
                throw new ArgumentException($"Thumbnail size parameters must be more than 0");
            }

			if (value is byte[] imageArray)
			{
				try
				{
					// Notice we return BitmapSource directly instead of byte[]
					return ResizeImageWpf(imageArray, (int)sizes[0], (int)sizes[1]);
				}
				catch
				{
					Runtime.DebugWrite("[Error] Image resize failed", color: ConsoleColor.DarkGray);
				}
			}

			return null;
        }

		public static BitmapSource? ResizeImageWpf(byte[] imageBytes, int width, int height)
		{
			if (imageBytes == null || imageBytes.Length == 0)
				return null;

			using (var ms = new MemoryStream(imageBytes))
			{
				// Use DecodePixelWidth so the decoder downsamples during read,
				// avoiding allocation of full-resolution pixel buffers in memory.
				// For a 4000x3000 source decoded to width=200, this saves ~95% memory.
				var image = new BitmapImage();
				image.BeginInit();
				image.DecodePixelWidth = width;
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = ms;
				image.EndInit();

				// Freeze so it can be passed across threads and used by UI without issues.
				if (image.CanFreeze)
				{
					image.Freeze();
				}

				return image;
			}
		}
	}
}

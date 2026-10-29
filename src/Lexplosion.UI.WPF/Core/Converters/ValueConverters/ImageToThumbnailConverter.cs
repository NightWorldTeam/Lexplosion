using Lexplosion.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
#if DEBUG
using Lexplosion.UI.WPF.Core;
#endif

namespace Lexplosion.UI.WPF.Core.Converters
{
    public sealed class ImageToThumbnailConverter : ConverterBase<ImageToThumbnailConverter>
    {
        // Image arrays are owned by the gallery model. The cache does not keep
        // those arrays alive after a gallery is closed.
        private static readonly ConditionalWeakTable<byte[], ThumbnailCache> Thumbnails =
            new ConditionalWeakTable<byte[], ThumbnailCache>();

        private sealed class ThumbnailCache
        {
            internal readonly Dictionary<int, BitmapSource> ByWidth =
                new Dictionary<int, BitmapSource>();
        }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (!(parameter is double[] sizes) || sizes.Length != 2 ||
                double.IsNaN(sizes[0]) || double.IsInfinity(sizes[0]) ||
                double.IsNaN(sizes[1]) || double.IsInfinity(sizes[1]) ||
                sizes[0] <= 0 || sizes[1] <= 0)
                throw new ArgumentException("Parameter must contain two positive thumbnail dimensions.");

            if (!(value is byte[] bytes) || bytes.Length == 0)
                return null;

            try
            {
                int width = (int)sizes[0];
                if (width <= 0) return null;
                var cache = Thumbnails.GetValue(bytes, _ => new ThumbnailCache());
                lock (cache.ByWidth)
                {
                    if (cache.ByWidth.TryGetValue(width, out var cached))
                        return cached;

#if DEBUG
                    using (UiDebugTrace.Measure("gallery.thumbnail.decode", 3))
#endif
                    {
                        // WIC decodes directly to the thumbnail size. The old
                        // converter performed a full GDI+ decode, high-quality
                        // resize and BMP encoding synchronously on the UI thread.
                        var thumbnail = new BitmapImage();
                        using (var stream = new MemoryStream(bytes, false))
                        {
                            thumbnail.BeginInit();
                            thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                            thumbnail.DecodePixelWidth = width;
                            thumbnail.StreamSource = stream;
                            thumbnail.EndInit();
                        }
                        thumbnail.Freeze();
                        cache.ByWidth.Add(width, thumbnail);
                        return thumbnail;
                    }
                }
            }
            catch (Exception error)
            {
                Runtime.DebugWrite("[Error] Thumbnail decode failed: " + error.GetType().Name,
                    color: ConsoleColor.DarkGray);
                return null;
            }
        }
    }
}

using Lexplosion.Tools;
using System;
using System.Globalization;

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
                return ImageTools.ResizeImage(imageArray, (int)sizes[0], (int)sizes[1]);
            }

            return null;
        }
    }
}

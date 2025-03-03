using Lexplosion.WPF.NewInterface.Core.Tools;
using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    /// <summary>
    /// Переводит объект в ImageSource.
    /// Если объект пустой или NULL, создаёт значение со стандратным изображение.
    /// </summary>
    public sealed class ImageSourceNullValidateConverter : ConverterBase<ImageSourceNullValidateConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ImageSource resBitmapImage = ImageTools.defaultBitmapImage;
            if (value == null)
            {
                return new ImageBrush(resBitmapImage);
            }

            if (value is byte[])
            {
                var bytes = value as byte[];
                resBitmapImage = ImageTools.ToImageWithoutValidate(bytes);
            }
            else if (value is string)
            {
                var str = value as string;
                if (str.Length != 0)
                    resBitmapImage = new BitmapImage(new Uri(str));
            }
            else if (value is ImageSource)
            {
                resBitmapImage = value as ImageSource;
            }

            return new ImageBrush(resBitmapImage);
        }
    }
}

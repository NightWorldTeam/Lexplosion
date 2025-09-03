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
            Stretch stretch = parameter == null || parameter is not Stretch ? Stretch.Fill : (Stretch)(parameter);
            ImageSource resBitmapImage = ImageTools.defaultBitmapImage;
            ImageBrush imageBrush = null;

            if (value == null)
            {
                return new ImageBrush(resBitmapImage);
            }

            try
            {
                if (value is byte[])
                {
                    var bytes = value as byte[];
                    resBitmapImage = ImageTools.ToImage(bytes);
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
            }
            catch
            {
                return new ImageBrush(resBitmapImage);
            }

            imageBrush = new ImageBrush(resBitmapImage);
            imageBrush.Stretch = stretch;
            return imageBrush;
        }
    }
}

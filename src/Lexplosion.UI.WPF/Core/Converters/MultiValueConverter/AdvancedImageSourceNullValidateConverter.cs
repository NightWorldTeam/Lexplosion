using Lexplosion.UI.WPF.Core.Converters.MultiValueConverter;
using Lexplosion.UI.WPF.Core.Tools;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Input;

namespace Lexplosion.UI.WPF.Core.Converters
{
    /// <summary>
    /// Переводит объект в ImageSource.
    /// Если объект пустой или NULL, создаёт значение со стандратным изображение.
    /// Если передать в качестве параметры Action, Action будет вызван после загрузки изображения.
    /// </summary>
    public sealed class AdvancedImageSourceNullValidateConverter : MultiValueConverterBase<AdvancedImageSourceNullValidateConverter>
    {
        public static readonly ImageSource _defaultBitmapValue = ImageTools.defaultBitmapImage;

        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 0) 
            {
                return new ImageBrush(_defaultBitmapValue);
            }

            var resBitmapImage = _defaultBitmapValue;
            var value = values[0];
            var loadedCommand = values[1] as ICommand;
            var loadedCommandParameter = (object)values[2];

            if (value == null)
            {
                return new ImageBrush(resBitmapImage);
            }

            // Если значенгие массив byte
            if (value is byte[])
            {
                var bytes = value as byte[];
                resBitmapImage = ImageTools.ToImage(bytes);

                if (parameter is Action imageLoaded)
                {
                    imageLoaded();
                }

                return new ImageBrush(resBitmapImage);
            }
            // Если значение строка
            if (value is string)
            {
                return LoadImageString(value as string, () => loadedCommand?.Execute(loadedCommandParameter));
            }
            // Если значение imagesource
            if (value is ImageSource)
            {
                resBitmapImage = value as ImageSource;
                return new ImageBrush(resBitmapImage);
            }

            throw new ArgumentException($"Unexceptable argument! Type: {value.GetType()} Value: {value}");
        }




        private ImageBrush LoadImageString(string value, Action loaded = null)
        {
            if (value.Length == 0)
            {
                return new ImageBrush(_defaultBitmapValue);
            }

            var bitmap = new BitmapImage(new Uri(value));

            bitmap.DownloadCompleted += (sender, args) =>
            {
                loaded?.Invoke();
            };

            bitmap.DownloadFailed += (sender, args) =>
            {
                loaded?.Invoke();
            };

            return new ImageBrush(bitmap);
        }
    }
}

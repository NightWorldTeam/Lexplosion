using Lexplosion.Tools;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Lexplosion.Common.Converters
{
    internal class UrlToImageConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                var url = (string)value;

                return ImageTools.GetImageByUrl(url);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

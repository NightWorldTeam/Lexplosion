using Lexplosion.Tools;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Lexplosion.Gui.Converters
{
    public class UrlToImageConveter : IValueConverter
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
    }
}

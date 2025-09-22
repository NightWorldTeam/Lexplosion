using System;
using System.Globalization;
using System.Linq;

namespace Lexplosion.WPF.NewInterface.Core.Converters
{
    public class ImagePathValidateConverter : ConverterBase<ImagePathValidateConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path && path.Contains("pack://application:,,,/")) 
            {
                if (RuntimeApp.ResourceNames.Contains(path.Replace("pack://application:,,,/", "").ToLower()))
                    return path;
            }

            return "pack://application:,,,/assets/images/icons/non_image.png";
        }
    }
}

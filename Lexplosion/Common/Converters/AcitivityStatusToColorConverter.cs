using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Lexplosion.Common.Converters
{
    public class AcitivityStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (ActivityStatus)value;

            if (status == ActivityStatus.Online)
            {
                return (Brush)new BrushConverter().ConvertFrom("#167FFC");
            }
            else if (status == ActivityStatus.InGame)
            {
                return Brushes.Green;
            }
            else if (status == ActivityStatus.NotDisturb)
            {
                return Brushes.Red;
            }
            else if (status == ActivityStatus.Offline)
            {
                return Brushes.Gray;
            }
            return Brushes.Lavender;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

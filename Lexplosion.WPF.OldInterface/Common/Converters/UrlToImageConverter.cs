using Lexplosion.Tools;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Lexplosion.Common.Converters
{
    internal class UrlToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                var url = value as string;

                return new AsyncTask(() => 
                {
                    return ImageTools.GetImageByUrl(url);
                });
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public class AsyncTask : INotifyPropertyChanged
        {
            public AsyncTask(Func<object> valueFunc)
            {
                AsyncValue = "loading async value";
                LoadValue(valueFunc);
            }

            private async Task LoadValue(Func<object> valueFunc)
            {
                AsyncValue = await Task<object>.Run(() =>
                {
                    return valueFunc();
                });
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("AsyncValue"));
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public object AsyncValue { get; set; }
        }
    }
}

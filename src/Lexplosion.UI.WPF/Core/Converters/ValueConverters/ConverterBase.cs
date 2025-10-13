using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Lexplosion.UI.WPF.Core.Converters
{
    public abstract class ConverterBase<T> : MarkupExtension, IValueConverter where T : class, new()
    {
        private static T _instance;


        #region Constructors


        static ConverterBase()
        {
            ConverterBase<T>._instance = Activator.CreateInstance<T>();
        }

        protected ConverterBase()
        {
        }


        #endregion Constructors


        #region Public & Protected Methods


        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default(object);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (ConverterBase<T>._instance == null)
            {
                ConverterBase<T>._instance = Activator.CreateInstance<T>();
            }
            return ConverterBase<T>._instance;
        }


        #endregion Public & Protected Methods
    }
}
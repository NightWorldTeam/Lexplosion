using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Lexplosion.WPF.NewInterface.Core.Converters.MultiValueConverter
{
    public abstract class MultiValueConverterBase<T> : MarkupExtension, IMultiValueConverter where T : class, new()
    {
        private static T _instance;


        #region Constructors


        static MultiValueConverterBase()
        {
            MultiValueConverterBase<T>._instance = Activator.CreateInstance<T>();
        }

        protected MultiValueConverterBase()
        {
        }


        #endregion Constructors


        #region Public & Protected Methods


        public abstract object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);
        public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (MultiValueConverterBase<T>._instance == null)
            {
                MultiValueConverterBase<T>._instance = Activator.CreateInstance<T>();
            }
            return MultiValueConverterBase<T>._instance;
        }


        #endregion Public & Protected Methods
    }
}

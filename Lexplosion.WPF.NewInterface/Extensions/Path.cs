using System.Windows.Media;
using System.Windows;

namespace Lexplosion.WPF.NewInterface.Extensions
{
    public static class Path
    {
        #region String Data Property


        public static DependencyProperty StringDataProperty
            = DependencyProperty.RegisterAttached("StringData", typeof(string), typeof(Path), new PropertyMetadata(
                string.Empty, OnStringDataChanged));

        private static void OnStringDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Shapes.Path)
            {
                var path = d as System.Windows.Shapes.Path;
                if (path.Data == e.OldValue)
                    return;
                path.Data = Geometry.Parse(e.NewValue.ToString());
            }
        }

        public static void SetStringData(DependencyObject dp, string value)
        {
            dp.SetValue(StringDataProperty, value);
        }

        public static string GetStringData(DependencyObject dp)
        {
            return (string)dp.GetValue(StringDataProperty);
        }


        #endregion String Data Property


        #region String Key Data Property


        public static DependencyProperty StringKeyDataProperty
            = DependencyProperty.RegisterAttached("StringKeyData", typeof(string), typeof(Path), new FrameworkPropertyMetadata(string.Empty, OnStringKeyDataChanged));

        private static void OnStringKeyDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Shapes.Path)
            {
                var path = d as System.Windows.Shapes.Path;
                path.Data = Geometry.Parse((string)App.Current.Resources[e.NewValue]);
            }
        }

        public static void SetStringKeyData(DependencyObject dp, string value)
        {
            dp.SetValue(StringKeyDataProperty, value);
        }

        public static string GetStringKeyData(DependencyObject dp)
        {
            return (string)dp.GetValue(StringKeyDataProperty);
        }


        #endregion String Key Data Property
    }
}

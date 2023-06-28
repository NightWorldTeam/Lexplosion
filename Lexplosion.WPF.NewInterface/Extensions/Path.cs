using System.Windows.Media;
using System.Windows;

namespace Lexplosion.WPF.NewInterface.Extensions
{
    public sealed class Path
    {
        public static readonly DependencyProperty StringDataProperty
            = DependencyProperty.Register(
                "StringData",
                typeof(string),
                typeof(System.Windows.Shapes.Path),
                new PropertyMetadata(
                    string.Empty, OnStringDataChanged
                    )
                );

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
    }
}

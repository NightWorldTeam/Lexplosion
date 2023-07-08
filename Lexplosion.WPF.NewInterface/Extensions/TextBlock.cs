using System.Windows;

namespace Lexplosion.WPF.NewInterface.Extensions
{
    public static class TextBlock
    {
        public static readonly DependencyProperty TextByKeyProperty
            = DependencyProperty.RegisterAttached("TextByKey", typeof(string), typeof(TextBlock),
                new FrameworkPropertyMetadata(string.Empty, OnTextByKeyChanged));

        public static void SetTextByKey(DependencyObject dp, string value)
        {
            dp.SetValue(TextByKeyProperty, value);
        }

        public static string GetTextByKey(DependencyObject dp)
        {
            return (string)dp.GetValue(TextByKeyProperty);
        }

        private static void OnTextByKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is System.Windows.Controls.TextBlock)
            {
                var textBlock = d as System.Windows.Controls.TextBlock;
                textBlock.SetResourceReference(System.Windows.Controls.TextBlock.TextProperty, e.NewValue);
            }
        }
    }
}

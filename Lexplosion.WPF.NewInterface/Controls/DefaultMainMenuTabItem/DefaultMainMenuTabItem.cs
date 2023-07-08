using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public class DefaultMainMenuTabItem : RadioButton
    {
        public static readonly DependencyProperty IconKeyProperty
            = DependencyProperty.Register("IconKey", typeof(string), typeof(DefaultMainMenuTabItem), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty TextKeyProperty
            = DependencyProperty.Register("TextKey", typeof(string), typeof(DefaultMainMenuTabItem), new FrameworkPropertyMetadata(string.Empty));

        public string IconKey
        {
            get => (string)GetValue(IconKeyProperty);
            set => SetValue(IconKeyProperty, value);
        }

        public string TextKey 
        {
            get => (string)GetValue(IconKeyProperty);
            set => SetValue(TextKeyProperty, value);
        }

        static DefaultMainMenuTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DefaultMainMenuTabItem), new FrameworkPropertyMetadata(typeof(DefaultMainMenuTabItem)));
        }

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            Runtime.DebugWrite("test");
        }
    }
}

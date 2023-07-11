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

        public static readonly DependencyProperty IconWidthProperty
            = DependencyProperty.Register("IconWidth", typeof(double), typeof(DefaultMainMenuTabItem), new FrameworkPropertyMetadata(20.0));

        public static readonly DependencyProperty IconHeightProperty
            = DependencyProperty.Register("IconHeight", typeof(double), typeof(DefaultMainMenuTabItem), new FrameworkPropertyMetadata(20.0));

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

        public double IconWidth 
        {
            get => (double)GetValue(IconWidthProperty);
            set => SetValue(IconWidthProperty, value);
        }

        public double IconHeight 
        {
            get => (double)GetValue(IconHeightProperty);    
            set => SetValue(IconHeightProperty, value);
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

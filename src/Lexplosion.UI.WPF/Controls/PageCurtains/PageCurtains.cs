using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Controls
{
    public sealed class PageCurtains : ContentControl
    {
        public static readonly DependencyProperty IsActiveProperty
            = DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(PageCurtains),
            new PropertyMetadata(defaultValue: false, propertyChangedCallback: OnIsEnabledChanged));

        public PageCurtains()
        {
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as PageCurtains;
            //_this.Visibility = !(bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

    }
}

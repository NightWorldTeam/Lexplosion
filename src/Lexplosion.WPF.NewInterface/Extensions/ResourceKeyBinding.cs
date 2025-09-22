using System.Windows;

namespace Lexplosion.WPF.NewInterface.Extensions
{
    public static class ResourceKeyBinding
    {
        // стандартное attached property ResourceKey
        public static object GetResourceKey(DependencyObject obj) =>
            obj.GetValue(ResourceKeyProperty);
        public static void SetResourceKey(DependencyObject obj, object value) =>
            obj.SetValue(ResourceKeyProperty, value);

        public static readonly DependencyProperty ResourceKeyProperty =
            DependencyProperty.RegisterAttached(
                "ResourceKey", typeof(object), typeof(ResourceKeyBinding),
                // callback при изменении значения ResourceKey
                new PropertyMetadata(OnResourceKeyChanged));

        // стандартное attached property ResourceValue
        public static object GetResourceValue(DependencyObject obj) =>
            obj.GetValue(ResourceValueProperty);
        public static void SetResourceValue(DependencyObject obj, object value) =>
            obj.SetValue(ResourceValueProperty, value);

        public static readonly DependencyProperty ResourceValueProperty =
            DependencyProperty.RegisterAttached(
                "ResourceValue", typeof(object), typeof(ResourceKeyBinding));

        // это будет вызвано при изменении значения ResourceKey:
        static void OnResourceKeyChanged(
            DependencyObject self, DependencyPropertyChangedEventArgs e)
        {
            // SetResourceReference устанавливает dynamic resource-привязку
            ((FrameworkElement)self).SetResourceReference(ResourceValueProperty, e.NewValue);
        }
    }
}

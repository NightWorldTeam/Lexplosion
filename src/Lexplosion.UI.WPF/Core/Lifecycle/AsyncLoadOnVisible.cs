using Lexplosion.Logic.Lifecycle;
using System;
using System.Threading;
using System.Windows;

namespace Lexplosion.UI.WPF.Core.Lifecycle
{
    /// <summary>
    /// WPF-only activation adapter. Data loaders do not depend on WPF or on
    /// animation timing. DataContext must implement IAsyncLoadable.
    /// </summary>
    public static class AsyncLoadOnVisible
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool),
                typeof(AsyncLoadOnVisible), new PropertyMetadata(false, OnEnabledChanged));

        public static bool GetEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject element, bool value)
        {
            element.SetValue(EnabledProperty, value);
        }

        private static void OnEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) return;

            if ((bool)e.NewValue)
            {
                element.Loaded += OnActivated;
                element.IsVisibleChanged += OnIsVisibleChanged;
                element.DataContextChanged += OnDataContextChanged;
                TryLoad(element);
            }
            else
            {
                element.Loaded -= OnActivated;
                element.IsVisibleChanged -= OnIsVisibleChanged;
                element.DataContextChanged -= OnDataContextChanged;
            }
        }

        private static void OnActivated(object sender, RoutedEventArgs e)
        {
            TryLoad((FrameworkElement)sender);
        }

        private static void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TryLoad((FrameworkElement)sender);
        }

        private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TryLoad((FrameworkElement)sender);
        }

        private static async void TryLoad(FrameworkElement element)
        {
            if (!element.IsLoaded || !element.IsVisible ||
                !(element.DataContext is IAsyncLoadable loadable))
                return;

            try
            {
                // EnsureLoadedAsync shares an in-flight request, including if
                // Loaded/IsVisibleChanged/DataContextChanged fire together.
                await loadable.EnsureLoadedAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                // A view activation must not propagate an async void exception
                // into the WPF Dispatcher; model owns its error state.
                Runtime.DebugWrite("Async screen activation failed: " +
                    exception.GetType().Name);
            }
        }
    }
}

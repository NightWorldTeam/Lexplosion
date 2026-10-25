using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Lexplosion.UI.WPF.Core.Converters
{
    // WPF works in device-independent pixels. The same breakpoints are shared by
    // the addon details XAML styles, avoiding SizeChanged layout mutations.
    public sealed class AddonDetailsBreakpointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double width)) return false;
            switch (parameter as string)
            {
                case "Compact": return width < 1120;
                case "Regular": return width >= 1120;
                case "Wide": return width >= 1340;
                case "Large": return width >= 1480;
                case "ExtraWide": return width >= 1700;
                case "ExtraLarge": return width >= 1880;
                default: return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

    // One inset calculation is used for the tab headers, browser and both WPF tabs.
    public sealed class AddonDetailsContentMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double width = value is double measured ? measured : 944;
            double left = width < 1120 ? 66 : 74;
            switch (parameter as string)
            {
                case "Gallery": return new Thickness(left, 26, 28, 36);
                case "Versions": return new Thickness(left, 26, 28, 24);
                case "Browser": return new Thickness(left, 0, 0, 0);
                default: return new Thickness(left, 0, 0, 0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }

    // ColumnDefinition is not a FrameworkElement; a MultiBinding provides both
    // the current width and the user's info-panel toggle state declaratively.
    public sealed class AddonDetailsSidebarWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double width = values.Length > 0 && values[0] is double w ? w : 944;
            bool opened = values.Length > 1 && values[1] is bool b && b;
            bool wide = width >= 1340;
            if (!wide && !opened) return new GridLength(0);
            double panelWidth = wide ? width >= 1700 ? 382 : 348
                : Math.Min(332, Math.Max(248, width * 0.41));
            return new GridLength(panelWidth);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => null;
    }
}

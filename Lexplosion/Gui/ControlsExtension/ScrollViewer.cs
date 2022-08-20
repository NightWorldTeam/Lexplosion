using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lexplosion.Gui.Extension
{
    public static class ScrollViewer
    {
        public static readonly DependencyProperty VerticalOffsetValueProperty
            = DependencyProperty.Register("VerticalOffsetValue", typeof(double), typeof(System.Windows.Controls.ScrollViewer), new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static double GetVerticalOffsetValue(DependencyObject d) 
        {
            return (double)d.GetValue(VerticalOffsetValueProperty);
        }

        public static void SetVerticalOffsetValue(DependencyObject d, double value) 
        { 
            d.SetValue(VerticalOffsetValueProperty, value);
        }

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            var scrollViewer = (System.Windows.Controls.ScrollViewer)d;

            scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Controls
{
    public class Modal : ContentControl
    {
        public static readonly DependencyProperty IsOpenProperty
            = DependencyProperty.Register(
                "IsOpen",
                typeof(bool),
                typeof(Modal),
                new PropertyMetadata(false)
                );

        public static readonly DependencyProperty BackgroundOpacityProperty
            = DependencyProperty.Register(
                "BackgroundOpacityProperty",
                typeof(double),
                typeof(Modal),
                new FrameworkPropertyMetadata(0.7)
                );

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public double BackgroundOpacity
        {
            get => (double)GetValue(BackgroundOpacityProperty);
            set => SetValue(BackgroundOpacityProperty, value);
        }

        static Modal()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Modal), new FrameworkPropertyMetadata(typeof(Modal)));
        }
    }
}

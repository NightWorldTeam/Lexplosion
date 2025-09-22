using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Core
{
    public sealed class SegmentItem : ContentControl
    {
        public event Action TextChanged;

        public static readonly DependencyProperty KeyProperty
            = DependencyProperty.Register(nameof(Text), typeof(string), typeof(SegmentItem),
            new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnKeyPropertyChanged));

        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register(nameof(Value), typeof(object), typeof(SegmentItem),
                new FrameworkPropertyMetadata(null));


        public string Text
        {
            get => (string)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public object Value
        {
            get => (object)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }


        private static void OnKeyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = (d as SegmentItem);
            _this.Text = e.NewValue as string;
            _this.TextChanged?.Invoke();
        }
    }
}

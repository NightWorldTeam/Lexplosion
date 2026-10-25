using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Lexplosion.UI.WPF.Core;

namespace Lexplosion.UI.WPF.Controls
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

        private FrameworkElement _selectedLayer;
        private int _selectionAnimationVersion;

        public DefaultMainMenuTabItem()
        {
            Checked += (sender, args) => UpdateSelectedAppearance(true);
            Unchecked += (sender, args) => UpdateSelectedAppearance(false);
        }

        public override void OnApplyTemplate()
        {
            ++_selectionAnimationVersion;
            base.OnApplyTemplate();
            _selectedLayer = GetTemplateChild("PART_SelectedLayer") as FrameworkElement;
            if (_selectedLayer != null)
                _selectedLayer.Opacity = IsChecked == true ? 1 : 0;
        }

        private void UpdateSelectedAppearance(bool selected)
        {
            if (_selectedLayer == null)
                return;

            var layer = _selectedLayer;
            double opacity = layer.Opacity;
            double target = selected ? 1 : 0;
            int version = ++_selectionAnimationVersion;

            layer.BeginAnimation(OpacityProperty, null);
            layer.Opacity = opacity;

            if (!IsLoaded || !AnimationPreferences.Enabled)
            {
                layer.Opacity = target;
                return;
            }

            var animation = new DoubleAnimation(opacity, target, new Duration(TimeSpan.FromMilliseconds(220)))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            animation.Completed += (sender, args) =>
            {
                if (version != _selectionAnimationVersion || !ReferenceEquals(_selectedLayer, layer))
                    return;

                layer.BeginAnimation(OpacityProperty, null);
                layer.Opacity = target;
            };
            layer.BeginAnimation(OpacityProperty, animation);
        }

        static DefaultMainMenuTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DefaultMainMenuTabItem), new FrameworkPropertyMetadata(typeof(DefaultMainMenuTabItem)));
        }
    }
}

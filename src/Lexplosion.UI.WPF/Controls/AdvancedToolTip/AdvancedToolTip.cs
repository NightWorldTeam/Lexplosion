using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.Controls
{
    public enum Orientation
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public sealed class AdvancedToolTip : ToolTip
    {

        #region Dependency Properties


        public static readonly DependencyProperty OrintationProperty
            = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(AdvancedToolTip),
                new FrameworkPropertyMetadata(propertyChangedCallback: OnOrintationChanged));

        public static readonly DependencyProperty TextKeyProperty
            = DependencyProperty.Register("TextKey", typeof(string), typeof(AdvancedToolTip),
            new FrameworkPropertyMetadata(propertyChangedCallback: OnTextKeyChanged));

        public Orientation Orientation
        {
            get => (Orientation)base.GetValue(Controls.AdvancedToolTip.OrintationProperty);
            set => SetValue(TextKeyProperty, value);
        }

        public string TextKey
        {
            get => (string)GetValue(TextKeyProperty);
            set => SetValue(TextKeyProperty, value);
        }

        #endregion Dependency Properties


        #region Constructors


        static AdvancedToolTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AdvancedToolTip), new FrameworkPropertyMetadata(typeof(AdvancedToolTip)));
        }

        public AdvancedToolTip()
        {
            Loaded += (object sender, RoutedEventArgs e) =>
            {

                var parent = Parent as UIElement;
                PlacementTarget = parent;
                var win = Window.GetWindow(this);

                var top = win.Top;
                var left = win.Left;
                var right = win.Left + win.Width;
                var bottom = win.Top + win.Height;
            };
        }


        public override void OnApplyTemplate()
        {
            //_body = Template.FindName(PART_BODY_GRID, this) as Grid;
            base.OnApplyTemplate();
        }

        #endregion Constructors


        #region Private Methods


        private static void OnOrintationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var advancedToolTip = d as AdvancedToolTip;
            switch (advancedToolTip.Orientation)
            {
                case Orientation.Left:
                    {

                    }
                    break;
                case Orientation.Right:
                    {

                    }
                    break;
                case Orientation.Top:
                    {

                    }
                    break;
                case Orientation.Bottom:
                    {

                    }
                    break;
            }
        }


        private static void OnTextKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }


        #endregion Private Methods
    }
}

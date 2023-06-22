using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Lexplosion.Controls
{
    [TemplatePart(Name = PART_POPUP_NAME, Type = typeof(Popup))]
    [TemplatePart(Name = PART_TOGGLE_NAME, Type = typeof(Control))]
    internal class DropdownMenu : ContentControl
    {
        private const string PART_POPUP_NAME = "PART_Popup";
        private const string PART_TOGGLE_NAME = "PART_Toggle";

        private Popup _popup;
        private CheckBox _toggle;
        private Control _control;

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(DropdownMenu), new PropertyMetadata(false));

        public static readonly DependencyProperty IsInfoBoxProperty
            = DependencyProperty.Register("IsInfoBox", typeof(bool), typeof(DropdownMenu), new PropertyMetadata(false));

        public static readonly DependencyProperty PopupMaxWidthProperty =
            DependencyProperty.Register("PopupMaxWidth", typeof(double), typeof(DropdownMenu),
                new FrameworkPropertyMetadata(Double.PositiveInfinity,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(OnTransformDirty)),
                    new ValidateValueCallback(IsMaxWidthHeightValid));

        public static readonly DependencyProperty PopupMinWidthProperty =
            DependencyProperty.Register("PopupMinWidth", typeof(double), typeof(DropdownMenu),
                new FrameworkPropertyMetadata(Double.PositiveInfinity,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(OnTransformDirty)),
                    new ValidateValueCallback(IsMaxWidthHeightValid));

        public static readonly DependencyProperty PopupMaxHeightProperty =
            DependencyProperty.Register("PopupMaxHeight", typeof(double), typeof(DropdownMenu),
                    new FrameworkPropertyMetadata(Double.PositiveInfinity,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(OnTransformDirty)),
                    new ValidateValueCallback(IsMaxWidthHeightValid));

        public static readonly DependencyProperty PopupMinHeightProperty =
            DependencyProperty.Register("PopupMinHeight", typeof(double), typeof(DropdownMenu),
                    new FrameworkPropertyMetadata(Double.PositiveInfinity,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(OnTransformDirty)),
                    new ValidateValueCallback(IsMaxWidthHeightValid));

        public static readonly DependencyProperty ButtonTemplateProperty
            = DependencyProperty.Register("ButtonTemplate", typeof(ControlTemplate), typeof(DropdownMenu), new PropertyMetadata());

        public static readonly DependencyProperty PopupPlacementProperty
            = DependencyProperty.Register("PopupPlacement", typeof(PlacementMode), typeof(DropdownMenu), new PropertyMetadata(PlacementMode.Left));

        public static readonly DependencyProperty PopupStaysOpenProperty
            = DependencyProperty.Register("PopupStaysOpen", typeof(bool), typeof(DropdownMenu), new PropertyMetadata(false));

        public static readonly DependencyProperty PopupVerticalOffsetProperty
            = DependencyProperty.Register("PopupVerticalOffset", typeof(double), typeof(DropdownMenu), new PropertyMetadata(-5d));

        public static readonly DependencyProperty PopupHorizontalOffsetProperty
            = DependencyProperty.Register("PopupHorizontalOffset", typeof(double), typeof(DropdownMenu), new PropertyMetadata(0d));

        public PlacementMode PopupPlacement
        {
            get => (PlacementMode)GetValue(PopupPlacementProperty);
            set => SetValue(PopupPlacementProperty, value);
        }

        public bool PopupStaysOpen
        {
            get => (bool)GetValue(PopupStaysOpenProperty);
            set => SetValue(PopupStaysOpenProperty, value);
        }

        public double PopupVerticalOffset
        {
            get => (double)GetValue(PopupVerticalOffsetProperty);
            set => SetValue(PopupVerticalOffsetProperty, value);
        }

        public double PopupHorizontalOffset
        {
            get => (double)GetValue(PopupHorizontalOffsetProperty);
            set => SetValue(PopupHorizontalOffsetProperty, value);
        }

        public ControlTemplate ButtonTemplate
        {
            get => (ControlTemplate)GetValue(ButtonTemplateProperty);
            set => SetValue(ButtonTemplateProperty, value);
        }

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public bool IsInfoBox
        {
            get => (bool)GetValue(IsInfoBoxProperty);
            set => SetValue(IsInfoBoxProperty, value);
        }

        public double PopupMaxWidth
        {
            get => (double)GetValue(PopupMaxWidthProperty);
            set => SetValue(PopupMaxWidthProperty, value);
        }

        public double PopupMinWidth
        {
            get => (double)GetValue(PopupMinWidthProperty);
            set => SetValue(PopupMinWidthProperty, value);
        }

        public double PopupMaxHeight
        {
            get => (double)GetValue(PopupMaxHeightProperty);
            set => SetValue(PopupMaxHeightProperty, value);
        }

        public double PopupMinHeight
        {
            get => (double)GetValue(PopupMinHeightProperty);
            set => SetValue(PopupMinHeightProperty, value);
        }

        static DropdownMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropdownMenu), new FrameworkPropertyMetadata(typeof(DropdownMenu)));
        }

        public override void OnApplyTemplate()
        {
            _popup = Template.FindName(PART_POPUP_NAME, this) as Popup;

            if (_popup != null)
                _popup.Closed += _popup_Closed;

            if (!IsInfoBox)
            {
                _toggle = Template.FindName(PART_TOGGLE_NAME, this) as CheckBox;
                _control = _toggle;
            }
            else
            {
                _control = Template.FindName(PART_TOGGLE_NAME, this) as Control;
                _control.MouseEnter += OnMouseEnterChanged;
                _control.MouseLeave += OnMouseLeaveChanged;
            }

            base.OnApplyTemplate();
        }

        private void OnMouseLeaveChanged(object sender, MouseEventArgs e)
        {
            IsOpen = false;
        }

        private void OnMouseEnterChanged(object sender, MouseEventArgs e)
        {
            IsOpen = true;
        }

        private void _popup_Closed(object sender, EventArgs e)
        {
            if (_toggle != null)
                if (!_toggle.IsMouseOver)
                    IsOpen = false;
        }

        private static bool IsMaxWidthHeightValid(object value)
        {
            double v = (double)value;
            return (!double.IsNaN(v) && v >= 0.0d);
        }

        private static void OnTransformDirty(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Callback for MinWidth, MaxWidth, Width, MinHeight, MaxHeight, Height, and RenderTransformOffset
            //FrameworkElement fe = (FrameworkElement)d;
            //fe.AreTransformsClean = false;
        }
    }
}

﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.Controls
{
    [TemplatePart(Name = PART_POPUP_NAME, Type = typeof(Popup))]
    [TemplatePart(Name = PART_TOGGLE_NAME, Type = typeof(Control))]
    public class DropdownMenu : ContentControl
    {
        public event Action<DropdownMenu> PopupOpenedEvent;

        private const string PART_POPUP_NAME = "PART_Popup";
        private const string PART_TOGGLE_NAME = "PART_Toggle";

        private Popup _popup;
        private CheckBox _toggle;
        private Control _control;


        #region Properties


        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(DropdownMenu), new PropertyMetadata(false));

        public static readonly DependencyProperty IsInfoBoxProperty
            = DependencyProperty.Register(nameof(IsInfoBox), typeof(bool), typeof(DropdownMenu), new PropertyMetadata(false));

        public static readonly DependencyProperty PopupMaxWidthProperty =
            DependencyProperty.Register(nameof(PopupMaxWidth), typeof(double), typeof(DropdownMenu),
                new FrameworkPropertyMetadata(Double.PositiveInfinity,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(OnTransformDirty)),
                    new ValidateValueCallback(IsMaxWidthHeightValid));

        public static readonly DependencyProperty PopupMinWidthProperty =
            DependencyProperty.Register(nameof(PopupMinWidth), typeof(double), typeof(DropdownMenu),
                new FrameworkPropertyMetadata(Double.PositiveInfinity,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(OnTransformDirty)),
                    new ValidateValueCallback(IsMaxWidthHeightValid));

        public static readonly DependencyProperty PopupMaxHeightProperty =
            DependencyProperty.Register(nameof(PopupMaxHeight), typeof(double), typeof(DropdownMenu),
                    new FrameworkPropertyMetadata(Double.PositiveInfinity,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(OnTransformDirty)),
                    new ValidateValueCallback(IsMaxWidthHeightValid));

        public static readonly DependencyProperty PopupMinHeightProperty =
            DependencyProperty.Register(nameof(PopupMinHeight), typeof(double), typeof(DropdownMenu),
                    new FrameworkPropertyMetadata(Double.PositiveInfinity,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    new PropertyChangedCallback(OnTransformDirty)),
                    new ValidateValueCallback(IsMaxWidthHeightValid));

        public static readonly DependencyProperty ButtonTemplateProperty
            = DependencyProperty.Register(nameof(ButtonTemplate), typeof(ControlTemplate), typeof(DropdownMenu),
                new FrameworkPropertyMetadata());

        public static readonly DependencyProperty PopupPlacementProperty
            = DependencyProperty.Register(nameof(PopupPlacement), typeof(PlacementMode), typeof(DropdownMenu),
                new FrameworkPropertyMetadata(defaultValue: PlacementMode.Left));

        public static readonly DependencyProperty PopupStaysOpenProperty
            = DependencyProperty.Register(nameof(PopupStaysOpen), typeof(bool), typeof(DropdownMenu), new
                FrameworkPropertyMetadata(defaultValue: false));

        public static readonly DependencyProperty PopupVerticalOffsetProperty
            = DependencyProperty.Register(nameof(PopupVerticalOffset), typeof(double), typeof(DropdownMenu),
                new FrameworkPropertyMetadata(defaultValue: -5d));

        public static readonly DependencyProperty PopupHorizontalOffsetProperty
            = DependencyProperty.Register(nameof(PopupHorizontalOffset), typeof(double), typeof(DropdownMenu),
                new FrameworkPropertyMetadata(defaultValue: 0d));

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


        #endregion Properties


        #region Constructors


        static DropdownMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropdownMenu), new FrameworkPropertyMetadata(typeof(DropdownMenu)));
        }


        #endregion Constructors


        #region Public Methods


        public override void OnApplyTemplate()
        {
            _popup = Template.FindName(PART_POPUP_NAME, this) as Popup;

            if (_popup != null)
                _popup.Closed += _popup_Closed;

            if (!IsInfoBox)
            {
                _toggle = Template.FindName(PART_TOGGLE_NAME, this) as CheckBox;
                _toggle.Checked += (s, e) => 
                {
                    IsOpen = true; 
                    PopupOpenedEvent?.Invoke(this);
                };
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


        #endregion Public Methods


        #region Private Methods


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


        #endregion Private Methods
    }
}

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

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
    }
}

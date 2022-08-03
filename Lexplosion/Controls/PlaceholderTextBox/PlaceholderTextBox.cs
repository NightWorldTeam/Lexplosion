using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.Controls
{
    public class PlaceholderTextBox : TextBox
    {
        // TODO: сделать потом полную настройку placeholder. [margin, padding, foreground, etc].

        public static readonly DependencyProperty PlaceholderProperty
            = DependencyProperty.Register("Placeholder", typeof(string), typeof(PlaceholderTextBox), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty PlaceholderOpacityProperty
            = DependencyProperty.Register("PlaceholderOpacity", typeof(double), typeof(PlaceholderTextBox), new UIPropertyMetadata(0.5d));

        public static readonly DependencyPropertyKey IsEmptyPropertyKey
            = DependencyProperty.RegisterReadOnly("IsEmpty", typeof(bool), typeof(PlaceholderTextBox), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

        public string Placeholder 
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public double PlaceholderOpacity 
        {
            get => (double)GetValue(PlaceholderOpacityProperty);
            set => SetValue(PlaceholderOpacityProperty, value);
        }

        /// <summary>
        /// Скрываем [private set => ...], чтобы нельзя было изменить значение вне класса.
        /// </summary>
        public bool IsEmpty 
        {
            get => (bool)GetValue(IsEmptyProperty);
            private set => SetValue(IsEmptyPropertyKey, value);
        }

        static PlaceholderTextBox() 
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PlaceholderTextBox), new FrameworkPropertyMetadata(typeof(PlaceholderTextBox)));
        }

        protected override void OnInitialized(EventArgs e)
        {
            UpdateIsEmpty();
            base.OnInitialized(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            UpdateIsEmpty();
            base.OnTextChanged(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
                IsEmpty = false;
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
                IsEmpty = true;
            base.OnLostFocus(e);
        }

        /// <summary>
        /// Проверяет, содержит ли TextBox текст.
        /// Нужно, чтобы если TextBox содержит текст, то не показывать Placeholder.
        /// </summary>
        private void UpdateIsEmpty()
        {
            IsEmpty = string.IsNullOrEmpty(Text);
        }
    }
}

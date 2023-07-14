using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.Controls
{
    public class UpperPlaceholderTextBox : TextBox
    {
        public static readonly DependencyProperty PlaceholderProperty
            = DependencyProperty.Register("Placeholder", typeof(string), typeof(UpperPlaceholderTextBox), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyPropertyKey IsEmptyPropertyKey
            = DependencyProperty.RegisterReadOnly("IsEmpty", typeof(bool), typeof(UpperPlaceholderTextBox), new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

        public static readonly DependencyProperty IsErrorProperty
            = DependencyProperty.Register("IsError", typeof(bool), typeof(UpperPlaceholderTextBox), new FrameworkPropertyMetadata(false));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        /// <summary>
        /// Скрываем [private set => ...], чтобы нельзя было изменить значение вне класса.
        /// </summary>
        public bool IsEmpty
        {
            get => (bool)GetValue(IsEmptyProperty);
            private set => SetValue(IsEmptyPropertyKey, value);
        }

        public bool IsError 
        {
            get => (bool)GetValue(IsErrorProperty);
            set => SetValue(IsErrorProperty, value);
        }

        static UpperPlaceholderTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UpperPlaceholderTextBox), new FrameworkPropertyMetadata(typeof(UpperPlaceholderTextBox)));
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
            UpdateIsEmpty();
            base.OnGotFocus(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            UpdateIsEmpty();
            base.OnLostFocus(e);
        }

        /// <summary>
        /// Проверяет, содержит ли TextBox текст.
        /// Нужно, чтобы если TextBox содержит текст, то не показывать PlaceholderKey.
        /// </summary>
        private void UpdateIsEmpty()
        {
            IsEmpty = string.IsNullOrEmpty(Text);
        }
    }
}

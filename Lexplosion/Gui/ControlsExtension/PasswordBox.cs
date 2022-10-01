using System.Windows;

namespace Lexplosion.Gui.Extension
{
    public static class PasswordBox
    {
        public static readonly DependencyProperty PasswordProperty 
            = DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordBox), new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        public static readonly DependencyProperty AttachProperty 
            = DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordBox), new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty 
            = DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBox));

        //IsPasswordEmpty
        public static readonly DependencyPropertyKey IsEmptyPasswordPropertyKey
            = DependencyProperty.RegisterAttachedReadOnly("IsEmptyPassword", typeof(bool), typeof(PasswordBox), new PropertyMetadata(false));

        public static readonly DependencyProperty IsEmptyPasswordProperty = IsEmptyPasswordPropertyKey.DependencyProperty;

        public static readonly DependencyProperty IsPassowordSavedProperty 
            = DependencyProperty.Register("IsPasswordSaved", typeof(bool), typeof(PasswordBox));

        public static void SetIsPasswordSaved(DependencyObject dp, bool value)
        {
            dp.SetValue(IsPassowordSavedProperty, value);
        }

        public static bool GetIsPasswordSaved(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsPassowordSavedProperty);
        }

        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        public static string GetPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(PasswordProperty);
        }

        public static void SetPassword(DependencyObject dp, string value)
        {
            dp.SetValue(PasswordProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        private static void SetIsEmptyPassword(System.Windows.Controls.PasswordBox dp) 
        {
            dp.SetValue(IsEmptyPasswordPropertyKey, dp.SecurePassword.Length == 0);
        }

        public static bool GetIsEmptyPassword(DependencyObject dp) 
        {
            return (bool)dp.GetValue(IsEmptyPasswordProperty);
        }

        private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Controls.PasswordBox passwordBox = sender as System.Windows.Controls.PasswordBox;
            passwordBox.PasswordChanged -= PasswordChanged;

            if (!(bool)GetIsUpdating(passwordBox))
            {
                passwordBox.Password = (string)e.NewValue;
            }
            passwordBox.PasswordChanged += PasswordChanged;

            SetIsEmptyPassword(passwordBox);
        }

        private static void Attach(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Controls.PasswordBox passwordBox = sender as System.Windows.Controls.PasswordBox;

            if (passwordBox == null)
                return;

            if ((bool)e.OldValue)
            {
                passwordBox.PasswordChanged -= PasswordChanged;
            }

            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordChanged;
            }
            SetIsEmptyPassword(passwordBox);
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.PasswordBox passwordBox = sender as System.Windows.Controls.PasswordBox;
            SetIsUpdating(passwordBox, true);
            SetPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }

    }
}

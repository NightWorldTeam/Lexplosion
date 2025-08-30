using System;
using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.WPF.NewInterface.WindowComponents.Header.Variants
{
    public abstract class HeaderBase : UserControl
    {
        public event Action<bool> IsNotificationOpenedChanged;


        private WindowHeaderArgs _windowHeaderArgs;


        #region Constructors


        protected HeaderBase()
        {
            DataContextChanged += OnDataContextChanged;
        }


        #endregion Constructors


        #region Public Methods


        public void Close(object sender, RoutedEventArgs e)
        {
            _windowHeaderArgs.Close();
        }

        public void Maximized(object sender, RoutedEventArgs e)
        {
            _windowHeaderArgs.Maximized();
        }

        public void Minimazed(object sender, RoutedEventArgs e)
        {
            _windowHeaderArgs.Minimized();
        }

        protected void NotificationOpenedChanged(bool value) 
        {
            IsNotificationOpenedChanged?.Invoke(value);
        }

        public abstract void ChangeOrintation();


        #endregion Public Methods


        #region Private Method


        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            _windowHeaderArgs = (WindowHeaderArgs)DataContext;
        }


        #endregion Private Method
    }
}

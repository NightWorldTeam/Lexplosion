using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lexplosion.WPF.NewInterface.WindowComponents.Header.Variants
{
    public abstract class HeaderBase : UserControl
    {
        private WindowHeaderArgs _windowHeaderArgs;


        protected HeaderBase()
        {
            DataContextChanged += OnDataContextChanged;
        }

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

        public abstract void ChangeOrintation(object sender, MouseButtonEventArgs e);


        #region Private Method


        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            _windowHeaderArgs = (WindowHeaderArgs)DataContext;
        }


        #endregion Private Method
    }
}

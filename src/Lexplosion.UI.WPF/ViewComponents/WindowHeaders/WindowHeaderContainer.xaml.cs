using System.Windows;
using System.Windows.Controls;

namespace Lexplosion.UI.WPF.WindowComponents.Header
{
    /// <summary>
    /// Interaction logic for WindowHeader.xaml
    /// </summary>
    public partial class WindowHeaderContainer : UserControl
    {
        #region Dependency Properties


        public static readonly DependencyProperty IsNotificationsOpenedProperty
            = DependencyProperty.Register(nameof(IsNotificationsOpened), typeof(bool), typeof(WindowHeaderContainer),
                new FrameworkPropertyMetadata(
                    defaultValue: false));

        public static readonly DependencyProperty HeaderTypeProperty
            = DependencyProperty.Register(nameof(HeaderType), typeof(string), typeof(WindowHeaderContainer),
                new FrameworkPropertyMetadata(defaultValue: string.Empty));

        public string HeaderType
        {
            get => (string)GetValue(HeaderTypeProperty);
            set => SetValue(HeaderTypeProperty, value);
        }

        public bool IsNotificationsOpened
        {
            get => (bool)GetValue(IsNotificationsOpenedProperty);
            set => SetValue(IsNotificationsOpenedProperty, value);
        }


        #endregion Dependency Properties


        public WindowHeaderContainer()
        {
            InitializeComponent();

        }

        private void NotificationOpenedChanged(bool obj)
        {
            IsNotificationsOpened = obj;
        }
    }
}

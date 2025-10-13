using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.ViewComponents.WindowHeaders
{
    /// <summary>
    /// Interaction logic for HeaderAddtionalFuncsPanelVIew.xaml
    /// </summary>
    public partial class HeaderAddtionalFuncsPanelView : UserControl
    {
        public event Action ChangedOrintation;
        public event Action<bool> NotificationsButtonState;

        public HeaderAddtionalFuncsPanelView()
        {
            InitializeComponent();
            NotificationsToggleButton.Click += NotificationsToggleButtonClicked;
        }

        private void NotificationsToggleButtonClicked(object sender, RoutedEventArgs e)
        {
            NotificationsButtonState.Invoke((sender as ToggleButton).IsChecked.Value);
        }

        private void ChangeOrintation(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (HorizontalAlignment == System.Windows.HorizontalAlignment.Left)
            {
                AddtionalFuncs.RenderTransform = new RotateTransform(0);
            }
            else
            {
                AddtionalFuncs.RenderTransform = new RotateTransform(360);
            }

            ChangedOrintation.Invoke();
        }
    }
}

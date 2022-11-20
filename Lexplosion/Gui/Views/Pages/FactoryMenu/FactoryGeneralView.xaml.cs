using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lexplosion.Gui.Views.Pages.FactoryMenu
{
    /// <summary>
    /// Логика взаимодействия для FactoryGeneralView.xaml
    /// </summary>
    public partial class FactoryGeneralView : UserControl
    {
        public FactoryGeneralView()
        {
            InitializeComponent();
            this.Opacity = 0.0;
            this.Visibility = Visibility.Visible;
            DoubleAnimation doubleAnimation = new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.4)
            };
            this.BeginAnimation(FrameworkElement.OpacityProperty, doubleAnimation);
        }

        private void RadioButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ScrollBottom();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            ScrollBottom();
        }

        private void ScrollBottom()
        {
            try
            {
                scroll.ScrollToBottom();
            }
            catch { }
        }
    }
}
